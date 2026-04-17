using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    // ==================== STATE MACHINE ====================
    public enum EnemyState { Patrol, Idle, Aggro, Chase, Attack, Die }
    private EnemyState currentState = EnemyState.Patrol;

    [Header("Настройки ИИ")]
    public Transform player;              
    public float chaseRange = 15f;        
    public float attackRange = 2f;        
    public float aggroDelay = 1.0f;

    [Header("Патруль")]
    [SerializeField] private Transform[] patrolPoints;    // Точки патруля
    private int currentPatrolIndex = 0;
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float patrolStopDistance = 1f;
    [SerializeField] private float randomWaitTime = 2f;
    private float patrolWaitTimer = 0f;
    private bool patrolDestinationSet = false;
    private bool hasStartedMoving = false;

    [Header("Стелс система")]
    [SerializeField] private float visionRange = 15f;     // Дальность видимости
    [SerializeField] private LayerMask visionObstacles;   // Слои что блокируют видимость (стены)
    [SerializeField] private float visionAngle = 90f;     // Угол видимости (90° = полусфера)
    private bool canSeePlayer = false;
    private Vector3 lastKnownPlayerPosition;

    [Header("Настройки Атаки")]
    public float attackDamage = 20f;      
    public float attackCooldown = 1.5f;   
    private float nextAttackTime = 0f;

    [Header("Анимации")]
    private Animator animator;

    private NavMeshAgent agent;
    private PlayerHealth playerHealth;
    private EnemyHealth myHealth;
    private float aggroTimer = 0f;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        myHealth = GetComponent<EnemyHealth>();
        animator = GetComponent<Animator>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        // Если нет патрольных точек - создаем 1 на месте врага
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogWarning("⚠️ Патрольные точки не назначены! Враг будет стоять на месте.");
            patrolPoints = new Transform[] { transform };
        }

        currentState = EnemyState.Patrol;
    }

    private void Update()
    {
        if (player == null || playerHealth == null || agent == null) return;

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning("⚠️ Враг не на NavMesh!");
            return;
        }

        // ==================== ПРОВЕРКА ВИДИМОСТИ (СТЕЛС) ====================
        canSeePlayer = CanSeePlayer();
        
        // ДЕБАГ: логируем текущее состояние каждые 60 кадров
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"🧟 State: {currentState}, canSeePlayer: {canSeePlayer}, agent.velocity: {agent.velocity.magnitude}");
        }

        // STATE MACHINE
        switch (currentState)
        {
            case EnemyState.Patrol:
                UpdatePatrol();
                break;
            case EnemyState.Idle:
                UpdateIdle();
                break;
            case EnemyState.Aggro:
                UpdateAggro();
                break;
            case EnemyState.Chase:
                UpdateChase();
                break;
            case EnemyState.Attack:
                UpdateAttack();
                break;
            case EnemyState.Die:
                return;
        }
    }

    // ==================== СТЕЛС: ВИДИМОСТЬ ИГРОКА ====================
    private bool CanSeePlayer()
    {
        if (player == null) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > visionRange) return false;

        StealthSystem stealth = player.GetComponent<StealthSystem>();
        bool playerInStealth = stealth != null && stealth.IsStealth();

        if (playerInStealth)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            if (angleToPlayer > visionAngle / 2f) return false;
        }

        // Исключаем слой самого игрока из проверки стен
        LayerMask obstaclesOnly = visionObstacles & ~(1 << player.gameObject.layer);
        
        Vector3 direction = (player.position - transform.position).normalized;
        Ray ray = new Ray(transform.position + Vector3.up, direction);
        if (Physics.Raycast(ray, distanceToPlayer, obstaclesOnly))
            return false;

        return true;
    }

    // ==================== STATE: PATROL ====================
    private void UpdatePatrol()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (canSeePlayer && distanceToPlayer <= chaseRange)
        {
            TransitionToState(EnemyState.Aggro);
            return;
        }

        if (!patrolDestinationSet)
        {
            agent.isStopped = false;
            agent.speed = patrolSpeed;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            patrolDestinationSet = true;
            hasStartedMoving = false; // сбрасываем — ждём разгона
            if (animator != null) animator.SetBool("isMoving", true);
            return;
        }

        // Фиксируем что агент реально пошёл
        if (agent.velocity.magnitude > 0.3f)
            hasStartedMoving = true;

        float distToPoint = Vector3.Distance(transform.position, patrolPoints[currentPatrolIndex].position);
        bool arrived = distToPoint <= patrolStopDistance;
        bool stuck = agent.pathStatus == NavMeshPathStatus.PathPartial
                  || agent.pathStatus == NavMeshPathStatus.PathInvalid;
        // agentStopped только если уже двигался раньше
        bool agentStopped = hasStartedMoving
                            && !agent.pathPending
                            && agent.velocity.magnitude < 0.1f;

        if (arrived || stuck || agentStopped)
        {
            agent.isStopped = true;
            if (animator != null) animator.SetBool("isMoving", false);

            patrolWaitTimer += Time.deltaTime;
            Debug.Log($"⏳ Ждём у точки {currentPatrolIndex} | таймер: {patrolWaitTimer:F1}/{randomWaitTime} | arrived:{arrived} stuck:{stuck} agentStopped:{agentStopped} | dist:{distToPoint:F2}");

            if (patrolWaitTimer >= randomWaitTime)
            {
                int oldIndex = currentPatrolIndex;
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                patrolWaitTimer = 0f;
                patrolDestinationSet = false;
                hasStartedMoving = false;
                Debug.Log($"➡️ Точка {oldIndex} → {currentPatrolIndex}");
            }
        }
    }

    // ==================== STATE: IDLE ====================
    private void UpdateIdle()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (canSeePlayer && distanceToPlayer <= chaseRange)
        {
            TransitionToState(EnemyState.Aggro);
            Debug.Log($"🧟 Враг заметил игрока! AGGRO");
        }

        agent.isStopped = true;
    }

    // ==================== STATE: AGGRO ====================
    private void UpdateAggro()
    {
        agent.isStopped = true;

        aggroTimer += Time.deltaTime;
        if (aggroTimer >= aggroDelay)
        {
            TransitionToState(EnemyState.Chase);
            Debug.Log($"🧟 Враг встал! CHASE");
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (!canSeePlayer || distanceToPlayer > chaseRange * 1.5f)
        {
            TransitionToState(EnemyState.Patrol);
            Debug.Log($"🧟 Враг потерял игрока! PATROL");
        }
    }

    // ==================== STATE: CHASE ====================
    private void UpdateChase()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            TransitionToState(EnemyState.Attack);
            Debug.Log($"🧟 Враг найти игрока! ATTACK");
        }
        else if (!canSeePlayer || distanceToPlayer > chaseRange * 1.5f)
        {
            // НОВОЕ: идём к последней известной позиции, а не сразу в Patrol
            agent.isStopped = false;
            agent.speed = 5.5f;
            agent.SetDestination(lastKnownPlayerPosition);

            float distToLastKnown = Vector3.Distance(transform.position, lastKnownPlayerPosition);
            if (distToLastKnown < 1.5f) // дошли — возвращаемся в патруль
            {
                TransitionToState(EnemyState.Patrol);
                Debug.Log("🧟 Враг потерял игрока! PATROL");
            }
        }
        else
        {
            lastKnownPlayerPosition = player.position; // запоминаем
            agent.isStopped = false;
            agent.speed = 5.5f;
            agent.SetDestination(player.position);
            if (animator != null) animator.SetBool("isMoving", true);
        }
    }

    // ==================== STATE: ATTACK ====================
    private void UpdateAttack()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > attackRange * 1.5f)
        {
            TransitionToState(EnemyState.Chase);
            Debug.Log($"🧟 Враг потерял цель! CHASE");
            return;
        }

        agent.isStopped = true;

        // Повернуться к игроку
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        if (Time.time >= nextAttackTime)
        {
            Debug.Log("💥 Враг кусает Ноа!");
            playerHealth.TakeDamage(attackDamage);
            nextAttackTime = Time.time + attackCooldown;
            
            if (animator != null) animator.SetTrigger("Attack");
        }
    }

    // ==================== ПЕРЕХОДЫ СОСТОЯНИЙ ====================
    private void TransitionToState(EnemyState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        aggroTimer = 0f;
        patrolDestinationSet = false;

        if (animator != null)
        {
            animator.SetBool("isMoving", newState == EnemyState.Chase || newState == EnemyState.Patrol);
        }
    }

    // ==================== СМЕРТЬ ====================
    public void SetDead()
    {
        TransitionToState(EnemyState.Die);
        Debug.Log("💀 Враг мертв! DIE");
    }

    private void OnDrawGizmosSelected()
    {
        // Видимость
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        // Дальность атаки
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Дальность погони
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // Патрольные точки
        if (patrolPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (Transform point in patrolPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawSphere(point.position, 0.3f);
                    Gizmos.DrawLine(transform.position, point.position);
                }
            }
        }
    }
}
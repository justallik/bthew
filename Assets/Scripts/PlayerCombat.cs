using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Компоненты")]
    public PlayerInputHandler inputHandler;
    public Camera playerCamera;
    public PlayerMovement playerMovement;

    [Header("Настройки боевки")]
    public float attackRange = 2.5f;
    public float attackCooldown = 0.6f;
    public float superAttackCooldown = 3.0f; 
    public float dodgeStaminaCost = 25f;

    [Header("Настройки блока")]
    [SerializeField] private float maxBlockDuration = 2.0f; // Блок длится максимум 2 секунды
    private float currentBlockTimer = 0f;
    private bool canBlockAgain = true; // Чтобы нельзя было спамить блок, не отпуская кнопку

    private float lastAttackTime = 0f;
    private float lastSuperAttackTime = 0f;
    
    [HideInInspector] public bool isBlocking = false;

    void Start()
    {
        if (inputHandler == null) inputHandler = GetComponent<PlayerInputHandler>();
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
        if (playerCamera == null) playerCamera = Camera.main;
    }

    void Update()
    {
        if (inputHandler == null || playerMovement == null) return;

        // 1. УКЛОНЕНИЕ (Всегда работает)
        CheckCrouchingDodgeInput();

        // 2. ОБНОВЛЕННАЯ ЛОГИКА БЛОКА
        HandleBlockLogic();

        // --- ПРОВЕРКА ОРУЖИЯ ДЛЯ АТАК ---
        bool hasWeapon = EquipmentManager.instance != null && 
                         EquipmentManager.instance.isEquipped && 
                         EquipmentManager.instance.currentEquippedItem != null;

        if (!hasWeapon) return; 

        // 3. ОБЫЧНЫЙ УДАР (ЛКМ)
        if (inputHandler.AttackInput && Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack();
        }

        // 4. СУПЕР УДАР (Клавиша V)
        if (Keyboard.current.vKey.wasPressedThisFrame && Time.time >= lastSuperAttackTime + superAttackCooldown)
        {
            PerformSuperAttack();
        }
    }

    private void PerformAttack()
    {
        lastAttackTime = Time.time;
        ItemData weaponData = EquipmentManager.instance.currentEquippedItem;

        Debug.Log("🗡 Обычный взмах ножом");
        PlayWeaponAnimation("Attack");
        
        // ЛОМАЕМ СТЕЛС при атаке
        StealthSystem stealth = GetComponent<StealthSystem>();
        if (stealth != null) stealth.BreakStealth();
        
        if (weaponData != null) CheckHit(weaponData.weaponDamage, false); 
    }

    private void CheckCrouchingDodgeInput()
    {
        if (Keyboard.current.shiftKey.isPressed)
        {
            Vector3 dodgeDir = Vector3.zero;

            if (Keyboard.current.aKey.wasPressedThisFrame) dodgeDir = -playerMovement.transform.right;
            else if (Keyboard.current.dKey.wasPressedThisFrame) dodgeDir = playerMovement.transform.right;

            if (dodgeDir != Vector3.zero)
            {
                if (playerMovement.HasEnoughStamina(dodgeStaminaCost))
                {
                    playerMovement.UseStamina(dodgeStaminaCost);
                    playerMovement.PerformCrouchingDodge(dodgeDir);
                    Debug.Log("💨 Уклонение!");
                    PlayWeaponAnimation("Dodge");
                    
                    // ✨ АКТИВИРУЕМ I-ФРЕЙМЫ
                    PlayerHealth playerHealth = GetComponent<PlayerHealth>();
                    if (playerHealth != null) playerHealth.StartIFrames();
                }
                else playerMovement.TriggerExhaustion();
            }
        }
    }

    private void HandleBlockLogic()
    {
        // Если игрок отпустил кнопку блока — разрешаем блокировать снова
        if (!inputHandler.BlockInput)
        {
            canBlockAgain = true;
            if (isBlocking) StopBlock();
            return;
        }

        // Если кнопка зажата, стамины хватает и мы не превысили лимит времени
        if (inputHandler.BlockInput && canBlockAgain && playerMovement.GetCurrentStamina() > 0)
        {
            if (!isBlocking) 
            {
                StartBlock();
            }

            // Считаем время блока
            currentBlockTimer += Time.deltaTime;

            // Тратим стамину
            float staminaCost = 15f; 
            if (EquipmentManager.instance.isEquipped && EquipmentManager.instance.currentEquippedItem != null)
            {
                staminaCost = EquipmentManager.instance.currentEquippedItem.blockStaminaCost;
            }
            playerMovement.UseStamina(staminaCost * Time.deltaTime);

            // Если время вышло — принудительно выключаем блок
            if (currentBlockTimer >= maxBlockDuration)
            {
                Debug.Log("💤 Ноа устал держать блок!");
                canBlockAgain = false; // Блокировка до тех пор, пока не перенажмет кнопку
                StopBlock();
            }
        }
        else if (isBlocking)
        {
            StopBlock();
        }
    }

    private void StartBlock()
    {
        isBlocking = true;
        currentBlockTimer = 0f; // Сброс таймера при новом нажатии
        Debug.Log("🛡 Начали блок");
    }

    private void StopBlock()
    {
        if (isBlocking)
        {
            isBlocking = false;
            currentBlockTimer = 0f;
            Debug.Log("🛡 Сняли блок");
        }
    }

    private void PerformSuperAttack()
    {
        float superCost = 40f; 
        if (playerMovement.HasEnoughStamina(superCost))
        {
            playerMovement.UseStamina(superCost);
            lastSuperAttackTime = Time.time;

            Debug.Log("💥 СУПЕР-УДАР: Нож + Кулак (V)!");
            PlayWeaponAnimation("SuperAttack");
            
            // ЛОМАЕМ СТЕЛС при супер-ударе
            StealthSystem stealth = GetComponent<StealthSystem>();
            if (stealth != null) stealth.BreakStealth();
            
            ItemData weaponData = EquipmentManager.instance.currentEquippedItem;
            if (weaponData != null) CheckHit(weaponData.weaponDamage * 2f, true);
        }
        else playerMovement.TriggerExhaustion();
    }

    private void CheckHit(float damage, bool isKnockout)
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, attackRange))
        {
            EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                if (isKnockout) enemy.ApplyKnockout(3.0f);
                Debug.Log($"🎯 Попали по врагу! Нанесен урон: {damage}");
            }
        }
    }

    private void PlayWeaponAnimation(string triggerName)
    {
        GameObject activeWeapon = EquipmentManager.instance.GetActiveWeaponObject();
        if (activeWeapon != null)
        {
            Animator anim = activeWeapon.GetComponent<Animator>();
            if (anim != null) anim.SetTrigger(triggerName);
        }
    }
}

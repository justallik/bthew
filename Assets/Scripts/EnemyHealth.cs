using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    public bool isKnockout = false;

    [Header("Ragdoll")]
    [SerializeField] private Rigidbody[] ragdollBodies;  // Все риджбоди для ragdoll

    void Start()
    {
        currentHealth = maxHealth;
        
        // Собираем все риджбоди детей для ragdoll
        ragdollBodies = GetComponentsInChildren<Rigidbody>();
        
        // По умолчанию все риджбоди отключены (враг живой и управляется AI)
        DisableRagdoll();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        
        Debug.Log($"🩸 Враг получил урон: {amount}. Осталось здоровья: {currentHealth}");
        
        if (currentHealth <= 0) Die();
    }

    public void ApplyKnockout(float duration)
    {
        if (isKnockout) return;
        StartCoroutine(KnockoutRoutine(duration));
    }

    private IEnumerator KnockoutRoutine(float duration)
    {
        isKnockout = true;
        Debug.Log("🥴 Враг в нокауте!");

        yield return new WaitForSeconds(duration);

        isKnockout = false;
        Debug.Log("🧟 Враг пришел в себя!");
    }

    void Die() 
    { 
        Debug.Log("💀 Враг повержен!");

        // 1. Переводим AI в состояние "Die"
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null) ai.SetDead();

        // 2. Отключаем NavMeshAgent
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        // 3. АКТИВИРУЕМ RAGDOLL
        EnableRagdoll();

        // 4. Уничтожаем врага через 3 секунды
        Destroy(gameObject, 3f);
    }

    // ==================== RAGDOLL ====================
    private void EnableRagdoll()
    {
        Debug.Log("💀 Включаем Ragdoll...");
        
        // Включаем все риджбоди
        foreach (Rigidbody rb in ragdollBodies)
        {
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
        }

        // Даем телу маленький импульс вверх для реалистичности
        if (ragdollBodies.Length > 0 && ragdollBodies[0] != null)
        {
            ragdollBodies[0].linearVelocity = new Vector3(0, 2f, 0);
        }
    }

    private void DisableRagdoll()
    {
        // Отключаем все риджбоди (враг управляется AI)
        foreach (Rigidbody rb in ragdollBodies)
        {
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }
    }
}

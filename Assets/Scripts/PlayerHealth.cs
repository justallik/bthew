using UnityEngine;
// using UnityEngine.SceneManagement; <- Убрали, так как сцену мы больше не перезагружаем!

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth instance;
    
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Respawn System")]
    public int currentLives = 10;      
    public Transform respawnPoint;     

    [Header("Combat Reference")]
    public PlayerCombat combatScript;

    [Header("I-фреймы (неуязвимость)")]
    [SerializeField] private float iFrameDuration = 0.5f; // На 0.5 сек неуязвим при уклонении
    private float iFrameTimer = 0f;
    public bool isInvulnerable = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        currentHealth = maxHealth;
        
        if (combatScript == null) combatScript = GetComponent<PlayerCombat>();
    }

    void Update()
    {
        // Обновляем таймер i-фреймов
        if (isInvulnerable)
        {
            iFrameTimer -= Time.deltaTime;
            if (iFrameTimer <= 0)
            {
                isInvulnerable = false;
            }
        }
    }

    // PUBLIC: Вызывается из PlayerCombat при уклонении
    public void StartIFrames()
    {
        isInvulnerable = true;
        iFrameTimer = iFrameDuration;
        Debug.Log($"✨ I-фреймы активированы на {iFrameDuration} сек");
    }

    public void TakeDamage(float amount)
    {
        // 1. ПРОВЕРКА НЕУЯЗВИМОСТИ (i-фреймы при уклонении)
        if (isInvulnerable)
        {
            Debug.Log($"🫡 Урон поглощен i-фреймами! {amount} HP не нанесено");
            return;
        }

        float finalDamage = amount;

        // 2. ЛОГИКА БЛОКА
        if (combatScript != null && combatScript.isBlocking)
        {
            float blockReduction = 0.2f; 

            if (EquipmentManager.instance != null && EquipmentManager.instance.isEquipped && EquipmentManager.instance.currentEquippedItem != null)
            {
                blockReduction = EquipmentManager.instance.currentEquippedItem.blockReduction;
            }
            
            float damageToBlock = amount * blockReduction;
            finalDamage -= damageToBlock;

            Debug.Log($"🛡 Заблокировано урона: {damageToBlock}. Итоговый урон: {finalDamage}");
        }
        else
        {
            Debug.Log($"🤕 Получено полного урона: {finalDamage}");
        }

        currentHealth -= finalDamage;

        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        currentLives--; // Отнимаем жизнь

        if (currentLives > 0)
        {
            Debug.Log($"💀 Ноа умер! Осталось жизней: {currentLives}. Возрождаемся...");
            Respawn();
        }
        else
        {
            Debug.Log("🚨 ИГРА ОКОНЧЕНА! Жизней больше нет.");
            // Тут потом добавим показ экрана Game Over
        }
    }

    private void Respawn()
    {
        // 1. Восстанавливаем ХП
        currentHealth = maxHealth;

        // 2. Телепортируем на точку спавна
        if (respawnPoint != null)
        {
            CharacterController cc = GetComponent<CharacterController>();
            
            // Отключаем контроллер, чтобы он не сопротивлялся телепортации
            if (cc != null) cc.enabled = false; 
            
            transform.position = respawnPoint.position;
            transform.rotation = respawnPoint.rotation;
            
            // Включаем контроллер обратно
            if (cc != null) cc.enabled = true;
            
            Debug.Log("✨ Ноа возродился на точке спавна. Вещи сохранены!");
        }
        else
        {
            Debug.LogError("⚠️ Точка респавна не назначена в Инспекторе!");
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        Debug.Log($"💖 Здоровье восстановлено! Текущее HP: {currentHealth}/{maxHealth}");
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercent() => (currentHealth / maxHealth) * 100f;
}

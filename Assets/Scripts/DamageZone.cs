using UnityEngine;

/// <summary>
/// Тестовая зона урона - ловушка для проверки системы здоровья
/// </summary>
public class DamageZone : MonoBehaviour
{
    [Header("Урон")]
    [SerializeField] private int damageAmount = 20;
    
    [Header("Эффекты")]
    [SerializeField] private ParticleSystem damageEffect;  // Визуальный эффект при попадании
    [SerializeField] private AudioClip damageSound;        // Звук урона (опционально)

    private void Start()
    {
        // Проверяем что это триггер
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning("⚠️ DamageZone куб ДОЛЖЕН быть триггером! Включи 'Is Trigger' в Inspector!");
        }
    }

    /// <summary>
    /// Срабатывает когда что-то входит в триггер куба
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Пытаемся найти PlayerHealth на объекте который коснулся куба
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount);
            Debug.Log($"💥 Игрок наступил в ловушку! Урон: -{damageAmount} HP");

            // Визуальный эффект если назначен
            if (damageEffect != null)
            {
                Instantiate(damageEffect, transform.position, Quaternion.identity);
            }

            // Звук если назначен
            if (damageSound != null)
            {
                AudioSource.PlayClipAtPoint(damageSound, transform.position);
            }
        }
    }

    /// <summary>
    /// Срабатывает когда что-то находится ВНУТРИ триггера
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        // Опционально: наносить урон каждый кадр пока игрок в ловушке
        // Раскомментируй если хочешь постоянный урон
        
        // PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        // if (playerHealth != null && Time.deltaTime > 0)
        // {
        //     playerHealth.TakeDamage(Mathf.CeilToInt(damageAmount * Time.deltaTime));
        // }
    }

    /// <summary>
    /// Срабатывает когда что-то выходит из триггера
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            Debug.Log("🚀 Игрок вышел из ловушки (живой!)");
        }
    }
}

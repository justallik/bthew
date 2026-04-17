using UnityEngine;
using UnityEngine.UI; // Обязательно для работы с картинками UI

/// <summary>
/// Связывает систему стамины PlayerMovement с визуальным отображением UI
/// Плавно анимирует полоску стамины
/// </summary>
public class StaminaUI : MonoBehaviour
{
    [Header("═══ ССЫЛКИ ═══")]
    public PlayerMovement playerMovement; // Ссылка на твой скрипт движения
    public Image staminaImage;            // Ссылка на картинку полоски
    
    [Header("═══ АНИМАЦИЯ ═══")]
    [SerializeField] private float fillAnimationSpeed = 5f; // Скорость плавного заполнения полоски (0-1 за N сек)

    private float targetFillAmount = 1f;

    void Update()
    {
        if (playerMovement != null && staminaImage != null)
        {
            // Вычисляем целевой процент: текущая стамина деленная на максимальную (от 0.0 до 1.0)
            targetFillAmount = playerMovement.GetCurrentStamina() / playerMovement.GetMaxStamina();
            
            // ПЛАВНО переводим полоску в целевую позицию (вместо резкого скачка)
            staminaImage.fillAmount = Mathf.Lerp(staminaImage.fillAmount, targetFillAmount, Time.deltaTime * fillAnimationSpeed);

            // БОНУС: если стамина кончилась (нельзя бегать), делаем полоску красной
            if (!playerMovement.CanSprint())
            {
                // Плавный переход в красный
                staminaImage.color = Color.Lerp(staminaImage.color, Color.red, Time.deltaTime * 3f);
            }
            else
            {
                // Плавный переход в белый полупрозрачный
                Color targetColor = new Color(1f, 1f, 1f, 0.5f);
                staminaImage.color = Color.Lerp(staminaImage.color, targetColor, Time.deltaTime * 3f);
            }
        }
    }
}

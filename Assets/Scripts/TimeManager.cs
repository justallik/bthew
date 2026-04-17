using UnityEngine;
using System.Collections;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;

    [Header("Настройки времени")]
    [Range(0, 24)] public float currentTime = 8f; // Утро по умолчанию (08:00)
    public bool isTimeRunning = false; // Выключено для 1 Акта (Скриптовое время)

    [Header("Длительность (в реальных минутах)")]
    public float dayDuration = 15f;   // Сколько длится день (если включено)
    public float nightDuration = 10f; // Сколько длится ночь (если включено)

    [Header("Светила (Перетащи сюда объекты)")]
    public Light sunLight;
    public float maxSunIntensity = 1.2f; // Максимальная яркость днем

    public Light moonLight;
    public float maxMoonIntensity = 0.3f; // Максимальная яркость ночью

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (isTimeRunning)
        {
            UpdateTime();
        }
        
        // Освещение обновляем каждый кадр (даже если время стоит на месте)
        UpdateLighting(); 
    }

    private void UpdateTime()
    {
        bool isDay = currentTime >= 6f && currentTime < 18f;
        float currentPhaseDuration = isDay ? dayDuration : nightDuration;
        float realSecondsPerInGameHour = (currentPhaseDuration * 60f) / 12f;

        currentTime += Time.deltaTime / realSecondsPerInGameHour;

        if (currentTime >= 24f) currentTime -= 24f;
    }

    private void UpdateLighting()
    {
        // 1. ВРАЩЕНИЕ (Солнце и Луна всегда друг напротив друга)
        float sunAngle = ((currentTime - 6f) / 24f) * 360f;
        float moonAngle = sunAngle + 180f;

        if (sunLight != null) sunLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);
        if (moonLight != null) moonLight.transform.rotation = Quaternion.Euler(moonAngle, 170f, 0f);

        // 2. ИНТЕНСИВНОСТЬ (Плавный переход на рассвете и закате)
        float sunTarget = 0f;
        float moonTarget = 0f;

        // Период, когда на небе есть Солнце (с 5 утра до 19 вечера)
        if (currentTime >= 5f && currentTime <= 19f)
        {
            float multiplier = 1f;
            
            // Рассвет (с 5:00 до 7:00 солнце разгорается, луна тускнеет)
            if (currentTime <= 7f) multiplier = (currentTime - 5f) / 2f; 
            // Закат (с 17:00 до 19:00 солнце тускнеет, луна разгорается)
            else if (currentTime >= 17f) multiplier = (19f - currentTime) / 2f; 

            sunTarget = maxSunIntensity * multiplier;
            moonTarget = maxMoonIntensity * (1f - multiplier);
        }
        else
        {
            // Глубокая ночь
            sunTarget = 0f;
            moonTarget = maxMoonIntensity;
        }

        // Применяем вычисленную яркость к светильникам
        if (sunLight != null) sunLight.intensity = sunTarget;
        if (moonLight != null) moonLight.intensity = moonTarget;
    }

    // --- ФУНКЦИИ ДЛЯ ТВОИХ КВЕСТОВ И КАТСЦЕН ---

    // Мгновенная установка времени (например, после сна)
    public void SetTime(float newTime)
    {
        currentTime = newTime;
        UpdateLighting();
        Debug.Log($"⏰ Время установлено на {newTime:F1}:00");
    }

    // Плавная перемотка (вызывай при завершении квестов или в кат-сценах)
    public void FastForwardTo(float targetTime, float durationInSeconds)
    {
        StartCoroutine(FastForwardRoutine(targetTime, durationInSeconds));
    }

    private IEnumerator FastForwardRoutine(float targetTime, float duration)
    {
        float startTime = currentTime;
        
        // Если мы перематываем через полночь (например, с 23:00 до 06:00 утра)
        if (targetTime < startTime) targetTime += 24f;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            
            // Плавно меняем время
            float lerpedTime = Mathf.Lerp(startTime, targetTime, timer / duration);
            
            currentTime = lerpedTime;
            if (currentTime >= 24f) currentTime -= 24f;

            // UpdateLighting вызывается в Update, поэтому всё будет крутиться само
            yield return null;
        }
        
        // Фиксируем точное время в конце
        SetTime(targetTime % 24f);
    }
}

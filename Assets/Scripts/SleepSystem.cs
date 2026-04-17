using System.Collections;
using UnityEngine;

public class SleepSystem : MonoBehaviour
{
    public static SleepSystem instance;
    public CanvasGroup fadeScreen; // Твоя черная шторка
    public float fadeDuration = 2f;

    private void Awake() => instance = this;

    public void StartSleeping()
    {
        StartCoroutine(SleepRoutine());
    }

    private System.Collections.IEnumerator SleepRoutine()
    {
        // 1. Включаем черную панель и затемняем экран
        fadeScreen.gameObject.SetActive(true);
        yield return StartCoroutine(Fade(0, 1));

        // --- ЛОГИКА СНА С ТЕНКОКУ ---
        // Ищем Тенкоку на сцене
        Tenkoku.Core.TenkokuModule tenkoku = FindFirstObjectByType<Tenkoku.Core.TenkokuModule>();
        
        if (tenkoku != null)
        {
            float startTime = tenkoku.currentHour;
            float wakeUpTime = 8f; // Просыпаемся в 8 утра
            float hoursSlept = 0f;

            // Считаем, сколько часов Ноа проспал
            if (startTime >= 22f) 
                hoursSlept = (24f - startTime) + wakeUpTime;
            else 
                hoursSlept = wakeUpTime - startTime;

            // Считаем эффективность сна для лечения (максимум 10 часов)
            float maxSleepCycle = 10f;
            float sleepEfficiency = Mathf.Clamp01(hoursSlept / maxSleepCycle);

            // Лечим Ноа
            if (PlayerHealth.instance != null)
            {
                float missingHealth = PlayerHealth.instance.maxHealth - PlayerHealth.instance.currentHealth;
                float healthToRestore = missingHealth * sleepEfficiency;
                PlayerHealth.instance.Heal(healthToRestore);
            }

            // ПЕРЕМОТКА ВРЕМЕНИ НА УТРО В ТЕНКОКУ
            tenkoku.currentHour = 8; // Убрали переменную и просто написали 8
            tenkoku.currentMinute = 0; // Убрали f
        }

        yield return new WaitForSeconds(1.5f); // Пауза в темноте для эффекта

        // 2. Осветляем экран и выключаем панель
        yield return StartCoroutine(Fade(1, 0));
        fadeScreen.gameObject.SetActive(false);
    }

    private IEnumerator Fade(float start, float end)
    {
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeScreen.alpha = Mathf.Lerp(start, end, timer / fadeDuration);
            yield return null;
        }
    }
}

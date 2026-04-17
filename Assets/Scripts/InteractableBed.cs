using UnityEngine;

public class InteractableBed : MonoBehaviour
{
    public void Interact()
    {
        // 1. Ищем скрипт Тенкоку на нашей сцене
        Tenkoku.Core.TenkokuModule tenkoku = FindFirstObjectByType<Tenkoku.Core.TenkokuModule>();
        
        if (tenkoku == null)
        {
            Debug.LogError("❌ Tenkoku не найден! Невозможно узнать время.");
            return;
        }

        // 2. Спрашиваем у него текущий час
        float time = tenkoku.currentHour; 

        // 3. Проверка: можно спать только с 22:00 вечера до 08:00 утра
        bool canSleep = (time >= 22f || time < 8f);

        if (!canSleep)
        {
            Debug.Log("☀️ Еще слишком рано для сна. Ноа не хочет спать днем.");
            return;
        }

        // 4. Если всё ок - запускаем сон!
        Debug.Log("🛏️ Ноа ложится спать...");
        SleepSystem.instance.StartSleeping();
    }
}

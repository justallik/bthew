using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [Header("Настройки предмета")]
    public ItemData itemData;

    [TextArea]
    public string noteContent;

    [Header("Дневник")]
    [Tooltip("Включи, если этот объект — 3D-модель дневника. При подборе разблокирует вкладку Дневник.")]
    public bool isDiaryObject = false;

    [Header("Квест")]
    [TextArea]
    public string questTextOnPickup; // Заполни в Inspector для нужных предметов

    public void Interact()
    {
        // ── Подбор дневника (3D-модель) ─────────────────────────────────────
        if (isDiaryObject)
        {
            if (DiaryManager.instance != null)
                DiaryManager.instance.UnlockDiary();
            else
                Debug.LogWarning("InteractableItem: DiaryManager не найден в сцене!");

            Debug.Log("📖 Дневник подобран!");
            TryUpdateQuest();
            Destroy(gameObject);
            return;
        }

        if (itemData == null)
        {
            Debug.LogError("InteractableItem: itemData is null!");
            return;
        }

        // ── Подбор записки → добавляем в дневник ────────────────────────────
        if (itemData.itemType == ItemData.ItemType.Note)
        {
            Debug.Log("📝 Записка подобрана: " + itemData.itemName);

            if (DiaryManager.instance != null)
            {
                string date = System.DateTime.Now.ToString(DiaryManager.DateFormat);
                DiaryManager.instance.AddEntry(itemData.itemName, noteContent, date);
            }
            else
            {
                Debug.LogWarning("InteractableItem: DiaryManager не найден — запись не добавлена в дневник.");
            }

            TryUpdateQuest();
            Destroy(gameObject);
            return;
        }

        // ── Обычный предмет → в инвентарь ───────────────────────────────────
        if (InventorySystemNew.instance == null)
        {
            Debug.LogError("InteractableItem: InventorySystemNew.instance is null!");
            return;
        }

        bool success = InventorySystemNew.instance.AddItem(itemData, 1);

        if (success)
        {
            Debug.Log("✅ Подобрано: " + itemData.itemName);
            TryUpdateQuest();
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("⚠️ Не удалось добавить: " + itemData.itemName);
        }
    }

    // Обновляем квест только если поле заполнено в Inspector
    private void TryUpdateQuest()
    {
        if (!string.IsNullOrEmpty(questTextOnPickup) && QuestManager.instance != null)
        {
            QuestManager.instance.UpdateQuest(questTextOnPickup);
        }
    }
}
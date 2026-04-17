using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [Header("Настройки предмета")]
    public ItemData itemData;

    [TextArea]
    public string noteContent;

    [Header("Квест")]
    [TextArea]
    public string questTextOnPickup; // Заполни в Inspector для нужных предметов

    public void Interact()
    {
        if (itemData == null)
        {
            Debug.LogError("InteractableItem: itemData is null!");
            return;
        }

        if (InventorySystemNew.instance == null)
        {
            Debug.LogError("InteractableItem: InventorySystemNew.instance is null!");
            return;
        }

        if (itemData.itemType == ItemData.ItemType.Note)
        {
            Debug.Log("📝 Читаем записку: " + noteContent);
            TryUpdateQuest(); // квест тоже можно обновить при чтении записки
            return;
        }

        bool success = InventorySystemNew.instance.AddItem(itemData, 1);

        if (success)
        {
            Debug.Log("✅ Подобрано: " + itemData.itemName);
            TryUpdateQuest(); // обновляем задание если оно задано
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
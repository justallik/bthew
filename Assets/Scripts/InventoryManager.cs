using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;
    
    [Header("Инвентарь")]
    public List<InventorySlot> inventorySlots = new List<InventorySlot>();
    public int maxSlots = 20; // Максимум разных типов предметов

    // Событие - срабатывает когда добавили предмет
    public delegate void OnInventoryChanged();
    public event OnInventoryChanged inventoryChanged;

    private void Awake()
    {
        // Синглтон - гарантируем, что на сцене только один InventoryManager
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// Добавить предмет в инвентарь
    /// </summary>
    public bool AddItem(ItemData itemData, int amount = 1)
    {
        if (itemData == null)
        {
            Debug.LogError("ItemData is null!");
            return false;
        }

        // 1. Ищем существующий слот с этим предметом
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.itemData != null && slot.itemData.itemName == itemData.itemName)
            {
                // Нашли! Пытаемся добавить в существующий слот
                int canAdd = slot.GetRemainingSpace();
                if (canAdd >= amount)
                {
                    slot.count += amount;
                    Debug.Log($"Добавлено {amount} x {itemData.itemName} (всего: {slot.count})");
                    inventoryChanged?.Invoke(); // Событие для обновления UI
                    return true;
                }
                else if (canAdd > 0)
                {
                    slot.count += canAdd;
                    amount -= canAdd;
                    Debug.Log($"Слот заполнен. Остаток: {amount}");
                    // Продолжаем цикл, может создадим новый слот
                }
            }
        }

        // 2. Если слота нет или не хватает места - создаём новый слот
        if (inventorySlots.Count < maxSlots)
        {
            int addNow = Mathf.Min(amount, itemData.maxStackSize);
            InventorySlot newSlot = new InventorySlot(itemData, addNow);
            inventorySlots.Add(newSlot);
            Debug.Log($"Новый слот: {addNow} x {itemData.itemName}");
            inventoryChanged?.Invoke(); // Событие для обновления UI

            // Если осталось больше - рекурсивно добавляем остаток
            int remaining = amount - addNow;
            if (remaining > 0)
            {
                return AddItem(itemData, remaining);
            }
            return true;
        }

        Debug.LogWarning("Инвентарь переполнен!");
        return false;
    }

    /// <summary>
    /// Удалить предмет из инвентаря
    /// </summary>
    public bool RemoveItem(string itemName, int amount = 1)
    {
        for (int i = inventorySlots.Count - 1; i >= 0; i--)
        {
            if (inventorySlots[i].itemData != null && inventorySlots[i].itemData.itemName == itemName)
            {
                if (inventorySlots[i].count >= amount)
                {
                    inventorySlots[i].count -= amount;
                    if (inventorySlots[i].count == 0)
                    {
                        inventorySlots.RemoveAt(i);
                    }
                    Debug.Log($"Удалено {amount} x {itemName}");
                    inventoryChanged?.Invoke(); // Отправляем событие об изменении
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Получить количество предмета в инвентаре
    /// </summary>
    public int GetItemCount(string itemName)
    {
        int total = 0;
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.itemData != null && slot.itemData.itemName == itemName)
            {
                total += slot.count;
            }
        }
        return total;
    }

    /// <summary>
    /// Получить все слоты инвентаря (для UI)
    /// </summary>
    public List<InventorySlot> GetAllSlots()
    {
        return inventorySlots;
    }
}

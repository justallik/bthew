using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ⚠️ НОВАЯ СИСТЕМА ИНВЕНТАРЯ - с отдельными контейнерами для оружия и лута
/// </summary>
public class InventorySystemNew : MonoBehaviour
{
    public static InventorySystemNew instance;
    
    [Header("МАЛЫЕ СЛОТЫ (мелкий лут)")]
    public List<InventorySlot> smallSlots = new List<InventorySlot>();
    public int maxSmallSlots = 12;

    [Header("БОЛЬШИЕ СЛОТЫ (оружие)")]
    public List<InventorySlot> weaponSlots = new List<InventorySlot>();
    public int maxWeaponSlots = 3;

    public delegate void OnInventoryChanged();
    public event OnInventoryChanged inventoryChanged;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        // Инициализируем большие слоты (все 3 места)
        while (weaponSlots.Count < maxWeaponSlots)
        {
            weaponSlots.Add(new InventorySlot(null, 0));
        }
    }

    /// <summary>
    /// УМНАЯ система добавления - автоматически определяет куда положить
    /// </summary>
    public bool AddItem(ItemData itemData, int amount = 1)
    {
        if (itemData == null)
        {
            Debug.LogError("ItemData is null!");
            return false;
        }

        // Если это оружие - добавляем в weaponSlots
        if (itemData.itemType == ItemData.ItemType.Weapon)
        {
            return AddItemToWeaponSlots(itemData, amount);
        }
        // Если мелкий лут - добавляем в smallSlots
        else
        {
            return AddItemToSmallSlots(itemData, amount);
        }
    }

    /// <summary>
    /// Добавить предмет СПЕЦИАЛЬНО в маленькие слоты (лут, припасы)
    /// </summary>
    public bool AddItemToSmallSlots(ItemData itemData, int amount = 1)
    {
        if (itemData == null || itemData.itemType == ItemData.ItemType.Weapon)
        {
            Debug.LogWarning("Нельзя добавить оружие в маленькие слоты!");
            return false;
        }

        // 1. Ищем существующий слот
        foreach (InventorySlot slot in smallSlots)
        {
            if (slot.itemData != null && slot.itemData.itemName == itemData.itemName)
            {
                int canAdd = slot.GetRemainingSpace();
                if (canAdd >= amount)
                {
                    slot.count += amount;
                    Debug.Log($"✅ Добавлено {amount} x {itemData.itemName} в маленькие слоты");
                    inventoryChanged?.Invoke();
                    return true;
                }
                else if (canAdd > 0)
                {
                    slot.count += canAdd;
                    amount -= canAdd;
                    // Продолжим ищется пустой слот
                }
            }
        }

        // 2. Ищем пустой слот
        if (smallSlots.Count < maxSmallSlots)
        {
            int addNow = Mathf.Min(amount, itemData.maxStackSize);
            InventorySlot newSlot = new InventorySlot(itemData, addNow);
            smallSlots.Add(newSlot);
            Debug.Log($"✅ Новый слот в маленьких: {addNow} x {itemData.itemName}");
            inventoryChanged?.Invoke();

            int remaining = amount - addNow;
            if (remaining > 0)
                return AddItemToSmallSlots(itemData, remaining);

            return true;
        }

        Debug.LogWarning("⚠️ Маленькие слоты переполнены!");
        return false;
    }

    /// <summary>
    /// Добавить оружие в большие слоты с правильным распределением
    /// - Pistol → Слот 0 (2x Medium)
    /// - Knife → Слот 1 (2x Medium)
    /// - Shotgun → Слот 2 (3x Big)
    /// - General → Первый пустой слот (0 или 1, для совместимости)
    /// </summary>
    public bool AddItemToWeaponSlots(ItemData weapon, int amount = 1)
    {
        if (weapon == null || weapon.itemType != ItemData.ItemType.Weapon)
        {
            Debug.LogWarning("Это не оружие!");
            return false;
        }

        // Оружие не складируется - максимум 1 штука за слот
        if (amount != 1)
        {
            Debug.LogWarning("Оружие не складывается! Попытка добавить > 1");
            amount = 1;
        }

        Debug.Log($"🔫 AddItemToWeaponSlots: {weapon.itemName}, тип = {weapon.weaponSlotType}");
        Debug.Log($"   Текущее состояние слотов: [0]={weaponSlots[0].itemData?.itemName ?? "пусто"}, [1]={weaponSlots[1].itemData?.itemName ?? "пусто"}, [2]={weaponSlots[2].itemData?.itemName ?? "пусто"}");

        // ПИСТОЛЕТ - только в Слот 0 (индекс 0)
        if (weapon.weaponSlotType == ItemData.WeaponSlotType.Pistol)
        {
            Debug.Log($"🔫 ПИСТОЛЕТ распознан!");
            // Убеждаемся что слот 0 существует
            while (weaponSlots.Count < 1)
            {
                weaponSlots.Add(new InventorySlot(null, 0));
            }

            if (weaponSlots[0].itemData == null || weaponSlots[0].count == 0)
            {
                weaponSlots[0] = new InventorySlot(weapon, 1);
                Debug.Log($"✅ ПИСТОЛЕТ добавлен в слот 0 (2x): {weapon.itemName}");
                inventoryChanged?.Invoke();
                return true;
            }
            else
            {
                Debug.LogWarning($"⚠️ Слот 0 для пистолета занят оружием: {weaponSlots[0].itemData.itemName}");
                return false;
            }
        }

        // НОЖ - только в Слот 1 (индекс 1)
        if (weapon.weaponSlotType == ItemData.WeaponSlotType.Knife)
        {
            Debug.Log($"🔪 НОЖ распознан!");
            // Убеждаемся что слот 1 существует
            while (weaponSlots.Count < 2)
            {
                weaponSlots.Add(new InventorySlot(null, 0));
            }

            if (weaponSlots[1].itemData == null || weaponSlots[1].count == 0)
            {
                weaponSlots[1] = new InventorySlot(weapon, 1);
                Debug.Log($"✅ НОЖ добавлен в слот 1 (2x): {weapon.itemName}");
                inventoryChanged?.Invoke();
                return true;
            }
            else
            {
                Debug.LogWarning($"⚠️ Слот 1 для ножа занят оружием: {weaponSlots[1].itemData.itemName}");
                return false;
            }
        }

        // ДРОБОВИК - только в Слот 2 (индекс 2)
        if (weapon.weaponSlotType == ItemData.WeaponSlotType.Shotgun)
        {
            Debug.Log($"🔫 ДРОБОВИК распознан!");
            // Убеждаемся, что у нас есть 3 слота
            while (weaponSlots.Count < 3)
            {
                weaponSlots.Add(new InventorySlot(null, 0));
            }

            if (weaponSlots[2].itemData == null || weaponSlots[2].count == 0)
            {
                weaponSlots[2] = new InventorySlot(weapon, 1);
                Debug.Log($"✅ ДРОБОВИК добавлен в слот 2 (3x): {weapon.itemName}");
                inventoryChanged?.Invoke();
                return true;
            }
            else
            {
                Debug.LogWarning("⚠️ Слот 2 для дробовика занят!");
                return false;
            }
        }

        // GENERAL (для совместимости со старым кодом) - первый пустой слот из 0-1
        if (weapon.weaponSlotType == ItemData.WeaponSlotType.General)
        {
            Debug.Log($"⚠️ GENERAL тип - ищем первый пустой слот!");
            for (int i = 0; i < 2; i++)
            {
                // Убеждаемся, что слот существует
                while (weaponSlots.Count <= i)
                {
                    weaponSlots.Add(new InventorySlot(null, 0));
                }

                if (weaponSlots[i].itemData == null || weaponSlots[i].count == 0)
                {
                    weaponSlots[i] = new InventorySlot(weapon, 1);
                    Debug.Log($"✅ Оружие (General) добавлено в слот {i}: {weapon.itemName}");
                    inventoryChanged?.Invoke();
                    return true;
                }
            }
            Debug.LogWarning("⚠️ Оба слота (0,1) заняты!");
            return false;
        }

        Debug.LogWarning($"⚠️ Неизвестный тип оружия: {weapon.weaponSlotType}");
        return false;
    }

    /// <summary>
    /// Удалить из маленьких слотов
    /// </summary>
    public bool RemoveItemFromSmallSlots(string itemName, int amount = 1)
    {
        for (int i = smallSlots.Count - 1; i >= 0; i--)
        {
            if (smallSlots[i].itemData != null && smallSlots[i].itemData.itemName == itemName)
            {
                if (smallSlots[i].count >= amount)
                {
                    smallSlots[i].count -= amount;
                    if (smallSlots[i].count == 0)
                        smallSlots.RemoveAt(i);

                    Debug.Log($"✅ Удалено {amount} x {itemName} из маленьких");
                    inventoryChanged?.Invoke();
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Удалить из больших слотов
    /// </summary>
    public bool RemoveItemFromWeaponSlots(string weaponName)
    {
        for (int i = 0; i < weaponSlots.Count; i++)
        {
            if (weaponSlots[i].itemData != null && weaponSlots[i].itemData.itemName == weaponName)
            {
                weaponSlots[i] = new InventorySlot(null, 0); // Очищаем слот
                Debug.Log($"✅ Оружие удалено: {weaponName}");
                inventoryChanged?.Invoke();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Получить количество в маленьких слотах
    /// </summary>
    public int GetItemCountSmallSlots(string itemName)
    {
        int total = 0;
        foreach (InventorySlot slot in smallSlots)
        {
            if (slot.itemData != null && slot.itemData.itemName == itemName)
                total += slot.count;
        }
        return total;
    }

    /// <summary>
    /// Проверить, есть ли оружие в больших слотах
    /// </summary>
    public bool HasWeapon(string weaponName)
    {
        foreach (InventorySlot slot in weaponSlots)
        {
            if (slot.itemData != null && slot.itemData.itemName == weaponName)
                return true;
        }
        return false;
    }

    /// <summary>
    /// УНИВЕРСАЛЬНЫЙ метод получения количества предмета (работает для обоих типов!)
    /// </summary>
    public int GetItemCount(string itemName)
    {
        int count = 0;

        // Проверяем маленькие слоты
        count += GetItemCountSmallSlots(itemName);

        // Проверяем большие слоты (оружие имеет count = 1)
        foreach (InventorySlot slot in weaponSlots)
        {
            if (slot.itemData != null && slot.itemData.itemName == itemName)
                count += slot.count;
        }

        return count;
    }

    /// <summary>
    /// УНИВЕРСАЛЬНЫЙ метод удаления предмета (работает для обоих типов!)
    /// </summary>
    public bool RemoveItem(string itemName, int amount = 1)
    {
        // Сначала пробуем удалить из маленьких слотов
        for (int i = smallSlots.Count - 1; i >= 0; i--)
        {
            if (smallSlots[i].itemData != null && smallSlots[i].itemData.itemName == itemName)
            {
                if (smallSlots[i].count >= amount)
                {
                    smallSlots[i].count -= amount;
                    if (smallSlots[i].count == 0)
                        smallSlots.RemoveAt(i);

                    Debug.Log($"✅ Удалено {amount} x {itemName} (маленькие слоты)");
                    inventoryChanged?.Invoke();
                    return true;
                }
            }
        }

        // Если не найдено в маленьких - пробуем большие слоты (оружие)
        return RemoveItemFromWeaponSlots(itemName);
    }
}

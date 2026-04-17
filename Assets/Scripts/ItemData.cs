using UnityEngine;

/// <summary>
/// Описание типа предмета (шаблон)
/// </summary>
[CreateAssetMenu(menuName = "Items/ItemData", fileName = "New Item")]
public class ItemData : ScriptableObject
{
    public string itemName = "Предмет";
    public enum ItemType { Weapon, Note, HealthItem, Ammunition }
    public ItemType itemType;
    
    // Для оружия - типы слотов
    // General   -> Автоматически заполняет первый пустой слот (0 или 1)
    // Pistol    -> Слот 0 (2x Medium)
    // Knife     -> Слот 1 (2x Medium)  
    // Shotgun   -> Слот 2 (3x Big)
    public enum WeaponSlotType { General, Pistol, Knife, Shotgun }
    
    [Header("Настройки оружия")]
    public WeaponSlotType weaponSlotType = WeaponSlotType.General;
    
    [Header("Боевые характеристики (только для оружия)")]
    public float weaponDamage = 25f;      // Урон
    public float attackStaminaCost = 15f; // Затраты стамины на удар
    public float blockStaminaCost = 5f;   // Затраты стамины в секунду на удержание блока
    public float blockReduction = 0.7f;   // Процент блокируемого урона (0.7 = 70%)
    
    public int maxStackSize = 1; // Максимум в одном слоте (траву х8, бинты х4, патроны х3 и т.д.)
    public int healAmount = 0;   // Сколько здоровья восстанавливает (только для HealthItem)
    public Sprite itemIcon;      // Иконка для основного инвентаря
    public Sprite hotbarIcon;    // Иконка для хотбара-крестовины (если null - используется itemIcon)
}

/// <summary>
/// Один слот в инвентаре (предмет + количество)
/// </summary>
[System.Serializable]
public class InventorySlot
{
    public ItemData itemData;
    public int count = 0;

    public InventorySlot(ItemData data, int amount = 1)
    {
        itemData = data;
        count = amount;
    }

    public bool CanAddMore()
    {
        return itemData != null && count < itemData.maxStackSize;
    }

    public int GetRemainingSpace()
    {
        return itemData != null ? itemData.maxStackSize - count : 0;
    }
}

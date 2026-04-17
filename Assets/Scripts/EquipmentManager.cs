using UnityEngine;
using UnityEngine.InputSystem;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager instance;

    [Header("Оружие в руке")]
    public Transform weaponHolder; 

    public ItemData currentEquippedItem = null;
    public bool isEquipped = false;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    void Update()
    {
        bool isPendingSlotSelection = HotbarManager.instance != null && HotbarManager.instance.IsPendingSlotSelection();
        
        // Блокируем, если инвентарь открыт
        if (Cursor.lockState == CursorLockMode.None && !isPendingSlotSelection) 
            return;

        // 🎯 КЛАВИША F — УНИВЕРСАЛЬНАЯ
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            // 1. Если в руках оружие, которого НЕТ в хотбаре — убираем его на F
            if (isEquipped && currentEquippedItem != null && !IsItemInHotbar(currentEquippedItem))
            {
                isEquipped = false;
                Debug.Log($"❌ Спрятали оружие (из инвентаря): {currentEquippedItem.itemName}");
                UpdateWeaponVisibility();
                return; 
            }

            // 2. Иначе — используем активную хилку/патроны из хотбара
            if (!isPendingSlotSelection && HotbarManager.instance != null)
            {
                int activeIndex = HotbarManager.instance.GetCurrentSlotIndex();
                ItemData activeItem = HotbarManager.instance.boundItems[activeIndex];

                // На F используем ТОЛЬКО расходники
                if (activeItem != null && activeItem.itemType != ItemData.ItemType.Weapon)
                {
                    TryUseItem(activeIndex);
                }
            }
        }

        // КЛАВИШИ 1, 2, 3, 4
        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) HandleSlotKey(0);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) HandleSlotKey(1);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) HandleSlotKey(2);
            if (Keyboard.current.digit4Key.wasPressedThisFrame) HandleSlotKey(3);
        }

        // СКРОЛЛ МЫШКОЙ (Умный)
        if (!isPendingSlotSelection && Mouse.current != null)
        {
            float scrollDelta = Mouse.current.scroll.ReadValue().y;
            if (scrollDelta > 0) { HotbarManager.instance.NextSlot(); HandleScrollAction(HotbarManager.instance.GetCurrentSlotIndex()); }
            else if (scrollDelta < 0) { HotbarManager.instance.PreviousSlot(); HandleScrollAction(HotbarManager.instance.GetCurrentSlotIndex()); }
        }
    }

    // Проверка: есть ли этот предмет в хотбаре?
    private bool IsItemInHotbar(ItemData item)
    {
        if (HotbarManager.instance == null) return false;
        for (int i = 0; i < 4; i++)
        {
            if (HotbarManager.instance.boundItems[i] != null && 
                HotbarManager.instance.boundItems[i].itemName == item.itemName)
                return true;
        }
        return false;
    }

    void HandleScrollAction(int slotIndex)
    {
        ItemData item = HotbarManager.instance.boundItems[slotIndex];

        // Если скроллим на оружие — достаем его
        if (item != null && item.itemType == ItemData.ItemType.Weapon)
        {
            currentEquippedItem = item;
            isEquipped = true;
            UpdateWeaponVisibility();
        }
        // Если скроллим на ПУСТО или на ХИЛКУ — прячем оружие (свободные руки)
        else
        {
            isEquipped = false;
            UpdateWeaponVisibility();
        }
    }

    void HandleSlotKey(int slotIndex)
    {
        // 1. Проверяем, не в режиме ли мы назначения слота
        if (HotbarManager.instance.IsPendingSlotSelection())
        {
            HotbarManager.instance.ConfirmSlotSelection(slotIndex);
            return; // Выходим
        }

        // 2. СИНХРОНИЗАЦИЯ: Передвигаем визуальную рамку подсветки на этот слот
        HotbarManager.instance.SetCurrentSlot(slotIndex);

        ItemData item = HotbarManager.instance.boundItems[slotIndex];

        // 3. ОСВОБОЖДАЕМ РУКИ (Логика анимаций)
        if (item == null)
        {
            // Если нажали цифру пустого слота — просто убираем оружие
            isEquipped = false;
            UpdateWeaponVisibility();
            return;
        }
        else if (item.itemType == ItemData.ItemType.HealthItem)
        {
            // Если нажали цифру с хилкой — ПРЯЧЕМ оружие, чтобы руки были свободны для анимации лечения
            isEquipped = false;
            UpdateWeaponVisibility();
        }

        // 4. Финальное использование предмета (вызов старой функции)
        TryUseItem(slotIndex);
    }

    void TryUseItem(int slotIndex)
    {
        ItemData item = HotbarManager.instance.boundItems[slotIndex];
        if (item == null) return;

        if (item.itemType == ItemData.ItemType.HealthItem)
        {
            // HealthItem: лечимся и удаляем из инвентаря
            if (InventorySystemNew.instance.GetItemCount(item.itemName) > 0)
            {
                PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.Heal(item.healAmount);
                    Debug.Log($"💊 Используем {item.itemName}: +{item.healAmount} HP");
                }
                
                InventorySystemNew.instance.RemoveItem(item.itemName, 1);
                Debug.Log($"✅ {item.itemName} удален из инвентаря");
            }
        }
        else if (item.itemType == ItemData.ItemType.Weapon)
        {
            // Оружие через хотбар (1-4) просто переключается
            if (currentEquippedItem == item) isEquipped = !isEquipped;
            else { currentEquippedItem = item; isEquipped = true; }
            UpdateWeaponVisibility();
        }
        else
        {
            // Прочие предметы (Ammunition, Note и т.д.)
            Debug.Log($"➕ Используем: {item.itemName}");
            if (InventorySystemNew.instance.GetItemCount(item.itemName) > 0)
            {
                InventorySystemNew.instance.RemoveItem(item.itemName, 1);
            }
        }
    }

    public void EquipItemDirectly(ItemData item)
    {
        if (item == null) return;

        if (item.itemType == ItemData.ItemType.Weapon)
        {
            currentEquippedItem = item;
            isEquipped = true;
            UpdateWeaponVisibility();
            // ЗАКРЫВАЕМ НОВЫЙ ИНВЕНТАРЬ
            if (InventoryUINew.instance != null) InventoryUINew.instance.CloseInventory();
        }
        else
        {
            InventorySystemNew.instance.RemoveItem(item.itemName, 1);
        }
    }

    public void UpdateWeaponVisibility() 
    { 
        if (weaponHolder == null) return;

        // Включаем или выключаем саму "папку" с руками
        weaponHolder.gameObject.SetActive(isEquipped); 

        // Если мы достаем оружие, нужно включить правильную модельку и спрятать остальные
        if (isEquipped && currentEquippedItem != null)
        {
            foreach (Transform weapon in weaponHolder)
            {
                // Сравниваем имя объекта в иерархии с именем в ItemData
                if (weapon.name == currentEquippedItem.itemName)
                {
                    weapon.gameObject.SetActive(true); // Включаем нужную пушку
                }
                else
                {
                    weapon.gameObject.SetActive(false); // Прячем остальные
                }
            }
        }
    }

    public void OnItemDropped(ItemData item)
    {
        if (item.itemType == ItemData.ItemType.Weapon && currentEquippedItem == item)
        {
            isEquipped = false;
            UpdateWeaponVisibility();
        }
    }

    public GameObject GetActiveWeaponObject()
    {
        if (!isEquipped || currentEquippedItem == null) return null;

        foreach (Transform weapon in weaponHolder)
        {
            if (weapon.gameObject.activeSelf)
                return weapon.gameObject;
        }
        return null;
    }
}



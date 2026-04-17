using UnityEngine;
using UnityEngine.EventSystems; // Для OnPointerEnter/Exit
using UnityEngine.InputSystem;
using TMPro; // Для TextMeshProUGUI

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ItemData myItem; // Какой предмет тут лежит
    private TextMeshProUGUI itemCountText; // Ссылка на счетчик (если есть)
    private bool isHovered = false; // Наведена ли мышка

    // Эту функцию вызовет наш "Строитель", когда создаст квадратик
    public void SetupSlot(ItemData item)
    {
        myItem = item;
        // Находим текст счетчика если он есть
        itemCountText = transform.Find("ItemCount")?.GetComponent<TextMeshProUGUI>();
    }

    // Мышка зашла на квадрат
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        Debug.Log($"🖱️ Мышка на {myItem?.itemName}");
    }

    // Мышка ушла с квадрата
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        Debug.Log($"👻 Мышка ушла с {myItem?.itemName}");
    }

    private void Update()
    {
        // Если предмет пустой — ничего не делаем
        if (myItem == null) 
            return;

        // 🎯 1. ЛОГИКА ПКМ (КОНТЕКСТНОЕ МЕНЮ)
        // Просто проверяем: мышка наведена на слот? И нажата ли ПКМ?
        if (isHovered && Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            Debug.Log($"✅ ПКМ ПОПАЛА НА НАШ СЛОТ: {myItem.itemName}!");
            
            if (ItemContextMenu.instance != null)
            {
                ItemContextMenu.instance.ShowMenu(myItem, GetComponent<RectTransform>());
                Debug.Log($"✅ Меню открыто для {myItem.itemName}");
            }
            else
            {
                Debug.LogError("❌ ItemContextMenu.instance НЕ НАЙДЕН!");
            }
        }

        // 🎯 2. ЛОГИКА F (БЫСТРОЕ ПЕРЕКЛЮЧЕНИЕ ЭКИПИРОВКИ)
        if (isHovered && Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            Debug.Log($"🔑 F НАЖАТА на {myItem.itemName}!");
            
            EquipmentManager equipManager = FindFirstObjectByType<EquipmentManager>();
            if (equipManager != null)
            {
                // Если оружие в руках - УБИРАЕМ. Если нет - БЕРЕМ
                equipManager.EquipItemDirectly(myItem);
                Debug.Log($"✅ Переключили оружие {myItem.itemName}");
            }
        }
    }

    private void AssignToHotbar(int slotIndex)
    {
        if (HotbarManager.instance != null)
        {
            HotbarManager.instance.AssignItem(slotIndex, myItem);
        }
    }
}

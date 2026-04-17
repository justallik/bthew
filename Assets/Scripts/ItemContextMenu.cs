using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ItemContextMenu : MonoBehaviour
{
    public static ItemContextMenu instance;

    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button useButton;
    [SerializeField] private Button dropButton;
    [SerializeField] private Button hotbarButton;
    [SerializeField] private CanvasGroup canvasGroup;

    private ItemData selectedItem = null;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (menuPanel == null)
            menuPanel = gameObject;
        
        if (canvasGroup == null)
        {
            canvasGroup = menuPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = menuPanel.AddComponent<CanvasGroup>();
        }
        
        // 🔍 ПОИСК КНОПОК - несколько способов
        if (useButton == null)
        {
            // Способ 1: Прямой поиск по имени
            Transform btn = transform.Find("UseButton");
            if (btn == null) btn = transform.Find("Use Button"); // с пробелом
            
            // Способ 2: Поиск во всех дочерних элементах
            if (btn == null)
            {
                Button[] allButtons = GetComponentsInChildren<Button>();
                foreach (Button b in allButtons)
                {
                    if (b.gameObject.name.Contains("Use"))
                    {
                        btn = b.transform;
                        break;
                    }
                }
            }
            
            if (btn) useButton = btn.GetComponent<Button>();
            Debug.Log($"🔍 UseButton найден: {(useButton != null ? "✅ " + useButton.gameObject.name : "❌")}");
        }
        
        if (dropButton == null)
        {
            Transform btn = transform.Find("DropButton");
            if (btn == null) btn = transform.Find("Drop Button");
            
            if (btn == null)
            {
                Button[] allButtons = GetComponentsInChildren<Button>();
                foreach (Button b in allButtons)
                {
                    if (b.gameObject.name.Contains("Drop"))
                    {
                        btn = b.transform;
                        break;
                    }
                }
            }
            
            if (btn) dropButton = btn.GetComponent<Button>();
            Debug.Log($"🔍 DropButton найден: {(dropButton != null ? "✅ " + dropButton.gameObject.name : "❌")}");
        }

        if (hotbarButton == null)
        {
            Transform btn = transform.Find("HotbarButton");
            if (btn == null) btn = transform.Find("Hotbar Button");
            
            if (btn == null)
            {
                Button[] allButtons = GetComponentsInChildren<Button>();
                foreach (Button b in allButtons)
                {
                    if (b.gameObject.name.Contains("Hotbar"))
                    {
                        btn = b.transform;
                        break;
                    }
                }
            }
            
            if (btn) hotbarButton = btn.GetComponent<Button>();
            Debug.Log($"🔍 HotbarButton найден: {(hotbarButton != null ? "✅ " + hotbarButton.gameObject.name : "❌")}");
        }

        Debug.Log($"═══ LISTENER'ЫМ ═══");
        if (useButton) 
        {
            useButton.onClick.AddListener(OnUseClicked);
            Debug.Log("✅ OnUseClicked listener добавлен на " + useButton.gameObject.name);
        }
        else Debug.LogError("❌ useButton NULL!");
        
        if (dropButton) 
        {
            dropButton.onClick.AddListener(OnDropClicked);
            Debug.Log("✅ OnDropClicked listener добавлен на " + dropButton.gameObject.name);
        }
        else Debug.LogError("❌ dropButton NULL!");
        
        if (hotbarButton) 
        {
            hotbarButton.onClick.AddListener(OnHotbarClicked);
            Debug.Log("✅ OnHotbarClicked listener добавлен на " + hotbarButton.gameObject.name);
        }
        else Debug.LogError("❌ hotbarButton NULL!");

        // 🔍 ОТЛАДКА - проверяем все компоненты перед скрытием
        Debug.Log("═══ ItemContextMenu ОТЛАДКА ═══");
        Debug.Log($"menuPanel: {(menuPanel != null ? "✅ " + menuPanel.name : "❌ NULL")}");
        Debug.Log($"useButton: {(useButton != null ? "✅ " + useButton.gameObject.name : "❌ NULL")}");
        Debug.Log($"dropButton: {(dropButton != null ? "✅ " + dropButton.gameObject.name : "❌ NULL")}");
        Debug.Log($"hotbarButton: {(hotbarButton != null ? "✅ " + hotbarButton.gameObject.name : "❌ NULL")}");
        Debug.Log($"canvasGroup: {(canvasGroup != null ? "✅" : "❌ NULL")}");
        
        if (canvasGroup != null)
        {
            Debug.Log($"canvasGroup начальное состояние:");
            Debug.Log($"  Alpha: {canvasGroup.alpha} (должно быть 0 изначально)");
            Debug.Log($"  Interactable: {canvasGroup.interactable} (должно быть false)");
            Debug.Log($"  Blocks Raycasts: {canvasGroup.blocksRaycasts} (должно быть false)");
        }

        HideMenu();
        Debug.Log("✅ ItemContextMenu инициализирован");
    }

    private void Update()
    {
        // Проверяем всё это только если меню сейчас видимо на экране
        if (canvasGroup.alpha > 0)
        {
            // Закрытие по клику ЛКМ "мимо" меню
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                // Получаем рамки нашего меню
                RectTransform panelRect = menuPanel.GetComponent<RectTransform>();
                // Получаем текущие координаты мышки на экране
                Vector2 mousePos = Mouse.current.position.ReadValue();

                // Специальная функция Unity: она проверяет, попала ли мышка внутрь квадрата меню
                // Если НЕ попала (стоит знак !), то мы закрываем меню
                if (!RectTransformUtility.RectangleContainsScreenPoint(panelRect, mousePos, null))
                {
                    HideMenu();
                }
            }
        }
    }

    public void ShowMenu(ItemData item, RectTransform slotRect)
    {
        if (item == null) return;
        selectedItem = item;

        RectTransform rect = menuPanel.GetComponent<RectTransform>();
        if (rect && slotRect != null)
        {
            // Меню появляется по центру слота с смещением
            rect.position = slotRect.position + new Vector3(50, -50, 0);
        }

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        
        Debug.Log($"📋 МЕНЮ ОТКРЫТО: {item.itemName}");
    }

    public void HideMenu()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        selectedItem = null;
        
        Debug.Log($"HIDE CONTEXT MENU вызван из: {Time.frameCount}");
    }

    public bool IsOpen()
    {
        return canvasGroup != null && canvasGroup.alpha > 0.001f;
    }

    public void OnUseClicked()
    {
        if (selectedItem == null) return;

        Debug.Log($"✅ USE: {selectedItem.itemName}");

        // Если это хилка - используем и восстанавливаем здоровье
        if (selectedItem.itemType == ItemData.ItemType.HealthItem)
        {
            Debug.Log($"💊 Используем хилку: {selectedItem.itemName}");
            
            // Восстанавливаем здоровье
            PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Heal(selectedItem.healAmount);
                Debug.Log($"🩹 +{selectedItem.healAmount} HP восстановлено");
            }
            
            // Удаляем предмет из инвентаря
            InventorySystemNew invSystem = FindFirstObjectByType<InventorySystemNew>();
            if (invSystem != null)
            {
                invSystem.RemoveItem(selectedItem.itemName, 1);
                Debug.Log($"✅ Хилка использована и удалена из инвентаря");
            }
            else
            {
                Debug.LogError("❌ InventorySystemNew НЕ НАЙДЕН!");
            }
        }
        // Если это оружие - экипируем (инвентарь закроется автоматически в EquipItemDirectly)
        else if (selectedItem.itemType == ItemData.ItemType.Weapon)
        {
            Debug.Log($"🔪 Берем оружие: {selectedItem.itemName}");
            EquipmentManager em = FindFirstObjectByType<EquipmentManager>();
            if (em) em.EquipItemDirectly(selectedItem);
        }

        HideMenu();
    }

    public void OnDropClicked()
    {
        if (selectedItem == null) return;

        Debug.Log($"🗑️ DROP: {selectedItem.itemName}");

        SpawnDroppedItem(selectedItem);

        // ИСПРАВЛЕНИЕ: используем новую систему инвентаря InventorySystemNew!
        InventorySystemNew invSystem = FindFirstObjectByType<InventorySystemNew>();
        if (invSystem != null)
        {
            invSystem.RemoveItem(selectedItem.itemName, 1);
        }

        // Если это оружие что в руках - разэкипируем
        EquipmentManager em = FindFirstObjectByType<EquipmentManager>();
        if (em)
            em.OnItemDropped(selectedItem);

        // ✅ НОВОЕ: Удаляем предмет из хотбара, если он там был
        if (HotbarManager.instance != null)
        {
            HotbarManager.instance.RemoveItemFromHotbar(selectedItem);
        }

        HideMenu();
    }

    public void OnHotbarClicked()
    {
        if (selectedItem == null) return;

        Debug.Log($"🔥 HOTBAR: {selectedItem.itemName}");

        if (HotbarManager.instance == null)
        {
            Debug.LogError("❌ HotbarManager.instance NULL!");
            HideMenu();
            return;
        }

        // Переходим в режим выбора слота
        HotbarManager.instance.SetPendingItemForHotbar(selectedItem);

        HideMenu();
    }

    private void SpawnDroppedItem(ItemData itemData)
    {
        Debug.Log($"🔴 SpawnDroppedItem ВЫЗВАНА для {itemData.itemName}");
        
        // Ищем камеру правильно
        Camera cam = FindFirstObjectByType<Camera>();
        if (!cam) 
        {
            Debug.LogError("❌ Camera НЕ НАЙДЕНА!");
            return;
        }

        Vector3 pos = cam.transform.position + cam.transform.forward * 1.5f;
        Debug.Log($"📍 Позиция спауна: {pos}");

        Debug.Log($"🔭 Ищу объект {itemData.itemName}...");
        GameObject template = FindObjectByName(itemData.itemName);
        
        if (!template) 
        {
            Debug.LogError($"❌❌❌ КРИТИЧЕСКАЯ ОШИБКА: Объект '{itemData.itemName}' НЕ НАЙДЕН на сцене!");
            Debug.Log("Проверь: есть ли в иерархии объект Leaf или Knife с компонентом InteractableItem?");
            return;
        }

        Debug.Log($"✅ Шаблон найден: {template.name}, активен: {template.activeSelf}");

        GameObject drop = Instantiate(template, pos, Quaternion.identity);
        drop.name = itemData.itemName + " (Dropped)";
        
        Debug.Log($"✅✅✅ СПАУНИ УСПЕШЕН: {drop.name} создан на {pos}");
    }

    private GameObject FindObjectByName(string name)
    {
        Debug.Log($"🔍 FindObjectByName ищет '{name}'");
        
#pragma warning disable CS0618
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        Debug.Log($"   Всего объектов на сцене: {allObjects.Length}");
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains(name))
            {
                Debug.Log($"   Найден объект с похожим именем: {obj.name}");
                
                InteractableItem interactable = obj.GetComponent<InteractableItem>();
                if (interactable)
                {
                    Debug.Log($"   ✅ ТОТ! {obj.name} имеет InteractableItem");
                    return obj;
                }
                else
                {
                    Debug.Log($"   ❌ {obj.name} НЕ имеет InteractableItem");
                }
            }
        }
#pragma warning restore CS0618
        
        Debug.LogError($"❌ FindObjectByName: '{name}' НЕ НАЙДЕН!");
        return null;
    }
}

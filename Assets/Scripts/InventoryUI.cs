using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI instance; // Синглтон

    [Header("UI Элементы")]
    public GameObject inventoryPanel;          // Панель инвентаря (на Q)
    public Transform inventoryContent;         // Контейнер для слотов (Content в ScrollView)
    public GameObject inventorySlotPrefab;     // Префаб для одного слота

    private bool isOpen = false;

    private void Start()
    {
        // Синглтон
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        Debug.Log("✅ InventoryUI ЗАПУСТИЛСЯ!");
        
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
        else
            Debug.LogError("❌ inventoryPanel НЕ НАЗНАЧЕНА!");

        // Подписываемся на событие изменения инвентаря
        if (InventoryManager.instance != null)
        {
            InventoryManager.instance.inventoryChanged += OnInventoryChanged;
            Debug.Log("✅ Подписались на inventoryChanged");
        }
        else
            Debug.LogError("❌ InventoryManager.instance НЕ НАЙДЕН!");
    }

    private void OnDestroy()
    {
        // Отписываемся от события при удалении
        if (InventoryManager.instance != null)
        {
            InventoryManager.instance.inventoryChanged -= OnInventoryChanged;
        }
    }

    private void OnInventoryChanged()
    {
        // Обновляем UI если инвентарь открыт
        if (isOpen)
        {
            UpdateInventoryDisplay();
        }
    }

    private void Update()
    {
        // Открыть/закрыть инвентарь на Q
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            Debug.Log("Q нажата! ToggleInventory вызывается...");
            ToggleInventory();
        }

        // Закрыть на Escape
        if (isOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseInventory();
        }
    }

    private void ToggleInventory()
    {
        if (isOpen)
            CloseInventory();
        else
            OpenInventory();
    }

    private void OpenInventory()
    {
        Debug.Log("📂 ИНВЕНТАРЬ ОТКРЫВАЕТСЯ!");
        isOpen = true;
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
            
            // ВАЖНО: Управляем Canvas Group чтобы инвентарь блокировал raycast
            CanvasGroup canvasGroup = inventoryPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = inventoryPanel.AddComponent<CanvasGroup>();
            
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            
            Debug.Log($"✅ Инвентарь UI включен (Blocks Raycasts = true)");
        }
        else
            Debug.LogError("❌ inventoryPanel NULL!");

        UpdateInventoryDisplay();

        // Подогни курсор
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseInventory()
    {
        isOpen = false;
        if (inventoryPanel != null)
        {
            // ВАЖНО: Отключаем raycast блокировку перед деактивацией
            CanvasGroup canvasGroup = inventoryPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                
                Debug.Log($"✅ Инвентарь UI отключен (Blocks Raycasts = false)");
            }
            
            inventoryPanel.SetActive(false);
        }

        // Закрываем контекстное меню если оно открыто
        if (ItemContextMenu.instance != null)
            ItemContextMenu.instance.HideMenu();

        // Верни курсор на место
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UpdateInventoryDisplay()
    {
        if (inventoryContent == null || inventorySlotPrefab == null)
        {
            Debug.LogError("InventoryUI: Missing inventoryContent or inventorySlotPrefab!");
            return;
        }

        if (InventoryManager.instance == null)
        {
            Debug.LogError("InventoryUI: InventoryManager.instance is null!");
            return;
        }

        // Очищаем старые слоты
        foreach (Transform child in inventoryContent)
        {
            Destroy(child.gameObject);
        }

        // Добавляем новые слоты
        var slots = InventoryManager.instance.GetAllSlots();
        foreach (InventorySlot slot in slots)
        {
            if (slot.itemData != null)
            {
                GameObject slotUI = Instantiate(inventorySlotPrefab, inventoryContent);
                
                // ВАЖНО: Добавляем GraphicRaycaster чтобы события мыши работали!
                if (slotUI.GetComponent<GraphicRaycaster>() == null)
                {
                    slotUI.AddComponent<GraphicRaycaster>();
                    Debug.Log("✅ GraphicRaycaster добавлен на слот");
                }
                
                // --- Передаем предмет в наш новый скрипт-сенсор ---
                InventorySlotUI slotScript = slotUI.GetComponent<InventorySlotUI>();
                if (slotScript != null) 
                {
                    slotScript.SetupSlot(slot.itemData);
                    Debug.Log($"📦 SetupSlot: {slot.itemData.itemName}");
                }
                // ---------------------------------------------------
                
                // Находим элементы в префабе
                Image itemIcon = slotUI.transform.Find("ItemIcon")?.GetComponent<Image>();
                TextMeshProUGUI itemNameText = slotUI.transform.Find("ItemName")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI itemCountText = slotUI.transform.Find("ItemCount")?.GetComponent<TextMeshProUGUI>();

                // Устанавливаем иконку
                if (itemIcon != null && slot.itemData.itemIcon != null)
                    itemIcon.sprite = slot.itemData.itemIcon;

                if (itemNameText != null)
                    itemNameText.text = slot.itemData.itemName;

                if (itemCountText != null)
                {
                    if (slot.itemData.maxStackSize > 1)
                        itemCountText.text = "x" + slot.count;
                    else
                        itemCountText.text = ""; // Для оружия не показываем количество
                }
            }
        }
    }
}

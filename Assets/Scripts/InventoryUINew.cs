using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class InventoryUINew : MonoBehaviour
{
    public static InventoryUINew instance;

    [Header("Главное меню (Родитель)")]
    public GameObject playerMenu;         

    [Header("Вкладки")]
    public GameObject inventoryPanel;     
    public GameObject diaryPanel;         

    [Header("Кнопки переключения")]
    public GameObject btnShowInventory; // Кнопка рюкзака - на дневнике
    public GameObject btnShowDiary;     // Кнопка дневника - на инвентаре

    [Header("МАЛЕНЬКИЕ слоты")]
    public Transform smallSlotsContent;           
    public GameObject smallSlotPrefab;            

    [Header("БОЛЬШИЕ слоты")]
    public Transform weaponSlotsContent;          
    public GameObject weaponSlotPrefab_2x;        
    public GameObject weaponSlotPrefab_3x;        

    [Header("Звуки UI")]
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioClip openInventoryClip;
    [SerializeField] private AudioClip closeInventoryClip;

    private bool isOpen = false;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (playerMenu != null) playerMenu.SetActive(false);
        
        if (InventorySystemNew.instance != null) 
            InventorySystemNew.instance.inventoryChanged += OnInventoryChanged;

        if (DiaryManager.instance != null)
            DiaryManager.instance.onDiaryChanged += OnDiaryStateChanged;
        
        // Привязываем кнопку "Дневник"
        if (btnShowDiary != null)
        {
            Button btn = btnShowDiary.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(ShowDiaryTab);
        }
        
        // Привязываем кнопку "Рюкзак"
        if (btnShowInventory != null)
        {
            Button btn = btnShowInventory.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(ShowInventoryTab);
        }
    }

    private void OnDestroy()
    {
        if (InventorySystemNew.instance != null) 
            InventorySystemNew.instance.inventoryChanged -= OnInventoryChanged;

        if (DiaryManager.instance != null)
            DiaryManager.instance.onDiaryChanged -= OnDiaryStateChanged;
        
        if (btnShowDiary != null)
        {
            Button btn = btnShowDiary.GetComponent<Button>();
            if (btn != null) btn.onClick.RemoveListener(ShowDiaryTab);
        }
        
        if (btnShowInventory != null)
        {
            Button btn = btnShowInventory.GetComponent<Button>();
            if (btn != null) btn.onClick.RemoveListener(ShowInventoryTab);
        }
    }

    private void OnInventoryChanged()
    {
        if (isOpen) UpdateInventoryDisplay();
    }

    // Срабатывает когда DiaryManager меняет состояние (разблокировка / новая запись)
    private void OnDiaryStateChanged()
    {
        if (isOpen && inventoryPanel != null && inventoryPanel.activeSelf)
        {
            // Обновляем видимость кнопки дневника если открыта вкладка инвентаря
            bool diaryUnlocked = DiaryManager.instance != null && DiaryManager.instance.IsUnlocked();
            if (btnShowDiary != null) btnShowDiary.SetActive(diaryUnlocked);
        }
    }

    private void Update()
    {
        if (Keyboard.current == null) return;
        
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            if (isOpen) CloseInventory();
            else OpenInventory();
        }
        
        if (isOpen && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // 1) Если открыто ПКМ-меню — закрываем только его и НЕ закрываем инвентарь
            if (ItemContextMenu.instance != null && ItemContextMenu.instance.IsOpen())
            {
                ItemContextMenu.instance.HideMenu();
                return;
            }

            // 2) Если pending выбор слота — отменяем, НЕ закрываем инвентарь
            if (HotbarManager.instance != null && HotbarManager.instance.IsPendingSlotSelection())
            {
                HotbarManager.instance.CancelSlotSelection();
                return;
            }

            // 3) Иначе закрываем инвентарь
            CloseInventory();
        }
    }

    private void OpenInventory()
    {
        isOpen = true;
        
        if (uiAudioSource != null && openInventoryClip != null)
        {
            uiAudioSource.PlayOneShot(openInventoryClip);
        }
        
        if (playerMenu != null)
        {
            playerMenu.SetActive(true);
            CanvasGroup canvasGroup = playerMenu.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = playerMenu.AddComponent<CanvasGroup>();
            
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        // По умолчанию открываем инвентарь
        ShowInventoryTab();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseInventory()
    {
        Debug.Log($"CLOSE INVENTORY вызван из: {Time.frameCount}");
        
        if (uiAudioSource != null && closeInventoryClip != null)
        {
            uiAudioSource.PlayOneShot(closeInventoryClip);
        }
        
        isOpen = false;
        
        // ✅ Отменяем режим выбора слота для хотбара, если он был активен
        if (HotbarManager.instance != null)
        {
            HotbarManager.instance.CancelSlotSelection();
        }
        
        if (playerMenu != null)
        {
            CanvasGroup canvasGroup = playerMenu.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            playerMenu.SetActive(false);
        }

        if (ItemContextMenu.instance != null) ItemContextMenu.instance.HideMenu();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ==========================================
    // ЛОГИКА ПЕРЕКЛЮЧЕНИЯ ВКЛАДОК (ОБНОВЛЕННАЯ)
    // ==========================================

    public void ShowInventoryTab()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(true);
        if (diaryPanel != null) diaryPanel.SetActive(false);

        // Кнопка дневника видна только если дневник разблокирован
        bool diaryUnlocked = DiaryManager.instance != null && DiaryManager.instance.IsUnlocked();
        if (btnShowInventory != null) btnShowInventory.SetActive(false);
        if (btnShowDiary != null) btnShowDiary.SetActive(diaryUnlocked);
        
        UpdateInventoryDisplay(); 
    }

    public void ShowDiaryTab()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (diaryPanel != null) diaryPanel.SetActive(true);
        if (btnShowInventory != null) btnShowInventory.SetActive(true);
        if (btnShowDiary != null) btnShowDiary.SetActive(false);

        // Уведомляем DiaryUI что вкладка открыта
        if (DiaryUI.instance != null) DiaryUI.instance.OnTabOpened();
    }

    public bool IsOpen() => isOpen;

    // ==========================================
    // ЛОГИКА ОБНОВЛЕНИЯ СЛОТОВ (Без изменений)
    // ==========================================

    public void UpdateInventoryDisplay()
    {
        UpdateSmallSlotsDisplay();
        UpdateWeaponSlotsDisplay();
    }

    private void UpdateSmallSlotsDisplay()
    {
        if (smallSlotsContent == null || smallSlotPrefab == null || InventorySystemNew.instance == null) return;
        foreach (Transform child in smallSlotsContent) Destroy(child.gameObject);
        foreach (InventorySlot slot in InventorySystemNew.instance.smallSlots)
        {
            if (slot.itemData != null) CreateSlotUI(slot, smallSlotPrefab, smallSlotsContent, "small");
        }
    }

    private void UpdateWeaponSlotsDisplay()
    {
        if (weaponSlotsContent == null || InventorySystemNew.instance == null) return;
        while (InventorySystemNew.instance.weaponSlots.Count < InventorySystemNew.instance.maxWeaponSlots)
        {
            InventorySystemNew.instance.weaponSlots.Add(new InventorySlot(null, 0));
        }

        for (int i = 0; i < weaponSlotsContent.childCount; i++)
        {
            Transform slotTrans = weaponSlotsContent.GetChild(i);
            GameObject slotUI = slotTrans.gameObject;
            if (i < InventorySystemNew.instance.maxWeaponSlots)
            {
                InventorySlot slot = InventorySystemNew.instance.weaponSlots[i];
                bool hasWeapon = slot.itemData != null && slot.count > 0;
                slotUI.SetActive(hasWeapon);
                if (hasWeapon) UpdateWeaponSlotUI(slotUI, slot, i);
            }
        }
    }

    private void UpdateWeaponSlotUI(GameObject slotUI, InventorySlot slot, int slotIndex)
    {
        Image itemIcon = slotUI.transform.Find("ItemIcon")?.GetComponent<Image>();
        if (itemIcon == null && slotUI.transform.childCount > 0) itemIcon = slotUI.transform.GetChild(0).GetComponent<Image>();
        if (itemIcon == null) itemIcon = slotUI.GetComponent<Image>();
        
        TextMeshProUGUI itemCountText = slotUI.transform.Find("ItemCount")?.GetComponent<TextMeshProUGUI>();
        InventorySlotUI slotScript = slotUI.GetComponent<InventorySlotUI>();

        if (slot.itemData != null)
        {
            if (itemIcon != null)
            {
                itemIcon.enabled = true;
                itemIcon.sprite = slot.itemData.itemIcon != null ? slot.itemData.itemIcon : null;
                itemIcon.color = new Color(1, 1, 1, 1);
                itemIcon.raycastTarget = true;
            }
            if (itemCountText != null) itemCountText.text = "";
            if (slotScript != null) slotScript.SetupSlot(slot.itemData);
        }
        else
        {
            if (itemIcon != null)
            {
                itemIcon.sprite = null;
                itemIcon.color = new Color(1, 1, 1, 0.3f);
                itemIcon.enabled = true; 
            }
            if (itemCountText != null) itemCountText.text = "";
        }
    }

    private void CreateSlotUI(InventorySlot slot, GameObject prefab, Transform parent, string slotType)
    {
        GameObject slotUI = Instantiate(prefab, parent);
        Image itemIcon = slotUI.transform.Find("ItemIcon")?.GetComponent<Image>();
        TextMeshProUGUI itemCountText = slotUI.transform.Find("ItemCount")?.GetComponent<TextMeshProUGUI>();

        if (itemIcon != null && slot.itemData != null && slot.itemData.itemIcon != null)
        {
            itemIcon.sprite = slot.itemData.itemIcon;
        }

        if (itemCountText != null)
        {
            itemCountText.text = slot.count >= 2 ? slot.count + "x" : "";
        }

        InventorySlotUI slotScript = slotUI.GetComponent<InventorySlotUI>();
        if (slotScript != null && slot.itemData != null)
        {
            slotScript.SetupSlot(slot.itemData);
        }
    }
}

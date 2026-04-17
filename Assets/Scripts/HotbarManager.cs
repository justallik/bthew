using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotbarManager : MonoBehaviour
{
    public static HotbarManager instance;

    [Header("Наши 4 картинки в крестовине")]
    [Tooltip("Порядок: 0-Вверх(1), 1-Вправо(2), 2-Вниз(3), 3-Влево(4)")]
    public Image[] slotIcons = new Image[4];
    
    [Header("Счётчики количества")]
    public TextMeshProUGUI[] slotCounts = new TextMeshProUGUI[4];

    [Header("Выделение активного слота")]
    public Image[] slotHighlights = new Image[4];  // Визуальное выделение активного слота

    [Header("Память хотбара (кто где лежит)")]
    public ItemData[] boundItems = new ItemData[4];

    [Header("Звуки UI")]
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioClip assignToHotbarClip;

    [Header("UI подсказка выбора слота")]
    [SerializeField] private GameObject pendingHintPanel; // панель/объект на Canvas
    [SerializeField] private TextMeshProUGUI pendingHintText; // текст внутри (можно null)
    [SerializeField] private string pendingHintMessage = "Выберите слот: 1  2  3  4 (Esc — отмена)";

    private int currentSlotIndex = 0;  // Текущий активный слот (0-3)
    
    // Ожидание выбора слота при добавлении предмета
    private bool isPendingSlotSelection = false;
    private ItemData pendingItem = null;

    private void Awake()
    {
        // Делаем скрипт одиночкой, чтобы к нему легко было обращаться отовсюду
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 🔍 Если поля пустые, пытаемся найти их автоматически
        if (slotIcons[0] == null)
        {
            Debug.Log("🔍 Поиск Image компонентов для слотов...");
            Image[] allImages = GetComponentsInChildren<Image>();
            
            // Берем первые 4 Image компонента (исключая фон контейнера)
            int count = 0;
            foreach (Image img in allImages)
            {
                if (count < 4 && img.gameObject.name.Contains("Slot"))
                {
                    slotIcons[count] = img;
                    count++;
                }
            }

            if (count < 4)
                Debug.LogWarning($"⚠️ Найдено только {count} Image компонентов вместо 4!");
        }

        // Точно так же для highlights
        if (slotHighlights[0] == null)
        {
            Debug.Log("🔍 Поиск Highlight компонентов...");
            Image[] allImages = GetComponentsInChildren<Image>();
            int count = 0;
            foreach (Image img in allImages)
            {
                if (count < 4 && img.gameObject.name.Contains("Highlight"))
                {
                    slotHighlights[count] = img;
                    count++;
                }
            }
        }

        UpdateHotbarUI();

        // ✅ Спрячем подсказку выбора слота в самом начале
        if (pendingHintPanel != null)
            pendingHintPanel.SetActive(false);

        // ✅ ИСПРАВЛЕНО: Подписываемся на НОВУЮ систему
        if (InventorySystemNew.instance != null)
        {
            InventorySystemNew.instance.inventoryChanged += UpdateHotbarUI;
        }
    }

    private void Update()
    {
        if (!isPendingSlotSelection) return;

        if (UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CancelSlotSelection();
        }
    }

    private void OnDestroy()
    {
        // ✅ ИСПРАВЛЕНО: Отписываемся от НОВОЙ системы
        if (InventorySystemNew.instance != null)
        {
            InventorySystemNew.instance.inventoryChanged -= UpdateHotbarUI;
        }
    }

    /// <summary>
    /// Функция для назначения предмета в слот (с механикой ОБМЕНА / SWAP)
    /// </summary>
    public void AssignItem(int slotIndex, ItemData item)
    {
        if (item == null) return;

        int oldSlotIndex = -1; // Сюда запишем старый слот Ножа (если он был)

        // 1. Ищем, был ли этот предмет (Нож) уже где-то в хотбаре
        for (int i = 0; i < boundItems.Length; i++)
        {
            if (boundItems[i] != null && boundItems[i].itemName == item.itemName)
            {
                oldSlotIndex = i; // Запоминаем, где лежал Нож
                break; // Нашли - останавливаем поиск
            }
        }

        // 2. Запоминаем, кто сейчас лежит в целевом слоте (например, Бинт)
        ItemData itemInTargetSlot = boundItems[slotIndex];

        // 3. Кладем наш новый предмет (Нож) в выбранный слот
        boundItems[slotIndex] = item;

        // 4. Что делать с Бинтом?
        if (oldSlotIndex != -1)
        {
            // Если Нож раньше где-то лежал, перекладываем Бинт на его старое место
            boundItems[oldSlotIndex] = itemInTargetSlot;
            
            if (itemInTargetSlot != null)
            {
                Debug.Log($"🔄 ОБМЕН: {item.itemName} занял слот {slotIndex + 1}, а {itemInTargetSlot.itemName} переехал в слот {oldSlotIndex + 1}");
            }
        }
        else
        {
            // Если Ножа до этого вообще не было в хотбаре, то Бинту некуда переезжать
            // Он просто убирается из хотбара (но остается в инвентаре!)
            if (itemInTargetSlot != null)
            {
                Debug.Log($"⚠️ {itemInTargetSlot.itemName} вытеснен из хотбара предметом {item.itemName}");
            }
        }

        // Обновляем картинки на экране
        UpdateHotbarUI();
    }

    /// <summary>
    /// Обновление картинок и счётчиков на экране
    /// </summary>
    public void UpdateHotbarUI()
    {
        for (int i = 0; i < 4; i++)
        {
            if (boundItems[i] != null)
            {
                // ИСПРАВЛЕНИЕ: используем новую систему инвентаря InventorySystemNew!
                int itemCount = 0;
                if (InventorySystemNew.instance != null)
                    itemCount = InventorySystemNew.instance.GetItemCount(boundItems[i].itemName);
                else if (InventoryManager.instance != null)
                    itemCount = InventoryManager.instance.GetItemCount(boundItems[i].itemName);
                
                if (itemCount > 0)
                {
                    // Показываем иконку
                    // Если у нас есть спец. иконка для хотбара - берем её, иначе обычную
                    if (boundItems[i].hotbarIcon != null)
                    {
                        slotIcons[i].sprite = boundItems[i].hotbarIcon;
                    }
                    else
                    {
                        slotIcons[i].sprite = boundItems[i].itemIcon;
                    }
                    slotIcons[i].enabled = true;
                    
                    // Показываем счётчик "x3"
                    if (slotCounts[i] != null)
                    {
                        slotCounts[i].text = $"x{itemCount}";
                        slotCounts[i].enabled = true;
                    }
                }
                else
                {
                    // Слот пуст - скрываем и иконку и счётчик
                    slotIcons[i].enabled = false;
                    if (slotCounts[i] != null)
                        slotCounts[i].enabled = false;
                    
                    // Очищаем слот если предмет закончился
                    boundItems[i] = null;
                }
            }
            else
            {
                // Пустой слот
                slotIcons[i].enabled = false;
                if (slotCounts[i] != null)
                    slotCounts[i].enabled = false;
            }

            // Обновляем выделение активного слота
            if (slotHighlights[i] != null)
            {
                if (i == currentSlotIndex)
                {
                    slotHighlights[i].enabled = true;  // Подсвечиваем активный слот
                }
                else
                {
                    slotHighlights[i].enabled = false;  // Скрываем остальные
                }
            }
        }
    }

    /// <summary>
    /// Удалить предмет из хотбара (например, при выкидывании из инвентаря)
    /// </summary>
    public void RemoveItemFromHotbar(ItemData item)
    {
        if (item == null) return;

        for (int i = 0; i < boundItems.Length; i++)
        {
            if (boundItems[i] != null && boundItems[i].itemName == item.itemName)
            {
                boundItems[i] = null;
                UpdateHotbarUI();
                Debug.Log($"🧹 Предмет {item.itemName} удален из хотбара (слот {i + 1}).");
                return; // Выходим, так как дубликатов быть не должно
            }
        }
    }

    /// <summary>
    /// Добавить предмет на первый свободный слот
    /// Запретить добавлять дубли одного типа!
    /// </summary>
    public bool AddItemToFirstFreeSlot(ItemData item)
    {
        if (item == null) return false;

        // ЗАПРЕТ: проверяем что такой тип предмета уже не добавлен
        for (int i = 0; i < 4; i++)
        {
            if (boundItems[i] != null && boundItems[i].itemName == item.itemName)
            {
                Debug.LogWarning($"⚠️ {item.itemName} уже в hotbar слоте {i + 1}! Дубли запрещены!");
                return false;
            }
        }

        // Ищем первый свободный слот
        for (int i = 0; i < 4; i++)
        {
            if (boundItems[i] == null)
            {
                AssignItem(i, item);
                Debug.Log($"✅ {item.itemName} добавлен в hotbar слот {i + 1}");
                return true;
            }
        }

        Debug.LogWarning($"⚠️ Hotbar полный! Не пустых свободных слотов для {item.itemName}");
        return false;
    }

    /// <summary>
    /// Переходим в режим ожидания выбора слота для добавления предмета
    /// </summary>
    public void SetPendingItemForHotbar(ItemData item)
    {
        if (item == null) return;
        
        // ❌ МЫ УДАЛИЛИ ПРОВЕРКУ НА ДУБЛИ ОТСЮДА!
        // Очистка старого места теперь происходит внутри AssignItem, 
        // когда игрок уже нажал конкретную цифру.

        isPendingSlotSelection = true;
        pendingItem = item;
        
        // ✅ Показываем подсказку выбора слота
        if (pendingHintPanel != null) pendingHintPanel.SetActive(true);
        if (pendingHintText != null) pendingHintText.text = pendingHintMessage;
        
        Debug.Log($"⏳ Выбор слота для '{item.itemName}' — нажмите 1, 2, 3 или 4");
    }

    /// <summary>
    /// Подтверждение выбора слота (вызывается при нажатии 1-4)
    /// </summary>
    public void ConfirmSlotSelection(int slotIndex)
    {
        Debug.Log($"🔧 ConfirmSlotSelection вызвана: slotIndex={slotIndex}, isPending={isPendingSlotSelection}, item={pendingItem?.itemName}");
        
        if (!isPendingSlotSelection || pendingItem == null)
        {
            Debug.LogWarning($"❌ Не в режиме выбора слота!");
            return;
        }

        if (slotIndex < 0 || slotIndex >= 4)
        {
            Debug.LogWarning("❌ Неверный индекс слота!");
            return;
        }

        string itemName = pendingItem.itemName;  // Сохраняем имя перед обнулением

        // Если в слоте что-то лежало - заменяем
        if (boundItems[slotIndex] != null)
        {
            Debug.Log($"⚠️ Заменяем {boundItems[slotIndex].itemName} на {itemName}");
        }

        // Добавляем предмет в выбранный слот
        AssignItem(slotIndex, pendingItem);
        
        // 🔊 Звук успешного назначения в хотбар (клип добавишь в инспекторе)
        if (uiAudioSource != null && assignToHotbarClip != null)
        {
            uiAudioSource.PlayOneShot(assignToHotbarClip);
        }

        // ✅ Спрячем подсказку
        if (pendingHintPanel != null) pendingHintPanel.SetActive(false);

        // Выходим из режима ожидания
        isPendingSlotSelection = false;
        pendingItem = null;
        
        Debug.Log($"✅ {itemName} добавлен в hotbar слот {slotIndex + 1}");
    }

    /// <summary>
    /// Проверить, ожидаем ли выбора слота
    /// </summary>
    public bool IsPendingSlotSelection()
    {
        return isPendingSlotSelection;
    }

    /// <summary>
    /// Отменить выбор слота
    /// </summary>
    public void CancelSlotSelection()
    {
        if (isPendingSlotSelection)
        {
            Debug.Log($"❌ Добавление {pendingItem.itemName} отменено");
            isPendingSlotSelection = false;
            pendingItem = null;
            
            // ✅ Спрячем подсказку
            if (pendingHintPanel != null) pendingHintPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Переключить активный слот на следующий (Scroll Up)
    /// </summary>
    public void NextSlot()
    {
        currentSlotIndex++;
        if (currentSlotIndex >= 4) currentSlotIndex = 0;
        UpdateHotbarUI();
        Debug.Log($"➡️ Активный слот: {currentSlotIndex + 1}");
    }

    /// <summary>
    /// Переключить активный слот на предыдущий (Scroll Down)
    /// </summary>
    public void PreviousSlot()
    {
        currentSlotIndex--;
        if (currentSlotIndex < 0) currentSlotIndex = 3;
        UpdateHotbarUI();
        Debug.Log($"⬅️ Активный слот: {currentSlotIndex + 1}");
    }

    /// <summary>
    /// Получить индекс активного слота
    /// </summary>
    public int GetCurrentSlotIndex()
    {
        return currentSlotIndex;
    }

    /// <summary>
    /// Установить активный слот
    /// </summary>
    public void SetCurrentSlot(int index)
    {
        if (index >= 0 && index < 4)
        {
            currentSlotIndex = index;
            UpdateHotbarUI();
            Debug.Log($"🎯 Активный слот: {currentSlotIndex + 1}");
        }
    }
}

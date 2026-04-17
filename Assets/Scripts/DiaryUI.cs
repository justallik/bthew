using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Управляет отображением вкладки Дневник в существующей панели Diary.
///
/// Привязки в Inspector:
///   markersContainer  → Panel_Left (Transform, родитель для кнопок-маркеров)
///   markerPrefab      → префаб кнопки с TMP-лейблом и дочерним объектом "NEW"
///   pageTitle         → TMP заголовок (Panel_Right)
///   pageDate          → TMP дата    (Panel_Right)
///   pageContent       → TMP текст   (Panel_Right)
///   pageCounter       → TMP счётчик "X из Y" (Panel_Right)
///   btnPrev / btnNext → кнопки листания (опционально)
///   emptyPlaceholder  → объект-заглушка "Дневник пуст" (опционально)
/// </summary>
public class DiaryUI : MonoBehaviour
{
    public static DiaryUI instance;

    [Header("Маркеры (Panel_Left)")]
    public Transform markersContainer;
    public GameObject markerPrefab;

    [Header("Содержимое страницы (Panel_Right)")]
    public TextMeshProUGUI pageTitle;
    public TextMeshProUGUI pageDate;
    public TextMeshProUGUI pageContent;
    public TextMeshProUGUI pageCounter;

    [Header("Листание")]
    public Button btnPrev;
    public Button btnNext;

    [Header("Заглушка (когда записей нет)")]
    public GameObject emptyPlaceholder;

    [Header("Внешний вид")]
    [Tooltip("Цвет активного маркера")]
    public Color activeMarkerColor = new Color(1f, 0.85f, 0.4f);
    [Tooltip("Цвет неактивного маркера")]
    public Color inactiveMarkerColor = Color.white;

    private int currentPage = 0;

    // ──────────────────────────────────────────────────────────────
    // Жизненный цикл
    // ──────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (btnPrev != null) btnPrev.onClick.AddListener(PrevPage);
        if (btnNext != null) btnNext.onClick.AddListener(NextPage);
    }

    private void OnEnable()
    {
        if (DiaryManager.instance != null)
            DiaryManager.instance.onDiaryChanged += RefreshUI;
        RefreshUI();
    }

    private void OnDisable()
    {
        if (DiaryManager.instance != null)
            DiaryManager.instance.onDiaryChanged -= RefreshUI;
    }

    // ──────────────────────────────────────────────────────────────
    // Публичный API
    // ──────────────────────────────────────────────────────────────

    /// <summary>Вызывается InventoryUINew при открытии вкладки Дневник.</summary>
    public void OnTabOpened()
    {
        // Переходим на последнюю (самую новую) запись
        if (DiaryManager.instance != null && DiaryManager.instance.EntryCount > 0)
            currentPage = DiaryManager.instance.EntryCount - 1;
        RefreshUI();
    }

    /// <summary>Перестроить весь UI по текущим данным DiaryManager.</summary>
    public void RefreshUI()
    {
        if (DiaryManager.instance == null) return;

        int count = DiaryManager.instance.EntryCount;
        RefreshMarkers(count);

        if (count == 0)
        {
            if (emptyPlaceholder != null) emptyPlaceholder.SetActive(true);
            ClearContent();
            return;
        }

        if (emptyPlaceholder != null) emptyPlaceholder.SetActive(false);

        currentPage = Mathf.Clamp(currentPage, 0, count - 1);
        ShowPage(currentPage);
    }

    // ──────────────────────────────────────────────────────────────
    // Листание
    // ──────────────────────────────────────────────────────────────

    public void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            RefreshUI();
        }
    }

    public void NextPage()
    {
        if (DiaryManager.instance != null &&
            currentPage < DiaryManager.instance.EntryCount - 1)
        {
            currentPage++;
            RefreshUI();
        }
    }

    // ──────────────────────────────────────────────────────────────
    // Внутренняя логика
    // ──────────────────────────────────────────────────────────────

    private void SelectPage(int index)
    {
        currentPage = index;
        RefreshUI();
    }

    private void RefreshMarkers(int count)
    {
        if (markersContainer == null || markerPrefab == null) return;

        // Удаляем лишние маркеры
        while (markersContainer.childCount > count)
        {
            Transform last = markersContainer.GetChild(markersContainer.childCount - 1);
            last.SetParent(null);
            Destroy(last.gameObject);
        }

        // Добавляем недостающие маркеры и сразу подключаем слушатель клика
        while (markersContainer.childCount < count)
        {
            int pageIndex = markersContainer.childCount; // индекс до Instantiate
            GameObject markerGO = Instantiate(markerPrefab, markersContainer);
            Button btn = markerGO.GetComponent<Button>();
            if (btn != null)
            {
                int captured = pageIndex;
                btn.onClick.AddListener(() => SelectPage(captured));
            }
        }

        // Обновляем визуальное состояние каждого маркера
        for (int i = 0; i < markersContainer.childCount; i++)
        {
            Transform marker = markersContainer.GetChild(i);

            // Цифра на маркере
            TextMeshProUGUI label = marker.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = (i + 1).ToString();

            // Бейдж "NEW" (дочерний объект с именем "NEW")
            Transform newBadge = marker.Find("NEW");
            if (newBadge != null)
            {
                DiaryEntry entry = DiaryManager.instance.GetEntry(i);
                newBadge.gameObject.SetActive(entry != null && entry.isNew);
            }

            // Активный маркер
            Image markerImage = marker.GetComponent<Image>();
            if (markerImage != null)
                markerImage.color = (i == currentPage) ? activeMarkerColor : inactiveMarkerColor;
        }
    }

    private void ShowPage(int index)
    {
        DiaryEntry entry = DiaryManager.instance.GetEntry(index);
        if (entry == null) return;

        if (pageTitle   != null) pageTitle.text   = entry.title;
        if (pageDate    != null) pageDate.text     = entry.date;
        if (pageContent != null) pageContent.text  = entry.content;
        if (pageCounter != null) pageCounter.text  = $"{index + 1} из {DiaryManager.instance.EntryCount}";

        // Помечаем как прочитанное
        DiaryManager.instance.MarkAsRead(index);

        // Скрываем NEW-бейдж на маркере
        if (markersContainer != null && index < markersContainer.childCount)
        {
            Transform newBadge = markersContainer.GetChild(index).Find("NEW");
            if (newBadge != null) newBadge.gameObject.SetActive(false);
        }

        // Состояние кнопок листания
        if (btnPrev != null) btnPrev.interactable = (index > 0);
        if (btnNext != null) btnNext.interactable = (index < DiaryManager.instance.EntryCount - 1);
    }

    private void ClearContent()
    {
        if (pageTitle   != null) pageTitle.text   = "";
        if (pageDate    != null) pageDate.text     = "";
        if (pageContent != null) pageContent.text  = "";
        if (pageCounter != null) pageCounter.text  = "";
        if (btnPrev     != null) btnPrev.interactable = false;
        if (btnNext     != null) btnNext.interactable = false;
    }
}

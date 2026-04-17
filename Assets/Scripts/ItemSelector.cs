using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class ItemSelector : MonoBehaviour
{
    [Header("Настройки луча")]
    public Camera playerCamera;
    public float rayDistance = 2.5f;
    public LayerMask interactableMask = ~0;

    [Header("UI (Интерфейс)")]
    public GameObject promptUI;
    public TextMeshProUGUI tmpText;

    private InteractableItem currentItem;
    private InteractableBed currentBed; // ДОБАВЛЕНО: Следим за кроватью

    private float lastDetectionTime = 0f;
    private const float DETECTION_DELAY = 0.05f;

    private void Start()
    {
        if (promptUI != null)
        {
            promptUI.SetActive(true);
            if (tmpText != null) tmpText.text = "...";
            promptUI.SetActive(false);
        }
    }

    void Update()
    {
        // Нажатие на E
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (currentItem != null)
            {
                currentItem.Interact();
                ClearSelection();
                return;
            }
            else if (currentBed != null) // ДОБАВЛЕНО: Сон по нажатию
            {
                currentBed.Interact();
                ClearSelection();
                return;
            }
        }

        if (Time.time - lastDetectionTime >= DETECTION_DELAY)
        {
            DetectItem();
            lastDetectionTime = Time.time;
        }
    }

    private void DetectItem()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        
        InteractableItem foundItem = null;
        InteractableBed foundBed = null;

        // Пускаем один луч для всего
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, interactableMask))
        {
            foundItem = hit.collider.GetComponentInParent<InteractableItem>();
            
            // Если это не предмет, проверяем, может это кровать?
            if (foundItem == null)
            {
                foundBed = hit.collider.GetComponentInParent<InteractableBed>();
            }
        }

        // --- ЛОГИКА ОТОБРАЖЕНИЯ ---

        // 1. Смотрим на ПРЕДМЕТ
        if (foundItem != null)
        {
            if (currentItem != foundItem)
            {
                currentItem = foundItem;
                currentBed = null; // Забываем про кровать
                UpdateItemUI();
            }
        }
        // 2. Смотрим на КРОВАТЬ
        else if (foundBed != null)
        {
            if (currentBed != foundBed)
            {
                currentBed = foundBed;
                currentItem = null; // Забываем про предмет
                UpdateBedUI();
            }
        }
        // 3. Смотрим в ПУСТОТУ
        else
        {
            if (currentItem != null || currentBed != null)
            {
                ClearSelection();
            }
        }
    }

    private void UpdateItemUI()
    {
        if (currentItem == null) return;

        string promptText;

        if (currentItem.isDiaryObject)
        {
            // 3D-модель дневника — особый текст
            string diaryName = (currentItem.itemData != null) ? currentItem.itemData.itemName : "Дневник";
            promptText = "[E] Подобрать " + diaryName;
        }
        else
        {
            if (currentItem.itemData == null) return;
            string actionText = (currentItem.itemData.itemType == ItemData.ItemType.Note) ? "Подобрать " : "Взять ";
            promptText = "[E] " + actionText + currentItem.itemData.itemName;
        }

        if (tmpText != null)
            tmpText.text = promptText;

        if (promptUI != null)
            promptUI.SetActive(true);
    }

    private void UpdateBedUI()
    {
        if (tmpText != null)
            tmpText.text = "[E] Лечь спать";

        if (promptUI != null)
            promptUI.SetActive(true);
    }

    void ClearSelection()
    {
        currentItem = null;
        currentBed = null;
        if (promptUI != null)
            promptUI.SetActive(false);
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Обязательно для работы с курсором мыши на UI

/// <summary>
/// Универсальный скрипт для подсветки любого слота при наведении мыши.
/// </summary>
public class SlotHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Настройки фона")]
    [Tooltip("Перетащи сюда Image фона слота (по умолчанию попытается найти сам)")]
    [SerializeField] private Image backgroundImage;

    [Header("Настройки цветов")]
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 1f); // Белый (или твой стандартный)
    [SerializeField] private Color hoverColor = new Color(0.8f, 0.8f, 0.8f, 1f); // Слегка затемненный

    private void Start()
    {
        // Если мы забыли перетащить Image в инспекторе, скрипт попробует найти его сам на этом же объекте
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }

        // Устанавливаем базовый цвет при старте игры
        if (backgroundImage != null)
        {
            backgroundImage.color = normalColor;
        }
        else
        {
            Debug.LogError($"На объекте {gameObject.name} нет компонента Image для подсветки!");
        }
    }

    /// <summary>
    /// Срабатывает, когда курсор мыши заходит на слот
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = hoverColor;
        }
    }

    /// <summary>
    /// Срабатывает, когда курсор мыши уходит со слота
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = normalColor;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Скрипт для затемнения ВРУЧНУЮ подогнанной картинки слота при наведении мыши.
/// </summary>
public class SlotTintHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Картинка для затемнения")]
    [Tooltip("Перетащи сюда внутренний Image, который ты подогнала под рисунок")]
    [SerializeField] private Image tintImage; // Сюда положим нашу ручную картинку

    [Header("Настройки цвета")]
    [SerializeField] private Color hoverColor = new Color(0f, 0f, 0f, 0.4f); // Полупрозрачный черный
    private Color normalColor = new Color(0f, 0f, 0f, 0f); // Полностью невидимый

    private void Start()
    {
        // При старте игры делаем ручную картинку невидимой
        if (tintImage != null)
        {
            tintImage.color = normalColor;
        }
    }

    // Когда мышка ЗАХОДИТ на главный слот
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tintImage != null)
        {
            tintImage.color = hoverColor;
        }
    }

    // Когда мышка УХОДИТ с главного слота
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tintImage != null)
        {
            tintImage.color = normalColor;
        }
    }
}

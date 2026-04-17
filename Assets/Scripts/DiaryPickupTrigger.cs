using UnityEngine;

/// <summary>
/// Альтернативный способ подбора дневника: через коллайдер-триггер.
/// Прикрепи к 3D-модели дневника вместо InteractableItem,
/// если хочешь автоматический подбор при заходе игрока в зону.
///
/// Для подбора через нажатие E — добавь InteractableItem и включи поле isDiaryObject.
/// </summary>
public class DiaryPickupTrigger : MonoBehaviour
{
    [Tooltip("Тег игрока (обычно 'Player')")]
    public string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
            PickUpDiary();
    }

    private void PickUpDiary()
    {
        if (DiaryManager.instance != null)
        {
            DiaryManager.instance.UnlockDiary();
            Debug.Log("📖 Дневник подобран (триггер)!");
        }
        else
        {
            Debug.LogWarning("DiaryPickupTrigger: DiaryManager не найден в сцене!");
        }

        Destroy(gameObject);
    }
}

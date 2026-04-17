using UnityEngine;
using TMPro;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI questText;
    [SerializeField] private GameObject questPanel; // панель с текстом

    [Header("Текущее задание")]
    [SerializeField] private string startingQuest = "Найди выход из лаборатории";

    private void Awake()
    {
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        UpdateQuest(startingQuest);
    }

    public void UpdateQuest(string newQuestText)
    {
        if (questText == null) return;

        questText.text = "📋 " + newQuestText;
        Debug.Log("📋 Задание обновлено: " + newQuestText);

        // Небольшая анимация — панель мигает при смене задания
        if (questPanel != null)
            StartCoroutine(FlashPanel());
    }

    private System.Collections.IEnumerator FlashPanel()
    {
        CanvasGroup cg = questPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = questPanel.AddComponent<CanvasGroup>();

        // Быстро гасим и возвращаем
        cg.alpha = 0f;
        yield return new WaitForSeconds(0.1f);
        cg.alpha = 1f;
    }
}

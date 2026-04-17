using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthUI : MonoBehaviour
{
    [Header("Компоненты UI")]
    [SerializeField] private Image healthBar;              
    [SerializeField] private TextMeshProUGUI healthText;   

    [Header("Жизни")]
    [SerializeField] private Image[] lifeIcons; 

    [Header("Эффект 1: Красная Рамка (при 40%)")]
    [SerializeField] private GameObject redBorderObject;    // Сюда твою рамку
    [SerializeField] private float borderThreshold = 40f;
    [SerializeField] private float borderPulseSpeed = 3f;   // Скорость биения сердца
    [SerializeField] private float borderMaxAlpha = 0.6f;   // Насколько яркой она становится

    [Header("Эффект 2: Темный Шум (при 20%)")]
    [SerializeField] private GameObject noiseObject;        // Сюда твой шум
    [SerializeField] private float noiseThreshold = 20f;
    [SerializeField] private float noiseFlickerSpeed = 15f; // Очень быстрое мерцание шума
    [SerializeField] private float noiseMaxAlpha = 0.4f;    // Прозрачность шума (чтобы не перекрывал игру полностью)

    private PlayerHealth playerHealth;
    private CanvasGroup borderCG;
    private CanvasGroup noiseCG;

    private void Start()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        
        if (healthBar == null)
            healthBar = transform.Find("HealthBar")?.GetComponent<Image>();

        if (redBorderObject != null)
        {
            borderCG = redBorderObject.GetComponent<CanvasGroup>();
            if (borderCG == null) borderCG = redBorderObject.AddComponent<CanvasGroup>();
        }

        if (noiseObject != null)
        {
            noiseCG = noiseObject.GetComponent<CanvasGroup>();
            if (noiseCG == null) noiseCG = noiseObject.AddComponent<CanvasGroup>();
        }
    }

    private void Update()
    {
        if (playerHealth == null) return;

        UpdateHealthBar();
        UpdateHealthText();
        UpdateScreenEffects();
        UpdateLives(); 
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null) return;
        healthBar.fillAmount = Mathf.Clamp01(playerHealth.GetHealthPercent() / 100f);
        healthBar.color = Color.white; 
    }

    private void UpdateHealthText()
    {
        if (healthText == null) return;
        healthText.text = $"{(int)playerHealth.GetCurrentHealth()}/{(int)playerHealth.GetMaxHealth()}";
    }

    private void UpdateScreenEffects()
    {
        float healthPercent = playerHealth.GetHealthPercent();

        // 1. УПРАВЛЕНИЕ КРАСНОЙ РАМКОЙ (Работает всегда, если ХП <= 40)
        if (borderCG != null && redBorderObject != null)
        {
            if (healthPercent <= borderThreshold)
            {
                if (!redBorderObject.activeSelf) redBorderObject.SetActive(true);
                // Плавное биение сердца
                borderCG.alpha = (Mathf.Sin(Time.time * borderPulseSpeed) * 0.5f + 0.5f) * borderMaxAlpha;
            }
            else
            {
                borderCG.alpha = 0f;
                if (redBorderObject.activeSelf) redBorderObject.SetActive(false);
            }
        }

        // 2. УПРАВЛЕНИЕ ШУМОМ (Накладывается сверху, если ХП <= 20)
        if (noiseCG != null && noiseObject != null)
        {
            if (healthPercent <= noiseThreshold)
            {
                if (!noiseObject.activeSelf) noiseObject.SetActive(true);
                // Хаотичное, жуткое мерцание (Perlin Noise)
                noiseCG.alpha = (Mathf.Sin(Time.time * noiseFlickerSpeed) * 0.5f + 0.5f) * noiseMaxAlpha;
            }
            else
            {
                noiseCG.alpha = 0f;
                if (noiseObject.activeSelf) noiseObject.SetActive(false);
            }
        }
    }

    private void UpdateLives()
    {
        if (lifeIcons == null || lifeIcons.Length == 0) return;
        int currentLives = playerHealth.currentLives; 
        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (lifeIcons[i] != null) lifeIcons[i].enabled = (i < currentLives);
        }
    }
}

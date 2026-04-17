using UnityEngine;

public class StealthSystem : MonoBehaviour
{
    public static StealthSystem instance;

    [Header("隐身状态")]
    private bool isStealth = false;
    
    [Header("隐身器材")]
    [SerializeField] private GameObject stealthHood;      // 隐身帽子/罩
    [SerializeField] private float stealthTransparency = 0.3f;  // Прозрачность (0-1)
    
    private Renderer playerRenderer;
    private Color originalColor;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        playerRenderer = GetComponent<Renderer>();
        if (playerRenderer != null)
        {
            originalColor = playerRenderer.material.color;
        }

        if (stealthHood != null)
        {
            stealthHood.SetActive(false);
        }
    }

    // ==================== ВКЛЮЧИТЬ СТЕЛС ====================
    public void EnableStealth()
    {
        if (isStealth) return;

        isStealth = true;
        Debug.Log("👻 Стелс ВКЛЮЧЕН!");

        // Показываем шапку
        if (stealthHood != null) stealthHood.SetActive(true);

        // Делаем игрока полупрозрачным
        if (playerRenderer != null)
        {
            Color stealthColor = originalColor;
            stealthColor.a = stealthTransparency;
            playerRenderer.material.color = stealthColor;
        }
    }

    // ==================== ВЫКЛЮЧИТЬ СТЕЛС ====================
    public void DisableStealth()
    {
        if (!isStealth) return;

        isStealth = false;
        Debug.Log("👻 Стелс ВЫКЛЮЧЕН");

        // Прячем шапку
        if (stealthHood != null) stealthHood.SetActive(false);

        // Восстанавливаем видимость
        if (playerRenderer != null)
        {
            playerRenderer.material.color = originalColor;
        }
    }

    // ==================== ПРОВЕРКА СТЕЛСА ====================
    public bool IsStealth() => isStealth;

    // ==================== ЛОМАНИЕ СТЕЛСА ПРИ АТАКЕ ====================
    public void BreakStealth()
    {
        if (isStealth)
        {
            Debug.Log("⚠️ Атака нарушила стелс!");
            DisableStealth();
        }
    }
}

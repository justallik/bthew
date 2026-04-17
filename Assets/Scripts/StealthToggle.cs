using UnityEngine;
using UnityEngine.InputSystem;

public class StealthToggle : MonoBehaviour
{
    private StealthSystem stealthSystem;

    private void Start()
    {
        stealthSystem = GetComponent<StealthSystem>();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // Нажми X для включения/выключения стелса (тестирование)
        if (Keyboard.current.xKey.wasPressedThisFrame)
        {
            ToggleStealth();
        }
    }

    public void ToggleStealth()
    {
        if (stealthSystem == null) return;

        if (stealthSystem.IsStealth())
        {
            stealthSystem.DisableStealth();
        }
        else
        {
            stealthSystem.EnableStealth();
        }
    }

    // ТАКЖЕ: вызовется когда подберешь предмет "Стелс-шапка" из инвентаря
    public void ActivateStealthItem()
    {
        if (stealthSystem != null)
        {
            stealthSystem.EnableStealth();
            Debug.Log("✨ Надел стелс-шапку!");
        }
    }
}

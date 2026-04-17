using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Обработка ввода от игрока. Отделена от логики движения для чистоты кода.
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public bool SprintInput { get; private set; }
    public bool CrouchInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool AttackInput { get; private set; }
    public bool BlockInput { get; private set; }
    public bool DodgeInput { get; private set; }
    public bool SuperAttackInput { get; private set; }

    private void Update()
    {
        HandleMoveInput();
        HandleSprintInput();
        HandleCrouchInput();
        HandleLookInput();
        HandleCombatInput();
    }

    private void HandleMoveInput()
    {
        // WASD или стики
        Vector2 moveInput = Vector2.zero;
        
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) moveInput.y += 1f;
            if (Keyboard.current.sKey.isPressed) moveInput.y -= 1f;
            if (Keyboard.current.dKey.isPressed) moveInput.x += 1f;
            if (Keyboard.current.aKey.isPressed) moveInput.x -= 1f;
        }
        
        // Нормализуем диагональ
        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();
        
        MoveInput = moveInput;
    }

    private void HandleSprintInput()
    {
        // Left Shift для спринта (удержание)
        SprintInput = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
    }

    private void HandleCrouchInput()
    {
        // Left Ctrl для приседа (удержание)
        CrouchInput = Keyboard.current != null && Keyboard.current.leftCtrlKey.isPressed;
    }

    private void HandleLookInput()
    {
        // Мышь для поворота (обработать в отдельном скрипте камеры)
        if (Mouse.current != null)
        {
            LookInput = Mouse.current.delta.ReadValue();
        }
    }

    private void HandleCombatInput()
    {
        if (Mouse.current != null)
        {
            AttackInput = Mouse.current.leftButton.wasPressedThisFrame;
            BlockInput = Mouse.current.rightButton.isPressed; // Удержание для блока
        }

        // Уклонение: проверяем нажатие S пока зажат Shift
        if (Keyboard.current != null)
        {
            DodgeInput = Keyboard.current.leftShiftKey.isPressed && Keyboard.current.sKey.wasPressedThisFrame;
            SuperAttackInput = Keyboard.current.vKey.wasPressedThisFrame; // V для супер-удара
        }
    }
}

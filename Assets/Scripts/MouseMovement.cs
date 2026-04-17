using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Управление камерой и мышью с реалистичным head bob эффектом для ощущения веса
/// </summary>
public class MouseMovement : MonoBehaviour
{
    [Header("═══ ЧУВСТВИТЕЛЬНОСТЬ ═══")]
    [SerializeField] private float mouseSensitivity = 35f;
    
    [Header("═══ ДИНАМИЧЕСКИЙ FOV (СКОРОСТЬ) ═══")]
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float sprintFOV = 70f;
    [SerializeField] private float fovChangeSpeed = 10f; // Насколько плавно меняется угол
    
    [Header("═══ НАКЛОН КАМЕРЫ (TILT) ═══")]
    [SerializeField] private float maxTiltAngle = 1f; // Градус наклона
    [SerializeField] private float tiltSpeed = 6f;      // Скорость наклона
    
    [Header("═══ HEAD BOB (ПОКАЧИВАНИЕ И ДЫХАНИЕ) ═══")]
    [SerializeField] private float crouchBobFrequency = 0.9f;
    [SerializeField] private float crouchBobIntensity = 0.06f;
    [SerializeField] private float walkBobFrequency = 1.2f;
    [SerializeField] private float walkBobIntensity = 0.07f;
    [SerializeField] private float sprintBobFrequency = 2.05f;
    [SerializeField] private float sprintBobIntensity = 0.115f;
    [SerializeField] private float idleBobFrequency = 0.5f;
    [SerializeField] private float idleBobIntensity = 0.015f;
    
    [Header("═══ ЗВУКИ ШАГОВ ═══")]
    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private AudioClip[] footstepSounds; // Сюда закинь 3-4 звука шагов
    [Range(-1f, 1f)]
    [SerializeField] private float stepTriggerThreshold = -0.9f; // Точка синусоиды (низ), где звучит шаг

    public Transform playerBody;

    private float xRotation = 0f;
    private float currentTilt = 0f;
    private float headBobTimer = 0f;
    private Vector3 originalCameraPosition;
    private bool stepTaken = false;
    private bool isFirstFrame = true; // Защита от первого spike движения мыши
    
    private PlayerInputHandler inputHandler;
    private PlayerMovement playerMovement;  // Ссылка на PlayerMovement
    private Camera cam;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        // Автопоиск playerBody
        if (playerBody == null)
        {
            playerBody = transform.parent;
        }
        
        originalCameraPosition = transform.localPosition;
        inputHandler = playerBody.GetComponent<PlayerInputHandler>();
        playerMovement = playerBody.GetComponent<PlayerMovement>();  // Получаем PlayerMovement
        cam = GetComponent<Camera>();
        
        if (cam != null) cam.fieldOfView = normalFOV;
    }

    private void Update()
    {
        if (playerBody == null || Mouse.current == null) return;

        // Пропускаем первый кадр чтобы избежать spike в мышке
        if (isFirstFrame)
        {
            Mouse.current.delta.ReadValue(); // Просто читаем и выбрасываем
            isFirstFrame = false;
            return;
        }

        // Если открыт инвентарь — управление курсором делает InventoryUINew
        if (InventoryUINew.instance != null && InventoryUINew.instance.IsOpen())
            return;

        HandleMouseAndTilt();
        HandleHeadBobAndFootsteps();
        HandleDynamicFOV();
        
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    // 1 & 3: ВРАЩЕНИЕ МЫШЬЮ И НАКЛОН ПРИ СТРЕЙФЕ
    private void HandleMouseAndTilt()
    {
        // 🎯 ЕСЛИ КУРСОР FREE (инвентарь/меню открыто) - не крутим камеру
        if (Cursor.lockState != CursorLockMode.Locked) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        
        // Защита от БОЛЬШИХ движений мыши (резкий драг по коллайдеру)
        // Уменьшили до 30 пиксельм чтобы гарантировано не слетала камера
        float maxDeltaPerFrame = 30f;
        float deltaLength = mouseDelta.magnitude;
        if (deltaLength > maxDeltaPerFrame)
        {
            mouseDelta = mouseDelta.normalized * maxDeltaPerFrame;
        }
        
        float mouseX = mouseDelta.x * mouseSensitivity * Time.deltaTime;
        float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);

        // Расчет наклона (Tilt) при нажатии A или D
        float targetTilt = 0f;
        if (inputHandler != null && inputHandler.MoveInput.magnitude > 0.1f)
        {
            // Берем ось X (-1 это влево, 1 это вправо)
            targetTilt = -inputHandler.MoveInput.x * maxTiltAngle; 
        }
        
        // Плавно наклоняем
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);

        // Применяем поворот мыши (X) и наклон (Z)
        transform.localRotation = Quaternion.Euler(xRotation, 0f, currentTilt);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    // 2: ДИНАМИЧЕСКИЙ FOV ПРИ БЕГЕ
    private void HandleDynamicFOV()
    {
        if (cam == null || playerMovement == null) return;

        bool isSprinting = playerMovement.GetCurrentSpeed() >= playerMovement.GetSprintSpeed() * 0.8f && inputHandler.MoveInput.magnitude > 0.1f;
        float targetFOV = isSprinting ? sprintFOV : normalFOV;

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovChangeSpeed);
    }

    // 1 & 4: HEAD BOB И ЗВУКИ ШАГОВ
    private void HandleHeadBobAndFootsteps()
    {
        if (inputHandler == null || playerMovement == null) return;

        float currentSpeed = playerMovement.GetCurrentSpeed();
        bool isCrouching = playerMovement.IsCrouching();
        bool playerMoving = inputHandler.MoveInput.magnitude > 0.1f;

        float targetFrequency = idleBobFrequency;
        float targetIntensity = idleBobIntensity;

        if (playerMoving)
        {
            if (isCrouching) { targetFrequency = crouchBobFrequency; targetIntensity = crouchBobIntensity; }
            else if (currentSpeed >= playerMovement.GetSprintSpeed() * 0.8f) { targetFrequency = sprintBobFrequency; targetIntensity = sprintBobIntensity; }
            else { targetFrequency = walkBobFrequency; targetIntensity = walkBobIntensity; }
        }

        headBobTimer += Time.deltaTime * targetFrequency;

        // Вычисляем синусоиду (от -1 до 1)
        float sinValue = Mathf.Sin(headBobTimer * Mathf.PI * 2f);
        Vector3 targetPosition = originalCameraPosition;

        if (playerMoving)
        {
            float verticalBob = sinValue * targetIntensity;
            float horizontalBob = Mathf.Cos(headBobTimer * Mathf.PI) * targetIntensity * 0.5f;
            targetPosition += new Vector3(horizontalBob, verticalBob, 0f);

            // ═══ ЛОГИКА ШАГОВ ═══
            // Если камера опустилась в самую нижнюю точку (-0.9) и шаг еще не сделан
            if (sinValue < stepTriggerThreshold && !stepTaken)
            {
                PlayFootstepSound();
                stepTaken = true; // Запоминаем, что наступили
            }
            // Если камера пошла наверх (больше 0), сбрасываем триггер шага
            else if (sinValue > 0f)
            {
                stepTaken = false; 
            }
        }
        else
        {
            // Дыхание
            targetPosition += new Vector3(0f, sinValue * targetIntensity, 0f);
            stepTaken = false; // Стоим - не шагаем
        }

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * 10f);
    }

    private void PlayFootstepSound()
    {
        // Проверяем, есть ли источник звука и сами звуки
        if (footstepAudioSource != null && footstepSounds != null && footstepSounds.Length > 0)
        {
            // Выбираем случайный звук из массива (чтобы не звучало как пулемет)
            int randomIndex = Random.Range(0, footstepSounds.Length);
            
            // Немного меняем высоту звука (Pitch) для реализма (от 0.9 до 1.1)
            footstepAudioSource.pitch = Random.Range(0.9f, 1.1f);
            
            // Если бежим - звук чуть громче, если крадемся - тише
            float volume = playerMovement.IsCrouching() ? 0.3f : (playerMovement.GetCurrentSpeed() >= playerMovement.GetSprintSpeed() * 0.8f ? 1f : 0.6f);
            
            footstepAudioSource.PlayOneShot(footstepSounds[randomIndex], volume);
        }
    }
}
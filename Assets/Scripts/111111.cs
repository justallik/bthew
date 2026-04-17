using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Управление камерой с реалистичным head bob:
/// - Фигура восьмёрки при ходьбе/беге
/// - Плавное переключение между состояниями (без скачков)
/// - Наклон камеры от боба (кивание в сторону шага)
/// - Дыхание в idle + усиление после бега
/// - Пружинное приземление
/// - Лёгкая инерция мыши
/// </summary>
public class Mous1111 : MonoBehaviour
{
    [Header("═══ ЧУВСТВИТЕЛЬНОСТЬ ═══")]
    [SerializeField] private float mouseSensitivity = 35f;
    [SerializeField] private float mouseInertia = 16f;          // Плавность мыши (выше = меньше инерции)

    [Header("═══ ДИНАМИЧЕСКИЙ FOV ═══")]
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float sprintFOV = 70f;
    [SerializeField] private float fovChangeSpeed = 10f;

    [Header("═══ НАКЛОН КАМЕРЫ (TILT) ═══")]
    [SerializeField] private float maxTiltAngle = 2.5f;         // Было 1f — поднято для заметности
    [SerializeField] private float tiltSpeed = 6f;
    [SerializeField] private float bobTiltStrength = 18f;       // Наклон от шага (кивание)

    [Header("═══ HEAD BOB ═══")]
    [SerializeField] private float crouchBobFrequency = 0.9f;
    [SerializeField] private float crouchBobIntensity = 0.05f;
    [SerializeField] private float walkBobFrequency = 1.2f;
    [SerializeField] private float walkBobIntensity = 0.07f;
    [SerializeField] private float sprintBobFrequency = 2.05f;
    [SerializeField] private float sprintBobIntensity = 0.115f;
    [SerializeField] private float idleBobFrequency = 0.5f;
    [SerializeField] private float idleBobIntensity = 0.015f;
    [SerializeField] private float stateTransitionSpeed = 4f;   // Плавность смены состояний

    [Header("═══ ДЫХАНИЕ ПОСЛЕ БЕГА ═══")]
    [SerializeField] private float exhaustionMultiplier = 2.2f; // Во сколько раз усиливается дыхание
    [SerializeField] private float exhaustionDecaySpeed = 1.2f; // Скорость восстановления

    [Header("═══ ПРУЖИННОЕ ПРИЗЕМЛЕНИЕ ═══")]
    [SerializeField] private float landImpactStrength = 0.06f;
    [SerializeField] private float landSpringStiffness = 14f;
    [SerializeField] private float landSpringDamping = 7f;
    [SerializeField] private float landImpactSpeedThreshold = 2f; // Мин. скорость падения для эффекта

    [Header("═══ ЗВУКИ ШАГОВ ═══")]
    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private AudioClip[] footstepSounds;
    [Range(-1f, 1f)]
    [SerializeField] private float stepTriggerThreshold = -0.9f;

    public Transform playerBody;

    // --- Мышь ---
    private float xRotation = 0f;
    private float currentTilt = 0f;
    private float smoothMouseX = 0f;
    private float smoothMouseY = 0f;
    private bool isFirstFrame = true;

    // --- Bob ---
    private float headBobTimer = 0f;
    private float currentFrequency = 0f;
    private float currentIntensity = 0f;
    private float bobTiltOffset = 0f;   // Наклон от шага — передаётся в HandleMouseAndTilt
    private Vector3 originalCameraPosition;
    private bool stepTaken = false;

    // --- Усталость ---
    private float exhaustionLevel = 0f;   // 0..1 после бега

    // --- Пружина приземления ---
    private float springVelocity = 0f;
    private float springOffset = 0f;
    private bool wasGrounded = false;

    // --- Ссылки ---
    private PlayerInputHandler inputHandler;
    private PlayerMovement playerMovement;
    private Camera cam;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        if (playerBody == null)
            playerBody = transform.parent;

        originalCameraPosition = transform.localPosition;
        inputHandler = playerBody.GetComponent<PlayerInputHandler>();
        playerMovement = playerBody.GetComponent<PlayerMovement>();
        cam = GetComponent<Camera>();

        if (cam != null) cam.fieldOfView = normalFOV;
    }

    private void Update()
    {
        if (playerBody == null || Mouse.current == null) return;

        if (isFirstFrame)
        {
            Mouse.current.delta.ReadValue();
            isFirstFrame = false;
            return;
        }

        HandleMouseAndTilt();
        HandleHeadBobAndFootsteps();
        HandleDynamicFOV();
        HandleLanding();

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked
                ? CursorLockMode.None
                : CursorLockMode.Locked;
        }
    }

    // ── 1. МЫШЬ + TILT ──────────────────────────────────────────────
    private void HandleMouseAndTilt()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        // Защита от спайков (увеличили до 50 для быстрых мышей)
        float maxDeltaPerFrame = 50f;
        if (mouseDelta.magnitude > maxDeltaPerFrame)
            mouseDelta = mouseDelta.normalized * maxDeltaPerFrame;

        float rawX = mouseDelta.x * mouseSensitivity * Time.deltaTime;
        float rawY = mouseDelta.y * mouseSensitivity * Time.deltaTime;

        // Инерция мыши — голова не поворачивается мгновенно
        smoothMouseX = Mathf.Lerp(smoothMouseX, rawX, Time.deltaTime * mouseInertia);
        smoothMouseY = Mathf.Lerp(smoothMouseY, rawY, Time.deltaTime * mouseInertia);

        xRotation -= smoothMouseY;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);

        // Tilt от стрейфа
        float targetTilt = 0f;
        if (inputHandler != null && inputHandler.MoveInput.magnitude > 0.1f)
            targetTilt = -inputHandler.MoveInput.x * maxTiltAngle;

        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);

        // bobTiltOffset приходит из HandleHeadBobAndFootsteps
        transform.localRotation = Quaternion.Euler(xRotation, 0f, currentTilt + bobTiltOffset);
        playerBody.Rotate(Vector3.up * smoothMouseX);
    }

    // ── 2. HEAD BOB + FOOTSTEPS ─────────────────────────────────────
    private void HandleHeadBobAndFootsteps()
    {
        if (inputHandler == null || playerMovement == null) return;

        float currentSpeed = playerMovement.GetCurrentSpeed();
        bool isCrouching = playerMovement.IsCrouching();
        bool playerMoving = inputHandler.MoveInput.magnitude > 0.1f;
        bool isSprinting = currentSpeed >= playerMovement.GetSprintSpeed() * 0.8f && playerMoving;

        // Определяем целевые freq/intensity
        float targetFrequency;
        float targetIntensity;

        if (!playerMoving)
        {
            // Idle: дыхание усиливается после бега
            float breathMultiplier = 1f + exhaustionLevel * (exhaustionMultiplier - 1f);
            targetFrequency = idleBobFrequency;
            targetIntensity = idleBobIntensity * breathMultiplier;
        }
        else if (isCrouching)
        {
            targetFrequency = crouchBobFrequency;
            targetIntensity = crouchBobIntensity;
        }
        else if (isSprinting)
        {
            targetFrequency = sprintBobFrequency;
            targetIntensity = sprintBobIntensity;
        }
        else
        {
            targetFrequency = walkBobFrequency;
            targetIntensity = walkBobIntensity;
        }

        // Плавное переключение состояний — без скачков
        currentFrequency = Mathf.Lerp(currentFrequency, targetFrequency, Time.deltaTime * stateTransitionSpeed);
        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * stateTransitionSpeed);

        // Усталость: накапливается при беге, рассеивается при idle
        if (isSprinting)
            exhaustionLevel = Mathf.Clamp01(exhaustionLevel + Time.deltaTime * 0.5f);
        else
            exhaustionLevel = Mathf.Clamp01(exhaustionLevel - Time.deltaTime * exhaustionDecaySpeed);

        headBobTimer += Time.deltaTime * currentFrequency;
        float sinValue = Mathf.Sin(headBobTimer * Mathf.PI * 2f);
        
        Vector3 targetPosition = originalCameraPosition;

        // 🔥 ВОТ ОНО: Опускаем глаза (камеру) вниз при приседе
        if (isCrouching)
        {
            targetPosition.y -= 0.8f; 
        }

        if (playerMoving)
        {
            // ── ФИГУРА ВОСЬМЁРКИ ──
            // Y качается с одной частотой, X — вдвое медленнее
            float verticalBob = sinValue * currentIntensity;
            float horizontalBob = Mathf.Sin(headBobTimer * Mathf.PI) * currentIntensity * 0.7f;

            targetPosition += new Vector3(horizontalBob, verticalBob, 0f);

            // Наклон камеры от шага (кивание в сторону)
            float targetBobTilt = horizontalBob * bobTiltStrength;
            bobTiltOffset = Mathf.Lerp(bobTiltOffset, targetBobTilt, Time.deltaTime * 10f);

            // Шаги — срабатывают в нижней точке синусоиды
            if (sinValue < stepTriggerThreshold && !stepTaken)
            {
                PlayFootstepSound();
                stepTaken = true;
            }
            else if (sinValue > 0f)
            {
                stepTaken = false;
            }
        }
        else
        {
            // Дыхание в idle — только по Y
            targetPosition += new Vector3(0f, sinValue * currentIntensity, 0f);

            // Наклон плавно возвращается к нулю
            bobTiltOffset = Mathf.Lerp(bobTiltOffset, 0f, Time.deltaTime * 6f);
            stepTaken = false;
        }

        // Пружина приземления добавляется к позиции
        targetPosition.y += springOffset;

        transform.localPosition = Vector3.Lerp(
            transform.localPosition, targetPosition, Time.deltaTime * 10f);
    }

    // ── 3. ПРУЖИННОЕ ПРИЗЕМЛЕНИЕ ────────────────────────────────────
    private void HandleLanding()
    {
        if (playerMovement == null) return;

        bool isGrounded = playerMovement.GetComponent<CharacterController>()?.isGrounded ?? false;

        if (isGrounded && !wasGrounded)
        {
            // Сила удара пропорциональна скорости падения
            float fallSpeed = Mathf.Abs(playerMovement.GetComponent<CharacterController>().velocity.y);
            if (fallSpeed > landImpactThreshold())
                springVelocity = -fallSpeed * landImpactStrength;
        }

        // Пружинная физика
        float springForce = -landSpringStiffness * springOffset;
        float damping = -landSpringDamping * springVelocity;
        springVelocity += (springForce + damping) * Time.deltaTime;
        springOffset += springVelocity * Time.deltaTime;
        springOffset = Mathf.Clamp(springOffset, -0.12f, 0.12f);

        wasGrounded = isGrounded;
    }

    private float landImpactThreshold() => landImpactSpeedThreshold;

    // ── 4. ДИНАМИЧЕСКИЙ FOV ─────────────────────────────────────────
    private void HandleDynamicFOV()
    {
        if (cam == null || playerMovement == null) return;

        bool isSprinting = playerMovement.GetCurrentSpeed() >= playerMovement.GetSprintSpeed() * 0.8f
                           && inputHandler.MoveInput.magnitude > 0.1f;

        float targetFOV = isSprinting ? sprintFOV : normalFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovChangeSpeed);
    }

    // ── 5. ЗВУКИ ШАГОВ ──────────────────────────────────────────────
    private void PlayFootstepSound()
    {
        if (footstepAudioSource == null || footstepSounds == null || footstepSounds.Length == 0)
            return;

        int randomIndex = Random.Range(0, footstepSounds.Length);
        footstepAudioSource.pitch = Random.Range(0.9f, 1.1f);

        float volume = playerMovement.IsCrouching() ? 0.3f
                     : playerMovement.GetCurrentSpeed() >= playerMovement.GetSprintSpeed() * 0.8f ? 1f
                     : 0.6f;

        footstepAudioSource.PlayOneShot(footstepSounds[randomIndex], volume);
    }
}

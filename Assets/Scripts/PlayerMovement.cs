using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("═══ СКОРОСТИ ═══")]
    [SerializeField] private float normalSpeed = 4f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float crouchSpeed = 1.8f;
    [SerializeField] private float speedChangeRate = 10f;
    
    [Header("═══ ПРИСЕД ═══")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float normalHeight = 2f;
    [SerializeField] private Transform visualCrouchRoot;
    [SerializeField] private float crouchVisualScaleY = 0.6f;
    
    [Header("═══ ГРАВИТАЦИЯ ═══")]
    [SerializeField] private float gravity = -15f;
    [SerializeField] private float terminalVelocity = -53f;
    
    [Header("═══ ПРОВЕРКА ЗЕМЛИ ═══")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundedOffset = -0.14f;
    [SerializeField] private float groundedRadius = 0.5f;
    [SerializeField] private LayerMask groundLayers;
    
    [Header("═══ ВЫНОСЛИВОСТЬ ═══")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 20f;
    [SerializeField] private float staminaRecoveryWalk = 15f;
    [SerializeField] private float staminaRecoveryIdle = 30f;
    [SerializeField] private float regenDelay = 1.0f;
    private float regenTimer = 0f;
    
    [Header("═══ НАСТРОЙКИ УКЛОНЕНИЯ (СПРИНТ 4) ═══")]
    [SerializeField] private float dodgeForce = 12f;
    [SerializeField] private float dodgeDuration = 0.2f;
    [SerializeField] private float dodgeRecoveryTime = 0.15f; // Стоп-момент
    
    private CharacterController controller;
    private PlayerInputHandler inputHandler;
    
    private float currentSpeed = 0f;
    private Vector3 velocity = Vector3.zero;
    private float currentStamina;
    
    private bool isGrounded;
    private bool isCrouching;
    private bool canSprint = true;
    private bool isDodging = false; // Блокировка движения
    
    private Vector3 standingCenterPos;
    private Vector3 standingVisualScale;
    private Vector3 standingVisualPos; // 🔥 Запоминаем исходную позицию модели

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        inputHandler = GetComponent<PlayerInputHandler>();
        
        if (controller != null)
        {
            normalHeight = controller.height;
            standingCenterPos = controller.center;
        }
        
        if (visualCrouchRoot != null)
        {
            standingVisualScale = visualCrouchRoot.localScale;
            standingVisualPos = visualCrouchRoot.localPosition; // Запоминаем
        }
        
        currentStamina = maxStamina;
    }

    private void Update()
    {
        if (controller == null || inputHandler == null || groundCheck == null) 
            return;

        // 🛑 ЕСЛИ УКЛОНЯЕМСЯ - БЛОКИРУЕМ ОБЫЧНОЕ УПРАВЛЕНИЕ
        if (isDodging)
        {
            ApplyGravity();
            controller.Move(new Vector3(0, velocity.y * Time.deltaTime, 0));
            return; 
        }

        GroundedCheck();
        UpdateStamina();
        ApplyGravity();
        HandleCrouch();
        Move();
    }

    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
        isGrounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
    }

    private void UpdateStamina()
    {
        bool isMoving = inputHandler.MoveInput.magnitude > 0.1f;
        bool isSprinting = inputHandler.SprintInput && !isCrouching && currentSpeed > 0;
        
        if (regenTimer > 0) regenTimer -= Time.deltaTime;

        if (isSprinting && currentStamina > 0)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            regenTimer = regenDelay;
            if (currentStamina <= 0) { currentStamina = 0; canSprint = false; }
        }
        else if (currentStamina < maxStamina && regenTimer <= 0)
        {
            if (!isMoving) currentStamina += staminaRecoveryIdle * Time.deltaTime;
            else if (isMoving && !isSprinting) currentStamina += staminaRecoveryWalk * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, maxStamina);
        }
        
        if (currentStamina > maxStamina * 0.5f) canSprint = true;
    }

    private void ApplyGravity()
    {
        if (isGrounded) { if (velocity.y < 0) velocity.y = -2f; }
        else { if (velocity.y < terminalVelocity) velocity.y = terminalVelocity; else velocity.y += gravity * Time.deltaTime; }
    }

    private void HandleCrouch()
    {
        bool wantCrouch = inputHandler.CrouchInput;
        if (wantCrouch != isCrouching)
        {
            isCrouching = wantCrouch;
            if (isCrouching)
            {
                controller.height = crouchHeight;
                // 🔥 Высчитываем правильный центр, чтобы ноги не отрывались от пола
                float newCenterY = standingCenterPos.y - (normalHeight - crouchHeight) * 0.5f;
                controller.center = new Vector3(standingCenterPos.x, newCenterY, standingCenterPos.z);
                
                if (visualCrouchRoot != null) 
                {
                    visualCrouchRoot.localScale = new Vector3(standingVisualScale.x, standingVisualScale.y * crouchVisualScaleY, standingVisualScale.z);
                    // 🔥 Сдвигаем саму модельку вниз
                    visualCrouchRoot.localPosition = standingVisualPos + new Vector3(0, -(normalHeight - crouchHeight) * 0.5f, 0);
                }
            }
            else
            {
                controller.height = normalHeight;
                controller.center = standingCenterPos;
                if (visualCrouchRoot != null) 
                {
                    visualCrouchRoot.localScale = standingVisualScale;
                    visualCrouchRoot.localPosition = standingVisualPos;
                }
            }
        }
    }

    private void Move()
    {
        float targetSpeed = inputHandler.MoveInput.magnitude > 0.1f ? (inputHandler.SprintInput && !isCrouching && canSprint ? sprintSpeed : normalSpeed) : 0f;
        if (isCrouching && targetSpeed > 0) targetSpeed = crouchSpeed;
        
        if (Mathf.Abs(currentSpeed - targetSpeed) > 0.1f) currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * speedChangeRate);
        else currentSpeed = targetSpeed;
        
        Vector3 inputDirection = Vector3.zero;
        if (inputHandler.MoveInput.magnitude > 0.1f) inputDirection = (transform.right * inputHandler.MoveInput.x + transform.forward * inputHandler.MoveInput.y).normalized;
        
        Vector3 movement = inputDirection * currentSpeed * Time.deltaTime + new Vector3(0, velocity.y * Time.deltaTime, 0);
        controller.Move(movement);
    }

    // --- ПУБЛИЧНЫЕ МЕТОДЫ ---
    public float GetCurrentSpeed() => currentSpeed;
    public bool IsCrouching() => isCrouching;
    public float GetSprintSpeed() => sprintSpeed;
    public float GetCurrentStamina() => currentStamina;
    public float GetMaxStamina() => maxStamina;
    public bool CanSprint() => canSprint;
    public bool HasEnoughStamina(float amount) => currentStamina >= amount;

    public void UseStamina(float amount)
    {
        currentStamina -= amount;
        regenTimer = regenDelay;
        if (currentStamina <= 0) { currentStamina = 0f; canSprint = false; }
    }

    public void TriggerExhaustion()
    {
        canSprint = false;
        regenTimer = regenDelay;
    }

    // 💨 НОВАЯ ЛОГИКА УКЛОНЕНИЯ С ПРИСЕДОМ
    public void PerformCrouchingDodge(Vector3 sideDirection)
    {
        if (!isDodging) StartCoroutine(DodgeRoutine(sideDirection));
    }

    private IEnumerator DodgeRoutine(Vector3 sideDirection)
    {
        isDodging = true;
        float startTime = Time.time;

        controller.height = crouchHeight;
        float newCenterY = standingCenterPos.y - (normalHeight - crouchHeight) * 0.5f;
        controller.center = new Vector3(standingCenterPos.x, newCenterY, standingCenterPos.z);
        
        if (visualCrouchRoot != null) 
        {
            visualCrouchRoot.localScale = new Vector3(standingVisualScale.x, standingVisualScale.y * crouchVisualScaleY, standingVisualScale.z);
            visualCrouchRoot.localPosition = standingVisualPos + new Vector3(0, -(normalHeight - crouchHeight) * 0.5f, 0);
        }

        while (Time.time < startTime + dodgeDuration)
        {
            controller.Move(sideDirection * dodgeForce * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(dodgeRecoveryTime);

        if (!inputHandler.CrouchInput)
        {
            controller.height = normalHeight;
            controller.center = standingCenterPos;
            if (visualCrouchRoot != null) 
            {
                visualCrouchRoot.localScale = standingVisualScale;
                visualCrouchRoot.localPosition = standingVisualPos;
            }
        }

        isDodging = false;
    }
}
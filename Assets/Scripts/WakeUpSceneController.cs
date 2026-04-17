using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Сцена 1: Пробуждение на поляне.
/// Вешается на отдельный GameObject в сцене (например Scene1_Controller).
/// В Inspector назначь: player, camp, UI-элементы.
/// </summary>
public class WakeUpSceneController : MonoBehaviour
{
    public enum WakeState
    {
        EyesClosed,      // размытие, подсказка ЛКМ
        EyesOpening,    // фокус в процессе
        StandingPrompt, // подсказка Пробел
        Standing,       // встал, качание, монолог
        LookAround,    // подсказка мышь, ждём взгляд вниз
        TattooCutscene, // 2 сек на татуировку
        NotePrompt,     // подсказка E
        NoteReading,    // записка открыта
        ReadyToWalk,    // задание + подсказки WASD, движение включено
        WalkingToCamp,  // идём к лагерю
        SmellTriggered  // отыграна реплика про запах
    }

    [Header("Ссылки")]
    public GameObject player;
    public Transform camp;
    public Camera playerCamera;

    [Header("UI (Canvas)")]
    public GameObject promptPanel;
    public Text promptText;
    public GameObject notePanel;
    public Text noteContentText;
    public GameObject objectivePanel;
    public Text objectiveText;
    public Image blurOverlay;
    public GameObject tattooPanel;

    [Header("Параметры")]
    public float eyesOpenDuration = 2f;
    public float lookDownAngle = -35f;
    public float tattooDuration = 2f;
    public float campSmellDistance = 20f;
    public float standingCameraSwayAmount = 2f;
    public float standingSwayDuration = 3f;

    private WakeState _state = WakeState.EyesClosed;
    private PlayerMovement _playerMovement;
    private MouseMovement _mouseMovement;
    private CharacterController _characterController;
    private float _standingTimer;
    private bool _smellLinePlayed;

    private const string NOTE_TEXT = "СУБЪЕКТ 17\n\nКто ты и кем был — не важно. А вот кем станешь — зависит только от тебя.\n\nУ тебя есть всё необходимое для выживания.\n\nТвоя задача: найти выход.";

    void Start()
    {
        if (player != null)
        {
            _playerMovement = player.GetComponent<PlayerMovement>();
            _mouseMovement = player.GetComponentInChildren<MouseMovement>();
            _characterController = player.GetComponent<CharacterController>();
        }

        SetBlurAlpha(1f);
        SetPrompt("[ЛКМ] Открыть глаза");
        HideNote();
        HideObjective();
        if (tattooPanel != null) tattooPanel.SetActive(false);
        DisableMovement();
    }

    void Update()
    {
        switch (_state)
        {
            case WakeState.EyesClosed:
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                    StartCoroutine(OpenEyes());
                break;

            case WakeState.StandingPrompt:
                if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                    DoStandUp();
                break;

            case WakeState.Standing:
                _standingTimer += Time.deltaTime;
                if (_standingTimer >= standingSwayDuration)
                {
                    _state = WakeState.LookAround;
                    SetPrompt("[МЫШЬ] Осмотреться");
                    EnableMouse();
                }
                break;

            case WakeState.LookAround:
                if (playerCamera != null)
                {
                    float pitch = playerCamera.transform.eulerAngles.x;
                    if (pitch > 180f) pitch -= 360f;
                    if (pitch >= -lookDownAngle && pitch < 90f)
                        TryTriggerLookDown();
                }
                break;

            case WakeState.NotePrompt:
                if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                    ShowNote();
                break;

            case WakeState.NoteReading:
                if (Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame))
                    CloseNote();
                break;

            case WakeState.ReadyToWalk:
            case WakeState.WalkingToCamp:
                if (camp != null && player != null && !_smellLinePlayed)
                {
                    float dist = Vector3.Distance(player.transform.position, camp.position);
                    if (dist <= campSmellDistance)
                        TriggerSmell();
                }
                break;
        }
    }

    private void TryTriggerLookDown()
    {
        _state = WakeState.TattooCutscene;
        SetPrompt("");
        StartCoroutine(TattooSequence());
    }

    private IEnumerator OpenEyes()
    {
        _state = WakeState.EyesOpening;
        SetPrompt("");
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / eyesOpenDuration;
            SetBlurAlpha(1f - t);
            yield return null;
        }
        SetBlurAlpha(0f);
        _state = WakeState.StandingPrompt;
        SetPrompt("[ПРОБЕЛ] Встать");
        yield return null;
    }

    private void DoStandUp()
    {
        SetPrompt("");
        if (_characterController != null)
        {
            _characterController.enabled = false;
            player.transform.position += Vector3.up * 1.6f;
            _characterController.enabled = true;
        }
        _state = WakeState.Standing;
        _standingTimer = 0f;
        StartCoroutine(HeadacheSway());
        SetPrompt(""); // монолог можно вывести в отдельный UI или оставить в логе
    }

    private IEnumerator HeadacheSway()
    {
        if (playerCamera == null) yield break;
        float elapsed = 0f;
        while (elapsed < standingSwayDuration)
        {
            elapsed += Time.deltaTime;
            float s = Mathf.Sin(elapsed * 3f) * standingCameraSwayAmount * (1f - elapsed / standingSwayDuration);
            playerCamera.transform.Rotate(s * 0.02f, 0f, 0f);
            yield return null;
        }
    }

    private IEnumerator TattooSequence()
    {
        EnableMouse();
        if (playerCamera != null)
        {
            Vector3 startRot = playerCamera.transform.localEulerAngles;
            float targetPitch = 60f;
            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                float p = Mathf.Lerp(0f, targetPitch, t / 0.3f);
                playerCamera.transform.localEulerAngles = new Vector3(p, startRot.y, startRot.z);
                yield return null;
            }
        }
        if (tattooPanel != null) tattooPanel.SetActive(true);
        yield return new WaitForSeconds(tattooDuration);
        if (tattooPanel != null) tattooPanel.SetActive(false);
        if (playerCamera != null)
        {
            float t = 0f;
            Vector3 from = playerCamera.transform.localEulerAngles;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                float p = Mathf.Lerp(from.x, 0f, t / 0.5f);
                playerCamera.transform.localEulerAngles = new Vector3(p, from.y, from.z);
                yield return null;
            }
        }
        _state = WakeState.NotePrompt;
        SetPrompt("[E] Прочитать записку");
        yield return null;
    }

    private void ShowNote()
    {
        _state = WakeState.NoteReading;
        if (notePanel != null) notePanel.SetActive(true);
        if (noteContentText != null) noteContentText.text = NOTE_TEXT;
        SetPrompt("");
    }

    private void CloseNote()
    {
        if (notePanel != null) notePanel.SetActive(false);
        _state = WakeState.ReadyToWalk;
        ShowObjective("НАЙТИ ЛАГЕРЬ");
        SetPrompt("[W A S D] Движение  |  [SHIFT] Бег  |  [CTRL] Присесть");
        EnableMovement();
    }

    private void TriggerSmell()
    {
        _smellLinePlayed = true;
        _state = WakeState.SmellTriggered;
        // Здесь можно включить звук реплики "Что за… запах?" и усилить жужжание мух
    }

    private void SetBlurAlpha(float a)
    {
        if (blurOverlay != null)
        {
            Color c = blurOverlay.color;
            c.a = a;
            blurOverlay.color = c;
        }
    }

    private void SetPrompt(string text)
    {
        if (promptText != null) promptText.text = text;
        if (promptPanel != null) promptPanel.SetActive(!string.IsNullOrEmpty(text));
    }

    private void ShowObjective(string text)
    {
        if (objectiveText != null) objectiveText.text = text;
        if (objectivePanel != null) objectivePanel.SetActive(true);
    }

    private void HideObjective()
    {
        if (objectivePanel != null) objectivePanel.SetActive(false);
    }

    private void HideNote()
    {
        if (notePanel != null) notePanel.SetActive(false);
    }

    private void DisableMovement()
    {
        if (_playerMovement != null) _playerMovement.enabled = false;
        if (_mouseMovement != null) _mouseMovement.enabled = false;
    }

    private void EnableMovement()
    {
        if (_playerMovement != null) _playerMovement.enabled = true;
        if (_mouseMovement != null) _mouseMovement.enabled = true;
    }

    private void EnableMouse()
    {
        if (_mouseMovement != null) _mouseMovement.enabled = true;
    }
}

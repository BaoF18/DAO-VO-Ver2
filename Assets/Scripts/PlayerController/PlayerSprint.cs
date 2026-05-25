using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSprint : MonoBehaviour
{
    [Header("Sprint Settings")]
    [Tooltip("Speed when sprinting in units per second (frame-independent).")]
    [SerializeField] public float SprintSpeed = 7.0f;

    [Tooltip("Hold sprint input to keep sprinting.")]
    [SerializeField] private bool holdToSprint = true;

    [Header("Input")]
    [Tooltip("Reference to the Input Actions asset.")]
    public InputActionAsset InputActions;

    public float smoothAmin = 0.1f;
    public float dampUnit = 0.05f;
    // Component references
    private Animator m_animator;
    private Rigidbody m_rigidbody;
    
    // Input action reference
    private InputAction m_sprintAction;
    
    // State variables
    private bool isSprintActive = false;
    private void OnEnable()
    {
        InputActions.FindActionMap("Player")?.Enable();

    }
    private void OnDisable()
    {

        InputActions.FindActionMap("Player")?.Disable();

    }

    private void Awake()
    {
        // Get component references
        m_animator = GetComponent<Animator>();
        m_rigidbody = GetComponent<Rigidbody>();

        // Find the sprint action by name
        if (InputActions != null)
        {
            m_sprintAction = InputActions.FindAction("Sprint");
        }

    }

    private void Update()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            if (isSprintActive)
            {
                isSprintActive = false;
                if (m_animator != null)
                {
                    m_animator.SetFloat("moveAmount", 0f, 0.2f, Time.deltaTime);
                }
            }
            return;
        }
        bool wantsSprint = false;
        bool sprintPressedThisFrame = false;

        if (Keyboard.current != null)
        {
            wantsSprint = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
            sprintPressedThisFrame = Keyboard.current.leftShiftKey.wasPressedThisFrame || Keyboard.current.rightShiftKey.wasPressedThisFrame;
        }
        else if (m_sprintAction != null)
        {
            wantsSprint = m_sprintAction.IsPressed();
            sprintPressedThisFrame = m_sprintAction.WasPressedThisFrame();
        }

        if (holdToSprint)
        {
            if (wantsSprint && !isSprintActive)
            {
                isSprintActive = true;
                if (m_animator != null)
                {
                    m_animator.SetFloat("moveAmount", 1f);
                }
            }
            else if (!wantsSprint && isSprintActive)
            {
                isSprintActive = false;
                if (m_animator != null)
                {
                    m_animator.SetFloat("moveAmount", 0f, 0.2f, Time.deltaTime);
                }
            }
        }
        else if (sprintPressedThisFrame)
        {
            isSprintActive = !isSprintActive;
            if (m_animator != null)
            {
                m_animator.SetFloat("moveAmount", isSprintActive ? 1f : 0f, 0.2f, Time.deltaTime);
            }
        }
    }


    // Public property to allow other scripts to check sprint state
    public bool IsSprintActive => isSprintActive;

    // Public property to get sprint speed
    public float GetSprintSpeed() => SprintSpeed;


    public float DampTime(float currentValue, float finalValue, float unit)
    {
        if(currentValue == finalValue)
        {
            return currentValue;
        }
        if(currentValue < finalValue)
        {
            currentValue += unit * Time.deltaTime;
        }
        else if(currentValue > finalValue)
        {
            currentValue -= unit * Time.deltaTime;
        }
        return currentValue;
    }
}

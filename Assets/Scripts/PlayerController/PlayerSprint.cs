using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSprint : MonoBehaviour
{
    [Header("Sprint Settings")]
    [Tooltip("Speed when sprinting in units per second (frame-independent).")]
    [SerializeField] public float SprintSpeed = 7.0f;

    [Tooltip("Maximum time between two W presses to trigger sprint (seconds).")]
    [SerializeField] private float doubleTapTimeWindow = 0.3f;

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
    // Double-tap tracking
    private float lastWPressTime = -999f; // Time of the last W press
    private int tapCount = 0; // Number of W presses detected within the time window

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
        // Check for W key presses to detect double-tap
        if (m_sprintAction.WasPressedThisFrame())
        {
            float timeSinceLastPress = Time.time - lastWPressTime;

            if (timeSinceLastPress <= doubleTapTimeWindow)
            {
                tapCount++;

                if (tapCount >= 2)
                {
                    // Double-tap detected → reset tap count
                    tapCount = 0;
                    Debug.Log("Double-tap detected!");
                }
            }
            else
            {
                // Too much time has passed since the last press → reset tap count and start counting again
                tapCount = 1;
                Debug.Log("First W press detected");
            }

            lastWPressTime = Time.time;
        }

        // Check if double-tap just happened and W is still held down
        float timeSinceDoubleTap = Time.time - lastWPressTime;
        bool doubleTapJustHappened = (tapCount == 0 && timeSinceDoubleTap <= 0.1f);// Allow a small grace period after the second tap to activate sprint

        if (doubleTapJustHappened && m_sprintAction.IsPressed())
        {
            if (!isSprintActive)
            {
                isSprintActive = true;
                if (m_animator != null)
                {
                    m_animator.SetFloat("moveAmount", 1f);
                }
            }
        }
        else if (!m_sprintAction.IsPressed())
        {
            // W released → cancel sprint
            if (isSprintActive)
            {
                isSprintActive = false;
                if (m_animator != null)
                {
                    m_animator.SetFloat("moveAmount", 0f, 0.2f, Time.deltaTime);
                }
                Debug.Log("Sprint canceled (W released)");
            }
        }

        // Reset tap count if too much time has passed since the last W press
        if (Time.time - lastWPressTime > doubleTapTimeWindow)
        {
            if (tapCount > 0)
            {
                tapCount = 0;
                Debug.Log("Tap count reset (timeout)");
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

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWalk : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Speed of player movement in units per second (frame-independent).")]
    [SerializeField] public float WalkSpeed = 3.0f;

    [Tooltip("Speed of player rotation in degrees per second (frame-independent).")]
    [SerializeField] public float RotateSpeed = 5.0f;

    [Tooltip("Initial upward velocity when jumping in units per second.")]
    [SerializeField] public float JumpSpeed = 5.0f;

    public float smoothAmin = 0.1f;

    public InputActionAsset InputActions;
    
    private InputAction m_moveAction;
    private InputAction m_lookAction;
    private InputAction m_jumpAction;
    
    private Vector2 m_moveAmt;
    private Vector2 m_lookAmt;

    //[Component references]
    private Animator m_animator;
    private Rigidbody m_rigidbody;
    private PlayerSprint playerSprint;

    // Reference to the CameraController script
    CameraController cameraController;

    private void OnEnable()
    {
        InputActions.FindActionMap("Player").Enable();
    }
    
    private void OnDisable()
    {
        InputActions.FindActionMap("Player").Disable();
    }

    private void Awake()
    {
        // Get the CameraController component from the main camera
        cameraController = Camera.main.GetComponent<CameraController>();
        // Find the input actions by name
        m_moveAction = InputActions.FindAction("Movement");

        m_animator = GetComponent<Animator>();
        m_rigidbody = GetComponent<Rigidbody>();
        playerSprint = GetComponent<PlayerSprint>();
    }

    private void Update()
    {
        m_moveAmt = m_moveAction.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        float currentSpeed = WalkSpeed; // Default to walk speed

        if (playerSprint != null && playerSprint.IsSprintActive)
        {
            currentSpeed = playerSprint.SprintSpeed; // Use sprint speed if sprinting
        }
        float moveAmount = Mathf.Clamp(Mathf.Abs(m_moveAmt.x) + Mathf.Abs(m_moveAmt.y), 0, 0.2f);
        
        if (moveAmount > 0f) // Check if moving
        {
            // Calculate movement direction in WORLD space (camera-relative, not player-relative)
            Vector3 cameraForward = cameraController.planarRotation * Vector3.forward;
            Vector3 cameraRight = cameraController.planarRotation * Vector3.right;
            
            Vector3 moveDirection = (cameraForward * m_moveAmt.y) + (cameraRight * m_moveAmt.x);

            // Apply movement
            m_rigidbody.MovePosition(m_rigidbody.position + moveDirection * currentSpeed * Time.deltaTime);

            // Apply rotation to face the movement direction
            if (moveDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotateSpeed * Time.deltaTime);
            }
        }

        // Only update animator if NOT sprinting (PlayerSprint has priority)
        if (playerSprint == null || !playerSprint.IsSprintActive)
        {
            m_animator.SetFloat("moveAmount", moveAmount, 0.2f, Time.deltaTime);
        }

    }
}

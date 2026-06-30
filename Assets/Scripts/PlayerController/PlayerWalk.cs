using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWalk : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Speed of player movement in units per second (frame-independent).")]
    [SerializeField] public float WalkSpeed = 3.0f;

    [Tooltip("Tốc độ lết bộ khi nhân vật cạn kiệt thể lực.")]
    [SerializeField] public float ExhaustedSpeed = 1.0f; // <--- THÊM BIẾN NÀY ĐỂ SẾP TÙY CHỈNH

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
    private PlayerDance playerDance;
    private bool inputLocked;

    // Reference to the CameraController script
    CameraController cameraController;

    private void OnEnable()
    {
        if (!inputLocked)
        {
            InputActions.FindActionMap("Player").Enable();
        }
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
        playerDance = GetComponent<PlayerDance>();
    }

    private void Start()
    {
        DialogueManager.DialogueStarted += HandleDialogueStarted;
        DialogueManager.DialogueEnded += HandleDialogueEnded;
    }

    private void OnDestroy()
    {
        DialogueManager.DialogueStarted -= HandleDialogueStarted;
        DialogueManager.DialogueEnded -= HandleDialogueEnded;
    }

    private void Update()
    {
        if (inputLocked)
        {
            m_moveAmt = Vector2.zero;
            return;
        }

        m_moveAmt = m_moveAction.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            m_animator.SetFloat("moveAmount", 0f, 0.1f, Time.deltaTime);
            return;
        }
        //Stop character movement if currently in a "LockMove" animation state (e.g. attacking, dodging, etc.)
        if (m_animator.GetCurrentAnimatorStateInfo(0).IsTag("LockMove"))
        {
            m_animator.SetFloat("moveAmount", 0f, 0.1f, Time.deltaTime);
            return;
        }

        // =======================================================
        // TÍNH TOÁN TỐC ĐỘ DI CHUYỂN
        // =======================================================
        float currentSpeed = WalkSpeed; // Default to walk speed

        // Ưu tiên 1: Đang cạn kiệt thể lực thì ép tốc độ về cực chậm
        if (PlayerHealth.Instance != null && PlayerHealth.Instance.IsExhausted())
        {
            currentSpeed = ExhaustedSpeed;
        }
        // Ưu tiên 2: Nếu còn thể lực và đang bấm Shift thì mới cho chạy nhanh
        else if (playerSprint != null && playerSprint.IsSprintActive)
        {
            currentSpeed = playerSprint.SprintSpeed;
        }
        // =======================================================

        float moveAmount = Mathf.Clamp(Mathf.Abs(m_moveAmt.x) + Mathf.Abs(m_moveAmt.y), 0, 0.2f);

        if (moveAmount > 0f) // Check if moving
        {
        //if moving then stop dancing
            if (playerDance != null)
            {
                // Check if the trigger for dancing is active, and if so, stop dancing to avoid spamming StopDancing more than twice
                if (m_animator.GetBool("IsDancing"))
                {
                    playerDance.StopDancing();
                }
            }
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

    private void HandleDialogueStarted()
    {
        inputLocked = true;
        InputActions.FindActionMap("Player").Disable();
        if (m_animator != null)
        {
            m_animator.SetFloat("moveAmount", 0f, 0.1f, Time.deltaTime);
        }
    }

    private void HandleDialogueEnded()
    {
        inputLocked = false;
        InputActions.FindActionMap("Player").Enable();
    }
}
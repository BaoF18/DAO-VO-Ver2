using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJump : MonoBehaviour
{
    [Header("Jump Settings")]
    public float jumpForce = 5f;
    public float groundCheckDistance = 0.2f; 
    public LayerMask groundLayer;      
    public int maxJumps = 2;           

    [Header("Input")]
    public InputActionAsset InputActions;
    private InputAction m_jumpAction;

    private Animator m_animator;
    private Rigidbody m_rb;

    private bool isGrounded;
    private bool wasGrounded;
    private int jumpCount = 0;

    private void Awake()
    {
        m_animator = GetComponentInChildren<Animator>();
        m_rb = GetComponent<Rigidbody>();

        if (InputActions != null)
        {
            m_jumpAction = InputActions.FindActionMap("Player")?.FindAction("Jump");
        }
    }

    private void OnEnable() => m_jumpAction?.Enable();
    private void OnDisable() => m_jumpAction?.Disable();

    private void Update()
    {
        CheckGrounded();

        if (m_jumpAction != null && m_jumpAction.WasPressedThisFrame())
        {
            if (isGrounded || jumpCount < maxJumps)
            {
                ExecuteJump();
            }
        }
    }

    private void CheckGrounded()
    {
        Vector3 origin = transform.position + (Vector3.up * 0.1f);
        isGrounded = Physics.Raycast(origin, Vector3.down, groundCheckDistance + 0.1f, groundLayer);

        if (m_animator != null) m_animator.SetBool("IsGrounded", isGrounded);

        // Root motion
        if (isGrounded)
        {
            if (!wasGrounded)
            {
                if (m_animator != null) m_animator.applyRootMotion = true;
                jumpCount = 0; 
            }
        }
        else
        {
            if (jumpCount == 0) jumpCount = 1; 
            if (m_animator != null) m_animator.applyRootMotion = false;
        }

        wasGrounded = isGrounded;
    }

    private void ExecuteJump()
    {
        jumpCount++;
        //Turn off root motion in case
        if (m_animator != null) m_animator.applyRootMotion = false;

        if (m_animator != null)
        {
            if (jumpCount >= 1) m_animator.SetTrigger("Jump");
        }

        m_rb.linearVelocity = new Vector3(m_rb.linearVelocity.x, 0f, m_rb.linearVelocity.z);

        m_rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}
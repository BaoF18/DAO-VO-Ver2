using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJump : MonoBehaviour
{
    [Header("Jump Settings")]
    public float jumpForce = 5f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;

    private Animator m_animator;
    private Rigidbody m_rb;

    private bool isGrounded;
    private bool wasGrounded;
    private bool hasJumped; // đã nhảy 1 lần, chưa chạm đất lại

    private void Awake()
    {
        m_animator = GetComponentInChildren<Animator>();
        m_rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        CheckGrounded();

        bool spacePressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

        // Chỉ cho nhảy khi đang đứng trên đất và chưa nhảy
        if (isGrounded && !hasJumped && spacePressed)
        {
            ExecuteJump();
        }
    }

    private void CheckGrounded()
    {
        Vector3 origin = transform.position + (Vector3.up * 0.1f);
        isGrounded = Physics.Raycast(origin, Vector3.down, groundCheckDistance + 0.1f, groundLayer);

        if (m_animator != null) m_animator.SetBool("IsGrounded", isGrounded);

        if (isGrounded)
        {
            if (!wasGrounded)
            {
                // Vừa chạm đất lại -> reset trạng thái nhảy
                if (m_animator != null) m_animator.applyRootMotion = true;
                hasJumped = false;
            }
        }
        else
        {
            if (m_animator != null) m_animator.applyRootMotion = false;
        }

        wasGrounded = isGrounded;
    }

    private void ExecuteJump()
    {
        hasJumped = true;

        if (m_animator != null)
        {
            m_animator.applyRootMotion = false;
            m_animator.SetTrigger("Jump");
        }

        m_rb.linearVelocity = new Vector3(m_rb.linearVelocity.x, 0f, m_rb.linearVelocity.z);
        m_rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBlock : MonoBehaviour
{
    public InputActionAsset InputActions;
    private InputAction m_blockAction;
    private Animator m_animator;

    private void Awake()
    {
        m_animator = GetComponentInChildren<Animator>(); // Tìm Animator ở cục model con
        m_blockAction = InputActions.FindActionMap("Player").FindAction("Block");
    }

    private void OnEnable() => m_blockAction.Enable();
    private void OnDisable() => m_blockAction.Disable();

    private void Update()
    {
        // Block on
        if (m_blockAction.WasPressedThisFrame())
        {
            if (m_animator != null) m_animator.SetBool("IsBlocking", true);

            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.isBlocking = true;
            }
        }

        // Block off
        if (m_blockAction.WasReleasedThisFrame())
        {
            if (m_animator != null) m_animator.SetBool("IsBlocking", false);

            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.isBlocking = false;
            }
        }
    }
}
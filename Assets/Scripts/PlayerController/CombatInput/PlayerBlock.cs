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
        // Khi BẮT ĐẦU giữ chuột phải -> Bật khiên
        if (m_blockAction.WasPressedThisFrame())
        {
            m_animator.SetBool("IsBlocking", true);
        }

        // Khi NHẢ chuột phải ra -> Tắt khiên
        if (m_blockAction.WasReleasedThisFrame())
        {
            m_animator.SetBool("IsBlocking", false);
        }
    }
}
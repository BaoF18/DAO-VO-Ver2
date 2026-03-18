using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Combo settings")]
    [Tooltip("Time maximum that player can continously click to release combo 2 or 3...")]
    public float comboWindow = 0.8f;

    public InputActionAsset InputActions;
    private InputAction m_attackAction;
    private Animator m_animator;

    private int comboStep = 0; // Đang ở hit thứ mấy (0, 1, 2, 3)
    private float lastClickTime = 0f;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_attackAction = InputActions.FindActionMap("Player").FindAction("Attack");
    }

    private void OnEnable() => m_attackAction.Enable();
    private void OnDisable() => m_attackAction.Disable();

    private void Update()
    {
        // 1. Kiểm tra xem người chơi có dừng bấm quá lâu không
        if (comboStep > 0 && Time.time - lastClickTime > comboWindow)
        {
            ResetCombo();
        }

        // 2. Lắng nghe từng nhát Click chuột
        if (m_attackAction.WasPressedThisFrame())
        {
            lastClickTime = Time.time;

            // Giới hạn tối đa là 3 hit (chuỗi 3 combo)
            if (comboStep < 3)
            {
                comboStep++;
                ExecuteAttack(comboStep);
            }
        }
    }

    private void ExecuteAttack(int step)
    {
        // Báo cho Animator biết đang ở Hit thứ mấy
        m_animator.SetInteger("ComboStep", step);

        // Ra lệnh Kích hoạt hoạt ảnh đánh
        m_animator.SetTrigger("DoAttack");

        Debug.Log("Combo Hit: " + step);

        // Lưu ý: Việc reset từ hit 3 về 0 thường được xử lý mượt mà nhất 
        // bằng Animation Events (gắn ở cuối animation đá trái). 
        // Tạm thời mình để nó tự reset bằng comboWindow timer ở hàm Update.
    }

    private void ResetCombo()
    {
        comboStep = 0;
        m_animator.SetInteger("ComboStep", 0);
        Debug.Log("Hết thời gian nối combo, reset về 0");
    }
}
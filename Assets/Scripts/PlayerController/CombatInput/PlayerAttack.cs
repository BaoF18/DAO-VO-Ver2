using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Combo settings")]
    [Tooltip("Time maximum that player can continously click to release combo 2 or 3...")]
    public float comboWindow = 0.8f;

    [Header("Auto Target Settings")]
    public float autoFaceRange = 3f; // Bán kính hút quái khi đánh

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
        // ==========================================
        // BƯỚC 1: HÚT QUÁI (TỰ ĐỘNG XOAY MẶT VỀ KẺ ĐỊCH)
        // ==========================================
        GameObject targetEnemy = FindClosestEnemy();
        if (targetEnemy != null)
        {
            FaceTarget(targetEnemy.transform.position);
        }
        // ==========================================

        // BƯỚC 2: THỰC THI HOẠT ẢNH
        // Báo cho Animator biết đang ở Hit thứ mấy
        m_animator.SetInteger("ComboStep", step);

        // Ra lệnh Kích hoạt hoạt ảnh đánh
        m_animator.SetTrigger("DoAttack");

        Debug.Log("Combo Hit: " + step + (targetEnemy != null ? " (Đã auto-target!)" : ""));
    }

    private void ResetCombo()
    {
        comboStep = 0;
        m_animator.SetInteger("ComboStep", 0);
        Debug.Log("Hết thời gian nối combo, reset về 0");
    }

    // ==========================================
    // CÁC HÀM HỖ TRỢ TÌM VÀ XOAY GÓC MẶT
    // ==========================================
    private GameObject FindClosestEnemy()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, autoFaceRange);
        GameObject closestEnemy = null;
        float minDistance = Mathf.Infinity;

        foreach (var hit in hitColliders)
        {
            // Kiểm tra xem vật thể có Layer là "Enemy" không
            if (hit.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestEnemy = hit.gameObject;
                }
            }
        }
        return closestEnemy;
    }

    // 1. Thêm biến này lên trên cùng class (Chỗ dưới khai báo Animator)
    private Coroutine lookCoroutine;

    // 2. XÓA hàm FaceTarget CŨ đi, và dán 2 hàm mới này vào:
    private void FaceTarget(Vector3 targetPosition)
    {
        // Dừng cú xoay trước đó (nếu bạn đang spam click) để xoay theo nhịp mới
        if (lookCoroutine != null) StopCoroutine(lookCoroutine);

        lookCoroutine = StartCoroutine(SmoothFaceTarget(targetPosition));
    }

    private System.Collections.IEnumerator SmoothFaceTarget(Vector3 targetPos)
    {
        Vector3 directionToTarget = targetPos - transform.position;
        directionToTarget.y = 0; // Khóa trục Y để không ngửa cổ lên trời

        // Chỉ xoay nếu quái không đứng quá sát cằm (tránh lỗi chia cho 0)
        if (directionToTarget.magnitude > 0.1f)
        {
            Quaternion startRotation = transform.rotation;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget.normalized);

            float time = 0;
            float duration = 0.15f; // Thời gian xoay (0.15s là cực kỳ mượt và chớp nhoáng)

            while (time < 1f)
            {
                time += Time.deltaTime / duration;

                // LƯU Ý: Đã bỏ chữ ".root". Bây giờ ta chỉ xoay cục model hình ảnh!
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, time);

                yield return null; // Chờ frame tiếp theo
            }
        }
    }

    // Vẽ một vòng tròn màu vàng trong cửa sổ Scene để bạn dễ hình dung tầm hút quái
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, autoFaceRange);
    }
}
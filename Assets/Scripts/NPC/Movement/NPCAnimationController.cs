// File: NPCAnimationController.cs
// Mô tả: Đồng bộ hóa animation NPC theo trạng thái di chuyển bằng Blend Tree.
// Parameter "State": 0 = Idle, 1 = Walk. Blend Tree tự blend giữa 2 animation.
// Sử dụng Animator parameter hash để tối ưu hiệu năng.

using UnityEngine;

public class NPCAnimationController : MonoBehaviour
{
    // Blend Tree parameter: 0 = Idle, 1 = Walk
    private static readonly int StateHash = Animator.StringToHash("State");

    private Animator animator;

    [Header("=== CẤU HÌNH ANIMATION ===")]
    [Tooltip("Thời gian chuyển tiếp mượt giữa các animation (giây)")]
    [SerializeField] private float dampTime = 0.15f;

    void Awake()
    {
        // Tìm Animator trong object hoặc con (hỗ trợ model có Animator ở child)
        animator = GetComponentInChildren<Animator>();

        // Tắt Root Motion để NavMeshAgent kiểm soát hoàn toàn vị trí NPC.
        // Nếu Root Motion bật, Animator sẽ ghi đè position mỗi frame → NPC không di chuyển theo waypoint.
        if (animator != null)
        {
            animator.applyRootMotion = false;
        }
    }

    /// <summary>
    /// Cập nhật animation mỗi frame - gọi bởi NPCMovement.
    /// state: 0 = Idle, 1 = Walk
    /// </summary>
    public void UpdateAnimation(float state)
    {
        if (animator == null) return;
        animator.SetFloat(StateHash, state, dampTime, Time.deltaTime);
    }

    /// <summary>
    /// Kích hoạt trigger animation đặc biệt (vẫy tay, ngồi xuống...)
    /// </summary>
    public void PlayTrigger(string triggerName)
    {
        if (animator == null) return;
        animator.SetTrigger(triggerName);
    }

    /// <summary>
    /// Kiểm tra có Animator hay không
    /// </summary>
    public bool HasAnimator()
    {
        return animator != null;
    }
}

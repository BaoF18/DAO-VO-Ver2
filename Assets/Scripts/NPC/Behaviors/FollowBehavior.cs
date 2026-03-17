// File: FollowBehavior.cs
// Mô tả: Hành vi THEO DÕI MỤC TIÊU - NPC bám theo một Transform (player, NPC khác, object).
// Tương tự companion follow trong Skyrim (Lydia theo Dragonborn), bodyguard trong GTA V:
// - Tự động đi theo target, giữ khoảng cách an toàn
// - Chạy nếu target quá xa, dừng nếu đủ gần
// - Cập nhật đích theo interval để tối ưu hiệu năng (không gọi MoveTo mỗi frame)

using UnityEngine;

public class FollowBehavior : NPCBehaviorBase
{
    [Header("=== CẤU HÌNH THEO DÕI ===")]
    [Tooltip("Mục tiêu cần theo dõi (kéo Player hoặc NPC khác vào đây)")]
    [SerializeField] private Transform target;

    [Tooltip("Khoảng cách dừng lại khi đủ gần target")]
    [SerializeField] private float stopDistance = 3f;

    [Tooltip("Khoảng cách bắt đầu chạy (target quá xa)")]
    [SerializeField] private float runDistance = 8f;

    [Tooltip("Tần suất cập nhật vị trí đích (giây) - tối ưu hiệu năng")]
    [SerializeField] private float updateInterval = 0.3f;

    // Bộ đếm cập nhật vị trí
    private float updateTimer;

    public override void Enter()
    {
        base.Enter();
        updateTimer = 0f;
    }

    public override void Tick()
    {
        // Không có target → không làm gì
        if (target == null) return;

        // Tính khoảng cách đến target
        float distance = Vector3.Distance(transform.position, target.position);

        // --- QUÁ GẦN → DỪNG LẠI ---
        if (distance <= stopDistance)
        {
            npcMovement.Stop();
            npcMovement.SetRunning(false);
            return;
        }

        // --- QUÁ XA → CHẠY ---
        npcMovement.SetRunning(distance >= runDistance);

        // --- CẬP NHẬT VỊ TRÍ ĐÍCH THEO INTERVAL ---
        // Không gọi MoveTo mỗi frame để tối ưu hiệu năng (giống GTA V)
        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0f)
        {
            npcMovement.MoveTo(target.position);
            updateTimer = updateInterval;
        }
    }

    public override void Exit()
    {
        base.Exit();
        updateTimer = 0f;
    }

    /// <summary>
    /// Gán target từ code (dùng khi muốn đổi target runtime)
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// Lấy target hiện tại
    /// </summary>
    public Transform GetTarget()
    {
        return target;
    }
}

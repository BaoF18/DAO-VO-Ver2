// File: IdleBehavior.cs
// Mô tả: Hành vi ĐỨNG YÊN - NPC dừng tại chỗ trong khoảng thời gian nhất định.
// Tương tự NPC đứng bán hàng trong Skyrim, NPC đứng hút thuốc trong GTA V:
// - Đứng yên tại vị trí hiện tại
// - Sau X giây có thể tự động chuyển sang behavior khác
// - Nếu duration = 0 thì đứng yên vĩnh viễn

using UnityEngine;

public class IdleBehavior : NPCBehaviorBase
{
    [Header("=== CẤU HÌNH ĐỨNG YÊN ===")]
    [Tooltip("Thời gian đứng yên (giây). 0 = đứng yên vĩnh viễn")]
    [SerializeField] private float idleDuration = 0f;

    [Tooltip("Behavior tự động chuyển sang sau khi hết thời gian (tùy chọn)")]
    [SerializeField] private NPCBehaviorBase nextBehavior;

    // Bộ đếm thời gian
    private float timer;

    public override void Enter()
    {
        base.Enter();

        // Dừng mọi di chuyển
        npcMovement.Stop();
        npcMovement.SetRunning(false);

        // Reset bộ đếm
        timer = idleDuration;
    }

    public override void Tick()
    {
        // Duration = 0 → đứng yên vĩnh viễn, không cần đếm
        if (idleDuration <= 0f) return;

        // Đếm ngược thời gian
        timer -= Time.deltaTime;

        // Hết thời gian → chuyển sang behavior tiếp theo nếu có
        if (timer <= 0f && nextBehavior != null)
        {
            npcMovement.SetBehavior(nextBehavior);
        }
    }

    /// <summary>
    /// Thiết lập thời gian idle từ code
    /// </summary>
    public void SetDuration(float duration)
    {
        idleDuration = duration;
    }

    /// <summary>
    /// Thiết lập behavior chuyển tiếp từ code
    /// </summary>
    public void SetNextBehavior(NPCBehaviorBase behavior)
    {
        nextBehavior = behavior;
    }
}

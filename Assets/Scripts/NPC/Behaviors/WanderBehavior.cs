// File: WanderBehavior.cs
// Mô tả: Hành vi LANG THANG NGẪU NHIÊN - NPC đi đến các điểm ngẫu nhiên trong bán kính cho trước.
// Tương tự dân thường đi lại trong GTA V, NPC đi chợ trong Skyrim:
// - Tìm điểm ngẫu nhiên hợp lệ trên NavMesh
// - Đi đến điểm đó, dừng chờ, rồi tìm điểm mới
// - Luôn giữ trong phạm vi bán kính wanderRadius

using UnityEngine;

public class WanderBehavior : NPCBehaviorBase
{
    [Header("=== CẤU HÌNH LANG THANG ===")]
    [Tooltip("Bán kính khu vực NPC được phép lang thang (mét)")]
    [SerializeField] private float wanderRadius = 10f;

    [Tooltip("Thời gian chờ giữa mỗi lần di chuyển (giây)")]
    [SerializeField] private float waitTime = 3f;

    [Tooltip("Bật: Lấy tâm từ vị trí ban đầu. Tắt: Tâm di chuyển theo NPC")]
    [SerializeField] private bool useStartPosition = true;

    // Trạng thái nội bộ
    private Vector3 wanderCenter;
    private float waitTimer;
    private bool isWaiting;

    public override void Enter()
    {
        base.Enter();

        // Lưu tâm khu vực lang thang
        wanderCenter = transform.position;

        isWaiting = false;
        waitTimer = 0f;

        npcMovement.SetRunning(false);

        // Tìm và đi đến điểm đầu tiên
        FindNewDestination();
    }

    public override void Tick()
    {
        // --- ĐANG CHỜ ---
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                FindNewDestination();
            }
            return;
        }

        // --- ĐANG DI CHUYỂN - kiểm tra đã đến chưa ---
        if (npcMovement.IsAtDestination())
        {
            npcMovement.Stop();
            isWaiting = true;
            waitTimer = waitTime;
        }
    }

    public override void Exit()
    {
        base.Exit();
        isWaiting = false;
        waitTimer = 0f;
    }

    /// <summary>
    /// Tìm điểm ngẫu nhiên trên NavMesh và di chuyển đến
    /// </summary>
    private void FindNewDestination()
    {
        // Xác định tâm: vị trí ban đầu hoặc vị trí hiện tại
        Vector3 center = useStartPosition ? wanderCenter : transform.position;

        if (npcMovement.GetRandomPoint(center, wanderRadius, out Vector3 point))
        {
            npcMovement.MoveTo(point);
        }
        else
        {
            // Không tìm được điểm hợp lệ → chờ rồi thử lại
            isWaiting = true;
            waitTimer = waitTime * 0.5f;
        }
    }

    /// <summary>
    /// Đặt tâm khu vực lang thang từ code
    /// </summary>
    public void SetWanderCenter(Vector3 center)
    {
        wanderCenter = center;
        useStartPosition = false;
    }

    /// <summary>
    /// Đặt bán kính lang thang từ code
    /// </summary>
    public void SetWanderRadius(float radius)
    {
        wanderRadius = radius;
    }
}

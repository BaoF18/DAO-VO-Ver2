// File: PatrolBehavior.cs
// Mô tả: Hành vi TUẦN TRA - NPC đi theo chuỗi waypoint, dừng lại tại mỗi điểm rồi tiếp tục.
// Tương tự lính gác tuần tra trong Skyrim, cảnh sát đi tuần trong GTA V:
// - Đi từ waypoint 0 → 1 → 2 → ... → cuối → quay lại đầu (nếu loop)
// - Dừng chờ tại mỗi waypoint theo waitTime của từng Waypoint
// - Loop bật/tắt trực tiếp trên WaypointPath

using UnityEngine;

public class PatrolBehavior : NPCBehaviorBase
{
    [Header("=== CẤU HÌNH TUẦN TRA ===")]
    [Tooltip("Kéo WaypointPath object vào đây")]
    [SerializeField] private WaypointPath waypointPath;

    [Header("=== TÙY CHỌN ===")]
    [Tooltip("NPC chạy thay vì đi bộ khi tuần tra")]
    [SerializeField] private bool useRunning = false;

    [Tooltip("Chỉ cho phép idle khi đến waypoint cuối (không loop)")]
    [SerializeField] private bool idleOnlyAtPathEnd = false;

    // Trạng thái nội bộ
    private int currentWaypointIndex;
    private float waitTimer;
    private bool isWaiting;
    private bool hasReachedPathEnd;

    public bool HasCompletedPath()
    {
        if (waypointPath == null || waypointPath.isLoop) return false;
        if (waypointPath.Count == 0) return false;
        return hasReachedPathEnd;
    }

    public override void Enter()
    {
        base.Enter();

        if (waypointPath == null || waypointPath.Count == 0) return;

        currentWaypointIndex = 0;
        isWaiting = false;
        waitTimer = 0f;
        hasReachedPathEnd = false;

        npcMovement.SetRunning(useRunning);
        MoveToCurrentWaypoint();
    }

    public override void Tick()
    {
        if (waypointPath == null || waypointPath.Count == 0) return;

        // --- ĐANG CHỜ TẠI WAYPOINT ---
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                GoToNextWaypoint();
            }
            return;
        }

        // --- ĐANG DI CHUYỂN - kiểm tra đã đến chưa ---
        if (npcMovement.IsAtDestination())
        {
            // Lấy waitTime trực tiếp từ Waypoint hiện tại
            Waypoint wp = waypointPath.GetWaypoint(currentWaypointIndex);
            float wait = wp != null ? wp.waitTime : 2f;

            npcMovement.Stop();
            isWaiting = true;
            waitTimer = wait;
        }
    }

    public override void Exit()
    {
        base.Exit();
        isWaiting = false;
        waitTimer = 0f;
        hasReachedPathEnd = false;
    }

    private void MoveToCurrentWaypoint()
    {
        Waypoint wp = waypointPath.GetWaypoint(currentWaypointIndex);
        if (wp != null)
        {
            npcMovement.MoveTo(wp.GetPosition());
        }
    }

    private void GoToNextWaypoint()
    {
        int nextIndex = waypointPath.GetNextIndex(currentWaypointIndex);

        // Hết path và không loop → dừng hẳn
        if (nextIndex == -1)
        {
            hasReachedPathEnd = true;
            npcMovement.Stop();
            return;
        }

        currentWaypointIndex = nextIndex;
        MoveToCurrentWaypoint();
    }

    /// <summary>
    /// Gán WaypointPath từ code (dùng khi muốn đổi path runtime)
    /// </summary>
    public void SetPath(WaypointPath path)
    {
        waypointPath = path;
    }
}

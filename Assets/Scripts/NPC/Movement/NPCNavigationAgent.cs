// File: NPCNavigationAgent.cs
// Mô tả: Wrapper bao bọc NavMeshAgent của Unity - tách biệt hoàn toàn logic navigation khỏi gameplay.
// Pattern này giống GTA V và Skyrim: gameplay code KHÔNG BAO GIỜ gọi NavMesh trực tiếp.
// Mọi lệnh di chuyển đều đi qua wrapper này, giúp dễ debug, thay đổi engine navigation sau này.

using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCNavigationAgent : MonoBehaviour
{
    private NavMeshAgent agent;

    // Thời điểm gọi MoveTo() gần nhất - dùng để tránh HasReachedDestination() trả true quá sớm
    private float moveCommandTime = -1f;

    // Hàng đợi destination khi agent chưa on NavMesh - tự động retry mỗi frame
    private Vector3? pendingDestination;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // NavMeshAgent + Rigidbody conflict: Rigidbody PHẢI là kinematic.
        // Nếu không, gravity kéo NPC rời NavMesh → isOnNavMesh = false → MoveTo() không hoạt động.
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    void Update()
    {
        // Retry: khi agent đã sẵn sàng trên NavMesh, thực thi lệnh MoveTo đã bị queue trước đó
        if (pendingDestination.HasValue && agent != null && agent.isOnNavMesh)
        {
            Vector3 dest = pendingDestination.Value;
            pendingDestination = null;
            agent.isStopped = false;
            agent.SetDestination(dest);
            moveCommandTime = Time.time;
        }
    }

    // === CÁC LỆNH DI CHUYỂN ===

    /// <summary>
    /// Ra lệnh NPC di chuyển đến vị trí đích
    /// </summary>
    public void MoveTo(Vector3 destination)
    {
        if (agent == null) return;

        // Nếu agent chưa on NavMesh → queue lại, Update() sẽ retry tự động
        if (!agent.isOnNavMesh)
        {
            pendingDestination = destination;
            return;
        }

        pendingDestination = null;
        agent.isStopped = false;
        agent.SetDestination(destination);
        moveCommandTime = Time.time;
    }

    /// <summary>
    /// Dừng NPC ngay lập tức, xóa đường đi hiện tại
    /// </summary>
    public void Stop()
    {
        pendingDestination = null;
        if (agent == null || !agent.isOnNavMesh) return;
        agent.isStopped = true;
        agent.ResetPath();
        moveCommandTime = -1f;
    }

    /// <summary>
    /// Kiểm tra NPC đã đến đích chưa (đáng tin cậy cho mọi trường hợp)
    /// </summary>
    public bool HasReachedDestination()
    {
        // Có lệnh MoveTo đang chờ agent sẵn sàng → chưa đến đích
        if (pendingDestination.HasValue) return false;

        if (agent == null || !agent.isOnNavMesh) return true;

        // Đang tính toán đường đi -> chưa đến
        if (agent.pathPending) return false;

        // Grace period: sau khi gọi MoveTo(), NavMeshAgent cần vài frame để bắt đầu di chuyển.
        // Trong thời gian đó velocity = 0 và remainingDistance có thể chưa cập nhật
        // → gây false-positive "đã đến đích" → NPC skip waypoint.
        if (moveCommandTime >= 0f && Time.time - moveCommandTime < 0.2f) return false;

        // Còn xa đích -> chưa đến
        if (agent.remainingDistance > agent.stoppingDistance) return false;

        // Gần đích VÀ đã dừng hoặc hết đường -> đã đến
        if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f)
        {
            return true;
        }

        return false;
    }

    // === THIẾT LẬP THÔNG SỐ ===

    /// <summary>
    /// Đặt tốc độ di chuyển
    /// </summary>
    public void SetSpeed(float speed)
    {
        if (agent != null) agent.speed = speed;
    }

    /// <summary>
    /// Đặt khoảng cách dừng khi gần đích
    /// </summary>
    public void SetStoppingDistance(float distance)
    {
        if (agent != null) agent.stoppingDistance = distance;
    }

    /// <summary>
    /// Đặt tốc độ xoay hướng
    /// </summary>
    public void SetAngularSpeed(float speed)
    {
        if (agent != null) agent.angularSpeed = speed;
    }

    // === ĐỌC THÔNG TIN ===

    /// <summary>
    /// Lấy tốc độ di chuyển hiện tại
    /// </summary>
    public float GetCurrentSpeed()
    {
        return agent != null ? agent.velocity.magnitude : 0f;
    }

    /// <summary>
    /// Lấy vector vận tốc hiện tại
    /// </summary>
    public Vector3 GetVelocity()
    {
        return agent != null ? agent.velocity : Vector3.zero;
    }

    /// <summary>
    /// Kiểm tra agent có đang đứng trên NavMesh không
    /// </summary>
    public bool IsOnNavMesh()
    {
        return agent != null && agent.isOnNavMesh;
    }

    // === TẠM DỪNG (dùng khi NPC interact/dialogue) ===

    /// <summary>
    /// Tạm dừng di chuyển nhưng GIỮ NGUYÊN đường đi hiện tại.
    /// Khác với Stop() - Stop() xóa path, Pause() giữ path để Resume() đi tiếp.
    /// </summary>
    public void Pause()
    {
        if (agent == null || !agent.isOnNavMesh) return;
        agent.isStopped = true;
    }

    /// <summary>
    /// Tiếp tục di chuyển theo đường đi đã có (sau khi Pause)
    /// </summary>
    public void Resume()
    {
        if (agent == null || !agent.isOnNavMesh) return;
        agent.isStopped = false;
    }

    // === TIỆN ÍCH ===

    /// <summary>
    /// Tìm một điểm ngẫu nhiên hợp lệ trên NavMesh trong bán kính cho trước.
    /// Thử tối đa 10 lần, nếu không tìm được trả về vị trí trung tâm.
    /// Dùng cho WanderBehavior.
    /// </summary>
    public bool TryGetRandomPoint(Vector3 center, float radius, out Vector3 result)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * radius;
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, radius, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        result = center;
        return false;
    }
}

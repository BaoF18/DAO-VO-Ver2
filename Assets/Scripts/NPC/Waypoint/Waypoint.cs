// File: Waypoint.cs
// Mô tả: Đại diện cho MỘT ĐIỂM trên đường tuần tra của NPC.
// Tương tự hệ thống waypoint trong GTA V - mỗi NPC đi theo chuỗi các điểm đánh dấu trong thế giới.
// Mỗi waypoint có thể có thời gian chờ riêng (NPC dừng lại nhìn xung quanh, nói chuyện...).

using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [Tooltip("Thứ tự của waypoint trong path (tự động gán bởi WaypointPath)")]
    public int index;

    [Tooltip("Thời gian NPC dừng lại tại điểm này (giây)")]
    public float waitTime = 2f;

    /// <summary>
    /// Lấy vị trí thế giới của waypoint
    /// </summary>
    public Vector3 GetPosition()
    {
        return transform.position;
    }

    // Vẽ sphere vàng khi chọn waypoint trong Scene view - dễ nhìn thấy vị trí
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
        Gizmos.DrawSphere(transform.position, 0.25f);
    }
}

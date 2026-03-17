// File: WaypointPath.cs
// Mô tả: Container chứa danh sách các Waypoint tạo thành MỘT ĐƯỜNG ĐI hoàn chỉnh.
// Tương tự Path Route trong The Witcher 3 - mỗi NPC được gán một path để tuần tra.
// Hỗ trợ loop (đi vòng lặp) và tự động thu thập waypoint con.

using System.Collections.Generic;
using UnityEngine;

public class WaypointPath : MonoBehaviour
{
    [Header("=== DANH SÁCH WAYPOINT ===")]
    [SerializeField] private List<Waypoint> waypoints = new List<Waypoint>();

    [Header("=== TÙY CHỌN ===")]
    [Tooltip("Bật: NPC đi vòng lặp liên tục. Tắt: NPC dừng ở waypoint cuối")]
    public bool isLoop = true;

    // === PROPERTIES ===
    public List<Waypoint> Waypoints => waypoints;
    public int Count => waypoints.Count;

    void Awake()
    {
        CollectChildWaypoints();
    }

    /// <summary>
    /// Lấy waypoint theo index (tự động wrap nếu loop)
    /// </summary>
    public Waypoint GetWaypoint(int index)
    {
        if (waypoints == null || waypoints.Count == 0) return null;

        if (isLoop)
        {
            // Wrap index về đầu nếu vượt quá
            index = ((index % waypoints.Count) + waypoints.Count) % waypoints.Count;
        }
        else
        {
            index = Mathf.Clamp(index, 0, waypoints.Count - 1);
        }

        return waypoints[index];
    }

    /// <summary>
    /// Tính index tiếp theo. Trả về -1 nếu hết path (không loop)
    /// </summary>
    public int GetNextIndex(int currentIndex)
    {
        int next = currentIndex + 1;

        if (next >= waypoints.Count)
        {
            // Hết path: quay về đầu nếu loop, trả -1 nếu không
            return isLoop ? 0 : -1;
        }

        return next;
    }

    /// <summary>
    /// Tự động tìm tất cả Waypoint con và sắp xếp theo thứ tự hierarchy
    /// </summary>
    public void CollectChildWaypoints()
    {
        waypoints.Clear();
        Waypoint[] children = GetComponentsInChildren<Waypoint>();

        for (int i = 0; i < children.Length; i++)
        {
            children[i].index = i;
            waypoints.Add(children[i]);
        }
    }

    // Vẽ đường nối các waypoint trong Scene view
    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count < 2) return;

        // Vẽ đường thẳng nối các waypoint liền kề
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].GetPosition(), waypoints[i + 1].GetPosition());
            }
        }

        // Vẽ đường nối cuối → đầu nếu loop (màu xanh lá)
        if (isLoop && waypoints[0] != null && waypoints[waypoints.Count - 1] != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(waypoints[waypoints.Count - 1].GetPosition(), waypoints[0].GetPosition());
        }
    }
}

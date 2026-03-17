// File: WaypointSceneGizmos.cs
// Mô tả: Vẽ đường path, mũi tên hướng đi, nhãn waypoint trong Scene view.
// Bật/tắt qua menu: NPC Tools > Bật-Tắt Waypoint Gizmos
// Tương tự Debug Path Visualization trong các engine AAA.

using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class WaypointSceneGizmos
{
    // Bật/tắt hiển thị gizmos
    private static bool showGizmos = true;

    // Màu sắc
    private static readonly Color pathColor = Color.cyan;
    private static readonly Color waypointColor = Color.yellow;
    private static readonly Color loopColor = new Color(0f, 1f, 0.5f, 0.7f);

    // Đăng ký vẽ gizmos khi Editor load
    static WaypointSceneGizmos()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    // Menu bật/tắt
    [MenuItem("NPC Tools/Bật-Tắt Waypoint Gizmos")]
    private static void ToggleGizmos()
    {
        showGizmos = !showGizmos;
        SceneView.RepaintAll();
    }

    /// <summary>
    /// Vẽ gizmos cho tất cả WaypointPath trong scene
    /// </summary>
    private static void OnSceneGUI(SceneView sceneView)
    {
        if (!showGizmos) return;

        // Tìm tất cả path trong scene
        WaypointPath[] paths = Object.FindObjectsByType<WaypointPath>(FindObjectsSortMode.None);

        foreach (var path in paths)
        {
            DrawPath(path);
        }
    }

    /// <summary>
    /// Vẽ chi tiết một path: sphere, nhãn, đường nối, mũi tên
    /// </summary>
    private static void DrawPath(WaypointPath path)
    {
        if (path == null || path.Waypoints == null || path.Waypoints.Count == 0) return;

        var waypoints = path.Waypoints;

        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null) continue;

            Vector3 pos = waypoints[i].GetPosition();

            // Vẽ sphere tại vị trí waypoint
            Handles.color = waypointColor;
            Handles.SphereHandleCap(0, pos, Quaternion.identity, 0.5f, EventType.Repaint);

            // Hiển thị nhãn: index + thời gian chờ
            Handles.Label(
                pos + Vector3.up * 0.8f,
                $"[{i}] {waypoints[i].waitTime:F1}s",
                EditorStyles.boldLabel);

            // Vẽ đường nối đến waypoint tiếp theo + mũi tên hướng
            if (i < waypoints.Count - 1 && waypoints[i + 1] != null)
            {
                Vector3 nextPos = waypoints[i + 1].GetPosition();

                Handles.color = pathColor;
                Handles.DrawLine(pos, nextPos, 2f);

                // Mũi tên ở giữa đoạn đường
                Vector3 direction = (nextPos - pos).normalized;
                Vector3 midPoint = (pos + nextPos) * 0.5f;
                DrawArrow(midPoint, direction, 0.5f);
            }
        }

        // Vẽ đường nối cuối → đầu (nét đứt) nếu loop
        if (path.isLoop && waypoints.Count > 1)
        {
            Waypoint first = waypoints[0];
            Waypoint last = waypoints[waypoints.Count - 1];

            if (first != null && last != null)
            {
                Handles.color = loopColor;
                Handles.DrawDottedLine(last.GetPosition(), first.GetPosition(), 4f);

                // Mũi tên loop
                Vector3 direction = (first.GetPosition() - last.GetPosition()).normalized;
                Vector3 midPoint = (last.GetPosition() + first.GetPosition()) * 0.5f;
                DrawArrow(midPoint, direction, 0.5f);
            }
        }
    }

    /// <summary>
    /// Vẽ mũi tên chỉ hướng di chuyển
    /// </summary>
    private static void DrawArrow(Vector3 position, Vector3 direction, float size)
    {
        if (direction == Vector3.zero) return;
        Quaternion rotation = Quaternion.LookRotation(direction);
        Handles.ConeHandleCap(0, position, rotation, size, EventType.Repaint);
    }
}

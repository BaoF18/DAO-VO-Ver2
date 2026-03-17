// File: WaypointPathEditor.cs
// Mô tả: Custom Inspector cho WaypointPath - thêm nút tiện ích ngay trong Inspector.
// Giúp level designer tạo/xóa/thu thập waypoint nhanh mà không cần code.

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WaypointPath))]
public class WaypointPathEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Vẽ Inspector mặc định
        DrawDefaultInspector();

        WaypointPath path = (WaypointPath)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("=== CÔNG CỤ ===", EditorStyles.boldLabel);

        // Nút: Thu thập tự động tất cả waypoint con
        if (GUILayout.Button("Thu thập Waypoint con"))
        {
            Undo.RecordObject(path, "Collect Waypoints");
            path.CollectChildWaypoints();
            EditorUtility.SetDirty(path);
        }

        // Nút: Thêm waypoint mới
        if (GUILayout.Button("Thêm Waypoint"))
        {
            AddWaypoint(path);
        }

        // Nút: Xóa tất cả waypoint
        if (GUILayout.Button("Xóa tất cả Waypoint"))
        {
            if (EditorUtility.DisplayDialog("Xác nhận xóa", "Xóa tất cả waypoint trong path này?", "Xóa", "Hủy"))
            {
                ClearWaypoints(path);
            }
        }

        // Hiển thị thông tin tóm tắt
        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox(
            $"Số waypoint: {path.Count}\nLoop: {(path.isLoop ? "Bật" : "Tắt")}",
            MessageType.Info);
    }

    /// <summary>
    /// Tạo waypoint mới làm con của path
    /// </summary>
    private void AddWaypoint(WaypointPath path)
    {
        GameObject wpObject = new GameObject($"Waypoint_{path.Count}");
        wpObject.transform.SetParent(path.transform);

        // Đặt vị trí: sau waypoint cuối hoặc tại vị trí path
        if (path.Count > 0)
        {
            Waypoint last = path.GetWaypoint(path.Count - 1);
            if (last != null)
            {
                wpObject.transform.position = last.GetPosition() + Vector3.forward * 2f;
            }
        }
        else
        {
            wpObject.transform.position = path.transform.position;
        }

        Waypoint wp = wpObject.AddComponent<Waypoint>();
        wp.index = path.Count;

        Undo.RegisterCreatedObjectUndo(wpObject, "Add Waypoint");

        // Cập nhật danh sách
        path.CollectChildWaypoints();
        EditorUtility.SetDirty(path);

        // Chọn waypoint mới tạo
        Selection.activeGameObject = wpObject;
    }

    /// <summary>
    /// Xóa toàn bộ waypoint con
    /// </summary>
    private void ClearWaypoints(WaypointPath path)
    {
        while (path.transform.childCount > 0)
        {
            Undo.DestroyObjectImmediate(path.transform.GetChild(0).gameObject);
        }

        path.CollectChildWaypoints();
        EditorUtility.SetDirty(path);
    }
}

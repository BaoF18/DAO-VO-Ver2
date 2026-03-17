// File: WaypointEditorWindow.cs
// Mô tả: Editor Window chuyên dụng để tạo và quản lý waypoint nhanh trong Scene.
// Mở bằng menu: NPC Tools > Waypoint Editor
// Tương tự công cụ Path Editor trong các engine AAA - cho phép level designer thao tác trực quan.

using UnityEngine;
using UnityEditor;

public class WaypointEditorWindow : EditorWindow
{
    private WaypointPath selectedPath;
    private float defaultWaitTime = 2f;
    private bool autoSelect = true;
    private Vector2 scrollPosition;

    // Mở cửa sổ từ menu
    [MenuItem("NPC Tools/Waypoint Editor")]
    public static void ShowWindow()
    {
        GetWindow<WaypointEditorWindow>("Waypoint Editor");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("WAYPOINT EDITOR", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // === CHỌN PATH ===
        selectedPath = (WaypointPath)EditorGUILayout.ObjectField(
            "Path", selectedPath, typeof(WaypointPath), true);
        defaultWaitTime = EditorGUILayout.FloatField("Thời gian chờ mặc định", defaultWaitTime);
        autoSelect = EditorGUILayout.Toggle("Tự động chọn waypoint mới", autoSelect);

        EditorGUILayout.Space(10);

        // Chưa chọn path → hiện hướng dẫn
        if (selectedPath == null)
        {
            EditorGUILayout.HelpBox("Kéo WaypointPath vào ô trên, hoặc tạo mới.", MessageType.Info);

            EditorGUILayout.Space(5);
            if (GUILayout.Button("Tạo Path mới trong Scene"))
            {
                CreateNewPath();
            }
            return;
        }

        // === CÁC NÚT THAO TÁC ===
        EditorGUILayout.LabelField("=== THAO TÁC ===", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Thêm Waypoint"))
        {
            AddWaypointToPath();
        }
        if (GUILayout.Button("Thêm tại vị trí Camera"))
        {
            AddWaypointAtSceneView();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Thu thập Waypoint con"))
        {
            selectedPath.CollectChildWaypoints();
            EditorUtility.SetDirty(selectedPath);
        }
        if (GUILayout.Button("Xóa tất cả"))
        {
            if (EditorUtility.DisplayDialog("Xác nhận", "Xóa tất cả waypoint?", "Xóa", "Hủy"))
            {
                ClearPath();
            }
        }
        EditorGUILayout.EndHorizontal();

        // === DANH SÁCH WAYPOINT ===
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField($"=== WAYPOINTS ({selectedPath.Count}) ===", EditorStyles.boldLabel);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        for (int i = 0; i < selectedPath.Waypoints.Count; i++)
        {
            Waypoint wp = selectedPath.Waypoints[i];
            if (wp == null) continue;

            EditorGUILayout.BeginHorizontal("box");

            // Tên waypoint
            EditorGUILayout.LabelField($"[{i}] {wp.gameObject.name}", GUILayout.Width(150));

            // Thời gian chờ
            wp.waitTime = EditorGUILayout.FloatField(wp.waitTime, GUILayout.Width(50));

            // Nút chọn waypoint trong scene
            if (GUILayout.Button("Chọn", GUILayout.Width(50)))
            {
                Selection.activeGameObject = wp.gameObject;
                if (SceneView.lastActiveSceneView != null)
                {
                    SceneView.lastActiveSceneView.Frame(
                        new Bounds(wp.GetPosition(), Vector3.one * 3f), false);
                }
            }

            // Nút xóa waypoint
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                Undo.DestroyObjectImmediate(wp.gameObject);
                selectedPath.CollectChildWaypoints();
                EditorUtility.SetDirty(selectedPath);
                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// Tạo WaypointPath mới trong scene
    /// </summary>
    private void CreateNewPath()
    {
        GameObject pathObject = new GameObject("WaypointPath_New");
        selectedPath = pathObject.AddComponent<WaypointPath>();
        Undo.RegisterCreatedObjectUndo(pathObject, "Create Waypoint Path");
        Selection.activeGameObject = pathObject;
    }

    /// <summary>
    /// Thêm waypoint vào cuối path
    /// </summary>
    private void AddWaypointToPath()
    {
        GameObject wpObject = new GameObject($"Waypoint_{selectedPath.Count}");
        wpObject.transform.SetParent(selectedPath.transform);

        if (selectedPath.Count > 0)
        {
            Waypoint last = selectedPath.GetWaypoint(selectedPath.Count - 1);
            if (last != null)
                wpObject.transform.position = last.GetPosition() + Vector3.forward * 2f;
        }
        else
        {
            wpObject.transform.position = selectedPath.transform.position;
        }

        Waypoint wp = wpObject.AddComponent<Waypoint>();
        wp.waitTime = defaultWaitTime;

        Undo.RegisterCreatedObjectUndo(wpObject, "Add Waypoint");
        selectedPath.CollectChildWaypoints();
        EditorUtility.SetDirty(selectedPath);

        if (autoSelect) Selection.activeGameObject = wpObject;
    }

    /// <summary>
    /// Thêm waypoint tại vị trí Scene camera đang nhìn (raycast xuống mặt đất)
    /// </summary>
    private void AddWaypointAtSceneView()
    {
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView == null) return;

        // Bắn tia từ camera xuống để tìm mặt đất
        Ray ray = new Ray(
            sceneView.camera.transform.position,
            sceneView.camera.transform.forward);

        Vector3 position;
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            position = hit.point;
        }
        else
        {
            position = sceneView.camera.transform.position +
                       sceneView.camera.transform.forward * 10f;
        }

        GameObject wpObject = new GameObject($"Waypoint_{selectedPath.Count}");
        wpObject.transform.SetParent(selectedPath.transform);
        wpObject.transform.position = position;

        Waypoint wp = wpObject.AddComponent<Waypoint>();
        wp.waitTime = defaultWaitTime;

        Undo.RegisterCreatedObjectUndo(wpObject, "Add Waypoint at Scene View");
        selectedPath.CollectChildWaypoints();
        EditorUtility.SetDirty(selectedPath);

        if (autoSelect) Selection.activeGameObject = wpObject;
    }

    /// <summary>
    /// Xóa toàn bộ waypoint trong path
    /// </summary>
    private void ClearPath()
    {
        while (selectedPath.transform.childCount > 0)
        {
            Undo.DestroyObjectImmediate(selectedPath.transform.GetChild(0).gameObject);
        }
        selectedPath.CollectChildWaypoints();
        EditorUtility.SetDirty(selectedPath);
    }
}

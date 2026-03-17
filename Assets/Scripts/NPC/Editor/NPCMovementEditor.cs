// File: NPCMovementEditor.cs
// Mô tả: Custom Inspector cho NPCMovement - hiển thị debug info runtime và nút chuyển behavior nhanh.
// Giúp debug trực tiếp trong Play mode: xem behavior đang chạy, tốc độ, trạng thái...

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NPCMovement))]
public class NPCMovementEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Vẽ Inspector mặc định
        DrawDefaultInspector();

        NPCMovement npc = (NPCMovement)target;

        // Debug info chỉ hiện khi đang Play
        if (!Application.isPlaying) return;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("=== DEBUG INFO (Runtime) ===", EditorStyles.boldLabel);

        // Behavior hiện tại
        NPCBehaviorBase currentBehavior = npc.GetCurrentBehavior();
        string behaviorName = currentBehavior != null ? currentBehavior.GetType().Name : "Không có";
        EditorGUILayout.LabelField("Behavior hiện tại:", behaviorName);

        // Tốc độ di chuyển
        EditorGUILayout.LabelField("Tốc độ:", $"{npc.GetCurrentSpeed():F2} m/s");

        // Trạng thái
        EditorGUILayout.LabelField("Đang chạy:", npc.IsRunning ? "Có" : "Không");
        EditorGUILayout.LabelField("Đã đến đích:", npc.IsAtDestination() ? "Có" : "Không");

        // === NÚT CHUYỂN BEHAVIOR NHANH ===
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("=== CHUYỂN BEHAVIOR NHANH ===", EditorStyles.boldLabel);

        // Tìm tất cả behavior trên NPC
        NPCBehaviorBase[] behaviors = npc.GetComponents<NPCBehaviorBase>();
        foreach (var behavior in behaviors)
        {
            string typeName = behavior.GetType().Name;
            bool isCurrent = behavior == currentBehavior;
            string label = isCurrent ? $"● {typeName} (đang chạy)" : $"Kích hoạt: {typeName}";

            EditorGUI.BeginDisabledGroup(isCurrent);
            if (GUILayout.Button(label))
            {
                npc.SetBehavior(behavior);
            }
            EditorGUI.EndDisabledGroup();
        }

        // Repaint liên tục khi Play mode để cập nhật debug info
        Repaint();
    }
}

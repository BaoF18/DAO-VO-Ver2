// File: WaypointPathConfig.cs
// Mô tả: ScriptableObject chứa cấu hình mặc định cho WaypointPath.
// Dùng để tạo preset: "Tuần tra nhanh", "Tuần tra chậm", "Tuần tra ngẫu nhiên"...
// Tương tự AI Preset system trong Skyrim - mỗi loại NPC dùng preset khác nhau.

using UnityEngine;

[CreateAssetMenu(fileName = "WaypointPathConfig", menuName = "NPC/Waypoint Path Config")]
public class WaypointPathConfig : ScriptableObject
{
    [Header("=== THỜI GIAN CHỜ ===")]
    [Tooltip("Thời gian chờ mặc định tại mỗi waypoint (giây)")]
    public float defaultWaitTime = 2f;

    [Header("=== TÙY CHỌN ĐƯỜNG ĐI ===")]
    [Tooltip("NPC đi vòng lặp khi hết path")]
    public bool loop = true;

    [Tooltip("Bắt đầu từ waypoint ngẫu nhiên thay vì waypoint đầu tiên")]
    public bool randomStart = false;
}

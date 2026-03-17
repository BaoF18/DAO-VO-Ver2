// File: NPCMovementConfig.cs
// Mô tả: ScriptableObject chứa toàn bộ thông số di chuyển cho NPC.
// Mỗi loại NPC (lính gác, thương nhân, quái vật) có config riêng - tương tự hệ thống AI Profile trong The Witcher 3 và Skyrim.
// Dùng ScriptableObject để dễ tái sử dụng, chỉnh sửa trong Inspector mà không cần sửa code.

using UnityEngine;

[CreateAssetMenu(fileName = "NPCMovementConfig", menuName = "NPC/Movement Config")]
public class NPCMovementConfig : ScriptableObject
{
    [Header("=== TỐC ĐỘ DI CHUYỂN ===")]
    [Tooltip("Tốc độ đi bộ bình thường")]
    public float walkSpeed = 2f;

    [Tooltip("Tốc độ chạy khi cần di chuyển nhanh")]
    public float runSpeed = 5f;

    [Header("=== DỪNG VÀ XOAY ===")]
    [Tooltip("Khoảng cách tối thiểu để coi là đã đến đích")]
    public float stoppingDistance = 0.5f;

    [Tooltip("Tốc độ xoay hướng NPC (độ/giây)")]
    public float rotationSpeed = 120f;
}

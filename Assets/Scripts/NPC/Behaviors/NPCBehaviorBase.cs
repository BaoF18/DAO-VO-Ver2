// File: NPCBehaviorBase.cs
// Mô tả: Lớp trừu tượng cơ sở cho TẤT CẢ hành vi NPC (tuần tra, đứng yên, theo dõi, lang thang).
// Tương tự AIBehavior base class trong The Witcher 3 và GTA V:
// - Enter(): Khởi tạo khi hành vi bắt đầu
// - Tick(): Chạy mỗi frame khi hành vi đang hoạt động
// - Exit(): Dọn dẹp khi chuyển sang hành vi khác
// Mọi behavior PHẢI kế thừa class này. Behavior chỉ gọi NPCMovement, KHÔNG gọi NavMesh trực tiếp.

using UnityEngine;

public abstract class NPCBehaviorBase : MonoBehaviour
{
    // Tham chiếu đến NPCMovement - cầu nối duy nhất để điều khiển NPC
    protected NPCMovement npcMovement;

    // Trạng thái hoạt động của behavior
    protected bool isActive;

    /// <summary>
    /// Khởi tạo tham chiếu - được gọi tự động bởi NPCMovement.SetBehavior()
    /// </summary>
    public void Initialize(NPCMovement movement)
    {
        npcMovement = movement;
    }

    /// <summary>
    /// Gọi khi behavior BẮT ĐẦU hoạt động
    /// </summary>
    public virtual void Enter()
    {
        isActive = true;
    }

    /// <summary>
    /// Gọi MỖI FRAME khi behavior đang hoạt động - override để viết logic
    /// </summary>
    public abstract void Tick();

    /// <summary>
    /// Gọi khi behavior KẾT THÚC (chuyển sang behavior khác)
    /// </summary>
    public virtual void Exit()
    {
        isActive = false;
        if (npcMovement != null) npcMovement.Stop();
    }
}

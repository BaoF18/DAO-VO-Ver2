using UnityEngine;

public class VehicleJobConfig : MonoBehaviour
{
    [Header("Cấu hình nhiệm vụ sửa xe")]
    [Tooltip("Chữ sẽ hiện lên Label nổi")]
    public string jobName = "Bơm xe";

    [Tooltip("Thoại khi mới vào gặp (Nhờ vả)")]
    public DialogueData preRepairDialogue;

    [Tooltip("Thoại khi sửa xong (Tính tiền)")]
    public DialogueData postRepairDialogue;
}
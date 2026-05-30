using UnityEngine;

public class NPCTaskIndicator : MonoBehaviour
{
    [Header("Định danh NPC")]
    [Tooltip("Phải nhập chữ giống hệt ô Target Id trong bảng Task Manager")]
    public string npcId;

    [Header("Giao diện trên đầu")]
    [Tooltip("Kéo object chữ [E] trên đỉnh đầu NPC vào đây")]
    public GameObject overheadUI;

    [Header("Tùy chọn hiển thị")]
    [Tooltip("Khoảng cách tối đa để hiện chữ E (Lại gần mới hiện)")]
    public float showDistance = 5f;
    [Tooltip("Luôn xoay chữ E về phía Camera để người chơi dễ đọc")]
    public bool faceCamera = true;

    private Transform playerTransform;

    void Start()
    {
        // Tự động dò tìm Player trên bản đồ (Nhớ set Tag "Player" cho nhân vật của bạn nhé)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        if (overheadUI != null) overheadUI.SetActive(false);
    }

    void Update()
    {
        if (overheadUI == null) return;

        bool isTaskTarget = false;

        // 1. CHỐT ĐIỀU KIỆN TASK: Kiểm tra xem NPC này có đang bị "gọi tên" trong Task không
        if (TaskManager.Instance != null && TaskManager.Instance.CurrentTask != null)
        {
            if (TaskManager.Instance.CurrentTask.State == TaskState.Active &&
                string.Equals(TaskManager.Instance.CurrentTask.TargetId, npcId, System.StringComparison.Ordinal))
            {
                isTaskTarget = true;
            }
        }

        // 2. CHỐT ĐIỀU KIỆN KHOẢNG CÁCH: Lại gần mới hiện
        bool isCloseEnough = false;
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            if (distance <= showDistance) isCloseEnough = true;
        }

        // 3. HIỂN THỊ: Chỉ bật chữ E khi CÓ TASK và ĐỨNG GẦN
        bool shouldShow = isTaskTarget && isCloseEnough;
        overheadUI.SetActive(shouldShow);

        // 4. HIỆU ỨNG BILLBOARD: Xoay chữ về hướng Camera để không bị ngược
        if (shouldShow && faceCamera && Camera.main != null)
        {
            overheadUI.transform.rotation = Camera.main.transform.rotation;
        }
    }
}
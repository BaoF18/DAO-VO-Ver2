using UnityEngine;

public class EventTriggerZone : MonoBehaviour
{
    [Header("Liên kết với Spawner")]
    public NPCEventSpawner spawnerSystem;

    [Header("Cài đặt Sự kiện cụ thể này")]
    public Transform mySpawnPoint; // NPC xuất hiện ở đâu?
    public WaypointPath myRoute;   // Đi theo đường nào?

    private bool hasTriggered = false; // Chỉ cho phép chạy 1 lần

    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem người đạp bẫy có phải là Player không
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;

            // Gọi Spawner ra tay
            if (spawnerSystem != null)
            {
                spawnerSystem.SpawnNPCForEvent(mySpawnPoint, myRoute);
            }
        }
    }
}

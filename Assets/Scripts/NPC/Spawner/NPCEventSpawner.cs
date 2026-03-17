using UnityEngine;

public class NPCEventSpawner : MonoBehaviour
{
    [Header("Bản mẫu NPC (Phải có NPCMovement)")]
    public GameObject npcPrefab;

    /// <summary>
    /// Hàm này được gọi bởi các Sự kiện.
    /// Nhận vào: Chỗ xuất hiện và Đường đi
    /// </summary>
    public void SpawnNPCForEvent(Transform spawnPoint, WaypointPath targetPath)
    {
        if (npcPrefab == null || spawnPoint == null || targetPath == null)
        {
            Debug.LogWarning("Thiếu thông tin! Không thể spawn NPC.");
            return;
        }

        // 1. Tạo NPC tại điểm Spawn
        GameObject newNPC = Instantiate(npcPrefab, spawnPoint.position, spawnPoint.rotation);

        // 2. Nắm lấy "Bộ não" của NPC
        NPCMovement movement = newNPC.GetComponent<NPCMovement>();
        if (movement != null)
        {
            // 3. Lấy băng cát-xét "Tuần tra" (PatrolBehavior)
            PatrolBehavior patrol = newNPC.GetComponent<PatrolBehavior>();
            if (patrol == null)
            {
                // Nếu prefab chưa có sẵn script này thì tự động gắn vào
                patrol = newNPC.AddComponent<PatrolBehavior>();
            }

            // 4. Nạp đường đi của Sự kiện vào băng cát-xét
            patrol.SetPath(targetPath);

            // 5. Ra lệnh cho Bộ não chạy cái băng này
            movement.SetBehavior(patrol);

            Debug.Log($"Sự kiện: Đã spawn NPC đi theo đường {targetPath.gameObject.name}");
        }
    }
}

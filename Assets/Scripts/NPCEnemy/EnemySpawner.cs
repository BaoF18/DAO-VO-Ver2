using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Liên kết Task")]
    public string requiredTaskId = "5"; // ID của Task đánh nhau (bạn tự set)

    [Header("Cấu hình Đợt Quái (Waves)")]
    public GameObject enemyPrefab;

    [Tooltip("Số lượng quái mỗi đợt. Ví dụ: đợt 1 (3 con), đợt 2 (5 con), đợt 3 (10 con)")]
    public int[] waveCounts = new int[] { 3, 5, 10 };

    [Tooltip("Khoảng cách tủa ra xung quanh để quái không đẻ đè lên nhau")]
    public float spawnRadius = 3f;

    [Tooltip("Thời gian cho player bơm máu giữa 2 đợt")]
    public float delayBetweenWaves = 2f;

    private bool isTaskActive = false;
    private bool isTaskCompleted = false;
    private int currentWaveIndex = 0;
    private bool isSpawningWave = false;

    // Cuốn sổ tử thần: Ghi danh các quái đang còn sống
    private List<GameObject> activeEnemies = new List<GameObject>();

    void Update()
    {
        // 1. CHỐT KÍCH HOẠT: Chỉ chạy khi TaskManager chuyển sang Task Đánh nhau
        if (!isTaskActive && !isTaskCompleted && TaskManager.Instance != null && TaskManager.Instance.CurrentTask != null)
        {
            if (TaskManager.Instance.CurrentTask.Id == requiredTaskId)
            {
                isTaskActive = true;
                Debug.Log($"[EnemySpawner] Giang hồ kéo tới! Bắt đầu Đợt 1!");
                StartCoroutine(SpawnWaveRoutine());
            }
        }

        // 2. THEO DÕI SINH TỬ: Quét xem còn bao nhiêu đứa sống
        if (isTaskActive && !isSpawningWave && activeEnemies != null)
        {
            // PHÉP THUẬT Ở ĐÂY: Xóa ngay những con đã bị Destroy khỏi sổ
            activeEnemies.RemoveAll(enemy => enemy == null);

            // Nếu sổ trống trơn = Đã quét sạch đợt hiện tại
            if (activeEnemies.Count == 0)
            {
                currentWaveIndex++; // Tăng số đợt lên

                if (currentWaveIndex >= waveCounts.Length)
                {
                    // Chúc mừng! Đã qua hết các đợt
                    FinishTask();
                }
                else
                {
                    // Nghỉ mệt xíu rồi gọi đợt tiếp theo
                    Debug.Log($"[EnemySpawner] Sạch bóng! Chuẩn bị Đợt {currentWaveIndex + 1}!");
                    StartCoroutine(SpawnWaveRoutine());
                }
            }
        }
    }

    private IEnumerator SpawnWaveRoutine()
    {
        isSpawningWave = true;

        // Từ đợt 2 trở đi mới có thời gian nghỉ giữa hiệp
        if (currentWaveIndex > 0)
        {
            yield return new WaitForSeconds(delayBetweenWaves);
        }

        int enemiesToSpawn = waveCounts[currentWaveIndex];

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            if (enemyPrefab != null)
            {
                // Dùng toán học tản quái ra xung quanh Spawner (Trục X và Z) để tụi nó khỏi kẹt nhau
                Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
                Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

                GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, transform.rotation);

                // Đẻ ra con nào, ghi tên vào sổ tử thần con đó
                activeEnemies.Add(newEnemy);
            }
        }

        isSpawningWave = false;
    }

    private void FinishTask()
    {
        isTaskActive = false;
        isTaskCompleted = true;

        // Báo cáo hoàn thành Task cuối cùng!
        if (TaskManager.Instance != null && TaskManager.Instance.CurrentTask != null)
        {
            TaskManager.Instance.CompleteTask(TaskManager.Instance.CurrentTask);
            Debug.Log("[EnemySpawner] Trùm cuối đã bị hạ! HOÀN THÀNH TASK 5 CỐT TRUYỆN!");
        }
    }
}
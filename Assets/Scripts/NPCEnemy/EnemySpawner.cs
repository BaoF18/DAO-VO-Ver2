using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Spawn Settings")]
    public GameObject enemyPrefab;
    public float delayTime = 60f;  

    void Start()
    {
        // Vừa vào game là bắt đầu bấm giờ luôn
        StartCoroutine(SpawnEnemyRoutine());
    }

    private IEnumerator SpawnEnemyRoutine()
    {
        Debug.Log("⏳ Bắt đầu đếm ngược " + delayTime + " giây để thả quái...");

        // Chờ đúng thời gian quy định (60s)
        yield return new WaitForSeconds(delayTime);

        // Hết thời gian -> Tiến hành triệu hồi
        if (enemyPrefab != null)
        {
            // Sinh ra quái vật ngay tại vị trí và góc xoay của cục Spawner này
            Instantiate(enemyPrefab, transform.position, transform.rotation);
            Debug.Log("Enemy spawned!");
        }
        else
        {
            Debug.LogError("There is no enemy prefab there...");
        }
    }
}
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Respawn Settings")]
    public Transform respawnPoint;

    private Rigidbody rb;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("💔Received" + damage + " damage. Remains " + currentHealth);

        if (currentHealth <= 0)
        {
            DieAndRespawn();
        }
    }

    private void DieAndRespawn()
    {
        Debug.Log("💀WASTED!");

        // 1. Bơm lại đầy máu
        currentHealth = maxHealth;

        // 2. Tắt gia tốc vật lý để lúc dịch chuyển không bị bay lung tung
        if (rb != null) rb.linearVelocity = Vector3.zero;

        // 3. Dịch chuyển về điểm lưu (Checkpoint)
        if (respawnPoint != null)
        {
            // Tắt nội tại vật lý 1 nhịp để dịch chuyển an toàn tuyệt đối
            rb.isKinematic = true;
            transform.position = respawnPoint.position;
            rb.isKinematic = false;
        }
        else
        {
            Debug.LogError("⚠️ No Spawnpoint set");
        }
    }
}
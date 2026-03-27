using UnityEngine;
using UnityEngine.UI;
using System.Collections; // Để xài Coroutine

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI & Respawn")]
    public Image healthBarFill;
    public Transform respawnPoint;

    private Rigidbody rb;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        UpdateHealthBar();

        if (UIManager.Instance != null && UIManager.Instance.damagePopupPrefab != null)
        {
            // Vị trí nảy số
            Vector3 popupPos = transform.position + Vector3.up * 1.5f;
            GameObject popupObj = Instantiate(UIManager.Instance.damagePopupPrefab, popupPos, Quaternion.identity);

            DamagePopup popupScript = popupObj.GetComponent<DamagePopup>();
            if (popupScript != null)
            {
                popupScript.Setup(damage);
            }
        }

        Debug.Log("Player bị đấm! Máu còn: " + currentHealth);

        if (currentHealth <= 0)
        {
            StartCoroutine(HandlePlayerDeath());
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    private IEnumerator HandlePlayerDeath()
    {
        isDead = true;
        Debug.Log("💀 WASTED!");

        // 1. Tắt vật lý để nhân vật ngã ra (nếu có anim) hoặc không bị đẩy trượt đi
        if (rb != null) rb.linearVelocity = Vector3.zero;

        // 2. Bật màn hình mờ
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowDeathScreen();
        }

        // 3. Đếm ngược 3 giây
        int waitTime = 3;
        while (waitTime > 0)
        {
            // Báo UIManager đổi số hiển thị
            if (UIManager.Instance != null) UIManager.Instance.UpdateCountdown(waitTime);

            yield return new WaitForSeconds(1f); // Đợi 1 giây thực tế
            waitTime--; // Trừ đi 1
        }

        // 4. Hết thời gian -> Tiến hành hồi sinh
        DieAndRespawn();
    }

    private void DieAndRespawn()
    {
        // Phục hồi máu và trạng thái
        currentHealth = maxHealth;
        isDead = false;
        UpdateHealthBar();

        // Dịch chuyển về điểm Checkpoint
        if (respawnPoint != null)
        {
            if (rb != null) rb.isKinematic = true;
            transform.position = respawnPoint.position;
            if (rb != null) rb.isKinematic = false;
        }

        // Tắt Death Screen đi để chơi tiếp
        if (UIManager.Instance != null && UIManager.Instance.deathScreenGroup != null)
        {
            UIManager.Instance.deathScreenGroup.SetActive(false);
        }
    }
}
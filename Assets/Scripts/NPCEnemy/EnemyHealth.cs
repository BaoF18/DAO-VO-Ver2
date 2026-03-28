using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro; 

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI")]
    public Image healthBarFill;
    public TextMeshProUGUI hpText; 

    private bool isDead = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI(); // Gọi hàm cập nhật UI mới
    }

    public void TakeDamage(int damageAmount, Vector3 attackerPosition)
    {
        if (isDead) return; // Nếu chết rồi thì miễn nhiễm sát thương

        currentHealth -= damageAmount;

        // Đảm bảo máu hiển thị không bị kẹt ở số âm (ví dụ: -10 / 100)
        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthUI(); // Cập nhật cả thanh máu và con số

        if (UIManager.Instance != null && UIManager.Instance.damagePopupPrefab != null)
        {
            // Vị trí nảy số
            Vector3 popupPos = transform.position + Vector3.up * 1.5f;

            // Sinh ra cục Text
            GameObject popupObj = Instantiate(UIManager.Instance.damagePopupPrefab, popupPos, Quaternion.identity);

            // Gắn số sát thương vào Text
            DamagePopup popupScript = popupObj.GetComponent<DamagePopup>();
            if (popupScript != null)
            {
                popupScript.Setup(damageAmount);
            }
        }
        Debug.Log("💥 Enemy lost " + damageAmount + " Health. Remains " + currentHealth + " ❤");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Gộp chung việc cập nhật thanh đỏ và chữ số vào 1 hàm
    void UpdateHealthUI()
    {
        // 1. Cập nhật thanh đỏ tụt
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }

        // 2. Cập nhật số (dạng "Hiện tại / Tối đa")
        if (hpText != null)
        {
            hpText.text = currentHealth + " / " + maxHealth;
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Knock out!");

        // Tắt Canvas chứa thanh máu đi cho gọn trước khi quái bốc hơi
        if (healthBarFill != null && healthBarFill.canvas != null)
        {
            healthBarFill.canvas.gameObject.SetActive(false);
        }

        Destroy(gameObject);
    }
}
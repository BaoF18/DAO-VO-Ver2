using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;
    [Header("UI")]
    public Image healthBarFill;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(int damageAmount, Vector3 attackerPosition)
    {
        currentHealth -= damageAmount;
        UpdateHealthBar();
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
        Debug.Log("💥 Enemy lost " + damageAmount + " Health. Remains" + currentHealth + " ❤");

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
    }


    void Die()
    {
        Debug.Log("Knock out!");
        Destroy(gameObject);
    }
}

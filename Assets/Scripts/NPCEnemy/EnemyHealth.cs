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
    public GameObject damagePopupPrefab;
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
        if (damagePopupPrefab != null)
        {
            Vector3 popupPos = transform.position + Vector3.up * 0.5f;

            GameObject popupObj = Instantiate(damagePopupPrefab, popupPos, Quaternion.identity);

            //DamagePopup popupScript = popupObj.GetComponent<DamagePopup>();
            //if (popupScript != null)
            //{
            //    popupScript.Setup(damageAmount);
            //}
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

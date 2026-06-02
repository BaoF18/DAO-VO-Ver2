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


    [Header("--- SOUND EFFECTS ---")]
    [Tooltip("Kéo AudioSource của con quái vào đây")]
    public AudioSource audioSource;
    [Tooltip("Tiếng đấm trúng")]
    public AudioClip[] hitSounds;
    [Tooltip("Tiếng quái la hét khi chết")]
    public AudioClip[] deathSounds;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(int damageAmount, Vector3 attackerPosition)
    {
        if (isDead) return;

        // =======================================================
        // PHÁT TIẾNG ĐẤM TRÚNG (IMPACT)
        // =======================================================
        if (audioSource != null && hitSounds != null && hitSounds.Length > 0)
        {
            AudioClip randomHit = hitSounds[Random.Range(0, hitSounds.Length)];
            audioSource.PlayOneShot(randomHit);
        }
        // =======================================================

        currentHealth -= damageAmount;

        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthUI();

        if (UIManager.Instance != null && UIManager.Instance.damagePopupPrefab != null)
        {
            Vector3 popupPos = transform.position + Vector3.up * 1.5f;
            GameObject popupObj = Instantiate(UIManager.Instance.damagePopupPrefab, popupPos, Quaternion.identity);

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

    void UpdateHealthUI()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }

        if (hpText != null)
        {
            hpText.text = currentHealth + " / " + maxHealth;
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Knock out!");

        // =======================================================
        // PHÁT TIẾNG CHẾT (DÙNG LOA VÔ HÌNH ĐỂ KHÔNG BỊ CẮT ÂM)
        // =======================================================
        if (deathSounds != null && deathSounds.Length > 0)
        {
            AudioClip randomDeath = deathSounds[Random.Range(0, deathSounds.Length)];

            // Lệnh này tạo một loa tạm thời phát âm thanh xong tự hủy, cực kỳ an toàn!
            // Phát ở vị trí của Camera để đảm bảo luôn nghe to và rõ nhất.
            AudioSource.PlayClipAtPoint(randomDeath, Camera.main.transform.position);
        }
        // =======================================================

        if (healthBarFill != null && healthBarFill.canvas != null)
        {
            healthBarFill.canvas.gameObject.SetActive(false);
        }

        Destroy(gameObject);
    }
}
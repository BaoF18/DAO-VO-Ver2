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
    public AudioSource audioSource;
    public AudioClip[] hitSounds;
    public AudioClip[] deathSounds;

    [Header("--- ANIMATION ---")]
    private Animator animator;
    private Collider enemyCollider;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
        //Support for Death Animation
        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        enemyCollider = GetComponent<Collider>();
        if (enemyCollider == null) enemyCollider = GetComponentInChildren<Collider>();
    }

    public void TakeDamage(int damageAmount, Vector3 attackerPosition)
    {
        if (isDead) return;

        if (audioSource != null && hitSounds != null && hitSounds.Length > 0)
        {
            AudioClip randomHit = hitSounds[Random.Range(0, hitSounds.Length)];
            audioSource.PlayOneShot(randomHit);
        }

        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthUI();

        if (UIManager.Instance != null && UIManager.Instance.damagePopupPrefab != null)
        {
            Vector3 popupPos = transform.position + Vector3.up * 1.5f;
            GameObject popupObj = Instantiate(UIManager.Instance.damagePopupPrefab, popupPos, Quaternion.identity);
            DamagePopup popupScript = popupObj.GetComponent<DamagePopup>();
            if (popupScript != null) popupScript.Setup(damageAmount);
        }

        Debug.Log("💥 Enemy lost " + damageAmount + " Health. Remains " + currentHealth + " ❤");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthUI()
    {
        if (healthBarFill != null) healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        if (hpText != null) hpText.text = currentHealth + " / " + maxHealth;
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Knock out! Đang diễn hoạt cảnh chết...");

        // 1. Tắt thanh máu trên đầu
        if (healthBarFill != null && healthBarFill.canvas != null)
        {
            healthBarFill.canvas.gameObject.SetActive(false);
        }

        // 2. Tắt va chạm (Collider)
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        // =======================================================
        // [MỚI] 2.5: RÚT ỐNG THỞ (TẮT NÃO VÀ CHÂN CỦA QUÁI)
        // =======================================================
        // Tắt bộ não AI (Thay "EnemyAI" bằng tên script AI sếp đang dùng nếu khác)
        EnemyAI aiScript = GetComponent<EnemyAI>();
        if (aiScript != null) aiScript.enabled = false;

        // Cắt gân chân (NavMeshAgent) để nó không trượt trên mặt đất
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;
        // =======================================================

        // 3. Kích hoạt hoạt cảnh ngã gục
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // 4. Phát tiếng la hét
        if (deathSounds != null && deathSounds.Length > 0)
        {
            AudioClip randomDeath = deathSounds[Random.Range(0, deathSounds.Length)];
            AudioSource.PlayClipAtPoint(randomDeath, Camera.main.transform.position);
        }

        // 5. Gọi Coroutine chờ 3 giây rồi mới dọn xác
        StartCoroutine(DestroyAfterDelay(3f));
    }

    // Coroutine đếm ngược dọn xác
    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
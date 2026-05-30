using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem; // Bắt buộc phải có để xài InputSystem mới

public class PlayerHealth : MonoBehaviour
{
    // Cấp quyền cho các script khác gọi dễ dàng (Ví dụ: PlayerHealth.Instance.TakeDamage)
    public static PlayerHealth Instance;

    [Header("UI & Respawn")]
    public Transform respawnPoint;
    private Rigidbody rb;
    private bool isDead = false;

    [Header("--- HEALTH (Xanh Lá) ---")]
    public int maxHealth = 100;
    private int currentHealth;
    public Image healthBarFill;

    [Header("--- STAMINA (Đỏ) ---")]
    public float maxStamina = 100f;
    private float currentStamina;
    public Image staminaBarFill;
    public float staminaDrainRate = 20f; // Trừ khi chạy
    public float staminaRegenRate = 15f; // Hồi khi nghỉ
    private PlayerSprint playerSprint;

    [Header("--- MANA (Xanh Biển) ---")]
    public float maxMana = 100f;
    private float currentMana;
    public Image manaBarFill;
    public float manaCostPerAttack = 20f; // Trừ khi đánh
    public float manaRegenRate = 5f;      // Tự động hồi từ từ


    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerSprint = GetComponent<PlayerSprint>();
        // Đổ đầy 3 bình khi bắt đầu game
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        currentMana = maxMana;

        UpdateAllBars();
    }

    void Update()
    {
        if (isDead) return; // Chết rồi thì cấm xài thể lực/mana

        HandleStamina();
        HandleMana();
    }

    // ==========================================
    // LOGIC THỂ LỰC VÀ MANA
    // ==========================================
    private void HandleStamina()
    {
        // Nhìn thẳng vào script PlayerSprint để biết có ĐANG CHẠY THỰC SỰ hay không
        bool isRunning = playerSprint != null && playerSprint.IsSprintActive;

        if (isRunning && currentStamina > 0)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
        }
        else if (!isRunning && currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
        }

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        if (staminaBarFill != null) staminaBarFill.fillAmount = currentStamina / maxStamina;
    }

    private void HandleMana()
    {
        // Chỉ còn chức năng Hồi Mana từ từ khi không đánh
        if (currentMana < maxMana)
        {
            currentMana += manaRegenRate * Time.deltaTime;
        }

        currentMana = Mathf.Clamp(currentMana, 0, maxMana);
        if (manaBarFill != null) manaBarFill.fillAmount = currentMana / maxMana;
    }
    public void ConsumeMana()
    {
        currentMana -= manaCostPerAttack;
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);
        if (manaBarFill != null) manaBarFill.fillAmount = currentMana / maxMana;
    }
    // Tiện ích để các script Di chuyển / Combat hỏi xem có đủ sức không
    public bool CanRun() => currentStamina > 0;

    // Yêu cầu Mana hiện tại phải lớn hơn hoặc bằng 30% của Max Mana
    public bool CanAttack() => currentMana >= (maxMana * 0.3f);

    // ==========================================
    // LOGIC MÁU VÀ HỒI SINH (Giữ nguyên của bạn)
    // ==========================================
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        UpdateAllBars();

        if (UIManager.Instance != null && UIManager.Instance.damagePopupPrefab != null)
        {
            Vector3 popupPos = transform.position + Vector3.up * 1.5f;
            GameObject popupObj = Instantiate(UIManager.Instance.damagePopupPrefab, popupPos, Quaternion.identity);

            DamagePopup popupScript = popupObj.GetComponent<DamagePopup>();
            if (popupScript != null) popupScript.Setup(damage);
        }

        Debug.Log("Player bị đấm! Máu còn: " + currentHealth);

        if (currentHealth <= 0)
        {
            StartCoroutine(HandlePlayerDeath());
        }
    }

    private void UpdateAllBars()
    {
        if (healthBarFill != null) healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        if (staminaBarFill != null) staminaBarFill.fillAmount = currentStamina / maxStamina;
        if (manaBarFill != null) manaBarFill.fillAmount = currentMana / maxMana;
    }

    private IEnumerator HandlePlayerDeath()
    {
        isDead = true;
        Debug.Log("💀 WASTED!");

        if (rb != null) rb.linearVelocity = Vector3.zero;

        if (UIManager.Instance != null) UIManager.Instance.ShowDeathScreen();

        int waitTime = 3;
        while (waitTime > 0)
        {
            if (UIManager.Instance != null) UIManager.Instance.UpdateCountdown(waitTime);
            yield return new WaitForSeconds(1f);
            waitTime--;
        }

        DieAndRespawn();
    }

    private void DieAndRespawn()
    {
        // Phục hồi CẢ 3 BÌNH về trạng thái sung mãn nhất
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        currentMana = maxMana;

        isDead = false;
        UpdateAllBars();

        if (respawnPoint != null)
        {
            if (rb != null) rb.isKinematic = true;
            transform.position = respawnPoint.position;
            if (rb != null) rb.isKinematic = false;
        }

        if (UIManager.Instance != null && UIManager.Instance.deathScreenGroup != null)
        {
            UIManager.Instance.deathScreenGroup.SetActive(false);
        }
    }
}
using UnityEngine;
using TMPro;
using System.Collections;

public class MagicalPoop : Interactable
{
    [Header("--- POOP SETTINGS ---")]
    public TextMeshPro overheadLabel;
    public string interactText = "Hấp thụ tinh hoa :) (E)";

    [Tooltip("Khoảng cách đứng gần để hiện chữ (mét)")]
    public float showDistance = 3f; // <-- Thêm biến khoảng cách vào đây

    [Header("--- ANIMATION & AUDIO ---")]
    public AudioClip eatSound;
    public float eatAnimationDuration = 2f;

    private bool isEating = false;
    private Transform playerTransform; // Lưu vị trí player để đo khoảng cách

    void Start()
    {
        if (overheadLabel != null)
        {
            overheadLabel.text = interactText;
            overheadLabel.gameObject.SetActive(false); // Mới đẻ ra là giấu chữ đi luôn
        }

        // Tự động tìm Player ngay từ đầu để xài cho mượt
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    void LateUpdate()
    {
        if (overheadLabel != null && Camera.main != null && playerTransform != null)
        {
            // Nếu chưa ăn thì mới quét radar ẩn/hiện chữ
            if (!isEating)
            {
                float distance = Vector3.Distance(transform.position, playerTransform.position);
                // Đứng gần <= 3 mét thì bật chữ, đi xa thì tắt chữ
                overheadLabel.gameObject.SetActive(distance <= showDistance);
            }

            // CHỈ khi nào chữ ĐANG BẬT thì mới bắt nó xoay theo Camera (để tiết kiệm hiệu năng)
            if (overheadLabel.gameObject.activeSelf)
            {
                overheadLabel.transform.LookAt(Camera.main.transform);
                overheadLabel.transform.Rotate(0, 180, 0);
            }
        }
    }

    public override void OnInteract()
    {
        if (isEating) return;
        StartCoroutine(EatRoutine());
    }

    private IEnumerator EatRoutine()
    {
        isEating = true;

        if (overheadLabel != null)
        {
            // Ép bật chữ lên lỡ player có nhích lùi lại một chút
            overheadLabel.gameObject.SetActive(true);
            overheadLabel.text = "tích cực ăn c***, tích đức mai sau";
        }

        // Kích hoạt hoạt ảnh ăn
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Animator playerAnim = null;

        if (player != null)
        {
            playerAnim = player.GetComponentInChildren<Animator>();
            if (playerAnim != null)
            {
                playerAnim.SetTrigger("Eat");
                playerAnim.SetBool("IsEating", true); // Bật trạng thái ăn
            }
        }

        if (eatSound != null) AudioSource.PlayClipAtPoint(eatSound, transform.position);

        // =======================================================
        // GỌI TRẠM PHÁT SÓNG UI LÊN
        // =======================================================
        if (SharedUI.Instance != null && SharedUI.Instance.uiPanel != null)
        {
            SharedUI.Instance.uiPanel.SetActive(true);
        }

        float elapsedTime = 0f;
        while (elapsedTime < eatAnimationDuration)
        {
            elapsedTime += Time.deltaTime;

            if (SharedUI.Instance != null && SharedUI.Instance.fillImage != null)
            {
                SharedUI.Instance.fillImage.fillAmount = elapsedTime / eatAnimationDuration;
            }

            yield return null;
        }

        // =======================================================
        // DỌN DẸP SAU KHI ĂN XONG
        // =======================================================
        if (SharedUI.Instance != null && SharedUI.Instance.uiPanel != null)
        {
            SharedUI.Instance.uiPanel.SetActive(false); // Tắt thanh UI
        }

        // TẮT TRẠNG THÁI ĂN CHO PLAYER
        if (playerAnim != null)
        {
            playerAnim.SetBool("IsEating", false);
        }

        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.RestoreFullHealth();
            PlayerHealth.Instance.RestoreFullStamina();
            PlayerHealth.Instance.RestoreFullMana();
        }

        Destroy(gameObject); // Phi tang vật chứng
    }

    public override void OnInteractContinue() { }
    public override void OnInteractEnd() { }
}
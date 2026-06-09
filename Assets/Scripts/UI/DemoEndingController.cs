using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DemoEndingController : MonoBehaviour
{
    public static DemoEndingController Instance;

    [Header("1. Giao diện & Chữ")]
    public GameObject thankYouUI;
    public Button okButton;
    public GameObject madeByTeamText;

    [Header("2. Hệ thống Ngày/Đêm")]
    public LightningManager lightningManager;
    public float nightTime = 21f;
    public float dayTime = 7f;
    public float timeTransitionDuration = 3f;

    [Header("3. Pháo hoa")]
    public GameObject[] fireworksPrefabs;
    public Transform[] fireworksSpawnPoints;
    public float fireworksDuration = 8f;
    public float spawnInterval = 0.5f;

    // ==========================================
    // KHU VỰC MỚI: ÂM THANH
    [Header("4. Âm thanh (Sound)")]
    [Tooltip("Kéo cục EndingManager (hoặc object chứa AudioSource) vào đây")]
    public AudioSource audioSource;
    [Tooltip("Kéo file âm thanh Minecraft Achievement vào đây")]
    public AudioClip achievementSound;
    // ==========================================

    private bool hasTriggeredEnding = false;
    private bool isWaitingForOk = false;
    private float savedSpeedOfDay;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (thankYouUI != null) thankYouUI.SetActive(false);
        if (madeByTeamText != null) madeByTeamText.SetActive(false);

        if (okButton != null)
        {
            Debug.Log("Nút Ok button đâu?? Kéo vô chưa???!!");
        }
    }

    public void PlayEndingCinematic()
    {
        if (hasTriggeredEnding) return;
        hasTriggeredEnding = true;

        StartCoroutine(EndingSequenceRoutine());
    }

    private IEnumerator EndingSequenceRoutine()
    {
        // ----------------------------------------------------
        // BƯỚC 1: Hiện UI Cảm ơn & PHÁT ÂM THANH
        // ----------------------------------------------------
        if (thankYouUI != null) thankYouUI.SetActive(true);

        // Phát âm thanh Achievement ngay lúc UI bật lên
        if (audioSource != null && achievementSound != null)
        {
            audioSource.PlayOneShot(achievementSound);
        }
        //Button
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        isWaitingForOk = true;
        Debug.Log("ENDING: Waiting for player to press Ok or something in that button...");

        // LỆNH THẦN THÁNH: Chờ cho đến khi isWaitingForOk = false
        while (isWaitingForOk)
        {
            yield return null;
        }
        //For the third-view player
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (thankYouUI != null) thankYouUI.SetActive(false);
        if (lightningManager != null)
        {
            savedSpeedOfDay = lightningManager.SpeedOfDay;
            lightningManager.SpeedOfDay = 0f;
        }


        // ----------------------------------------------------
        // BƯỚC 2: Tua thời gian đến Đêm
        // ----------------------------------------------------
        yield return StartCoroutine(FastForwardTime(nightTime, timeTransitionDuration));

        // ----------------------------------------------------
        // BƯỚC 3: Hiện chữ Khổng lồ & Bắn pháo hoa
        // ----------------------------------------------------
        if (madeByTeamText != null) madeByTeamText.SetActive(true);

        float elapsed = 0f;
        while (elapsed < fireworksDuration)
        {
            if (fireworksPrefabs != null && fireworksPrefabs.Length > 0 && fireworksSpawnPoints.Length > 0)
            {
                GameObject randomPrefab = fireworksPrefabs[Random.Range(0, fireworksPrefabs.Length)];
                Transform randomPoint = fireworksSpawnPoints[Random.Range(0, fireworksSpawnPoints.Length)];

                if (randomPrefab != null && randomPoint != null)
                {
                    GameObject fw = Instantiate(randomPrefab, randomPoint.position, randomPoint.rotation);
                    Destroy(fw, 5f);
                }
            }

            float waitTime = Random.Range(spawnInterval * 0.5f, spawnInterval * 1.5f);
            yield return new WaitForSeconds(waitTime);
            elapsed += waitTime;
        }

        // ----------------------------------------------------
        // BƯỚC 4: Tắt chữ và Tua đến Sáng
        // ----------------------------------------------------
        if (madeByTeamText != null) madeByTeamText.SetActive(false);
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(FastForwardTime(dayTime, timeTransitionDuration));

        // ----------------------------------------------------
        // BƯỚC 5: Trả lại thời gian
        // ----------------------------------------------------
        if (lightningManager != null)
        {
            lightningManager.SpeedOfDay = savedSpeedOfDay;
        }
    }
    public void OnOkButtonClicked()
    {
        isWaitingForOk = false;
        Debug.Log("Player has press the button! Starting fireworks...");
    }

    private IEnumerator FastForwardTime(float targetTime, float duration)
    {
        if (lightningManager == null) yield break;

        float startVal = lightningManager.TimeOfDay;
        float endVal = targetTime;

        if (endVal < startVal) endVal += 24f;

        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            float currentValue = Mathf.Lerp(startVal, endVal, time / duration);
            lightningManager.TimeOfDay = currentValue % 24f;
            yield return null;
        }

        lightningManager.TimeOfDay = targetTime;
    }
}
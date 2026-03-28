using UnityEngine;
using TMPro; 

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Groups")]
    public GameObject skillIconsGroup;
    public GameObject deathScreenGroup;

    [Header("Death Screen Elements")]
    public TextMeshProUGUI countdownText;

    [Header("Combat UI")]
    public GameObject damagePopupPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    private void Start()
    {
        if (skillIconsGroup != null) skillIconsGroup.SetActive(false);
        if (deathScreenGroup != null) deathScreenGroup.SetActive(false);
    }

    public void ShowSkillIcons(bool show)
    {
        if (skillIconsGroup != null) skillIconsGroup.SetActive(show);
    }

    public void ShowDeathScreen()
    {
        if (deathScreenGroup != null) deathScreenGroup.SetActive(true);
    }

    public void UpdateCountdown(int seconds)
    {
        if (countdownText != null)
        {
            countdownText.text = "Toang rồi ông giáo ạ...Hồi sinh sau: " + seconds + "s";
        }
    }
}
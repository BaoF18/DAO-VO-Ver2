using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// IntroUI
/// - Hiển thị tiêu đề và nội dung giới thiệu với hiệu ứng typewriter.
/// - Kéo TextMeshProUGUI vào field trong Inspector.
/// - Tự động chuyển scene sau thời gian quy định.
/// </summary>
[DisallowMultipleComponent]
public class IntroUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("TextMeshProUGUI để hiển thị tiêu đề.")]
    [SerializeField] private TextMeshProUGUI titleText;

    [Tooltip("TextMeshProUGUI để hiển thị nội dung chính.")]
    [SerializeField] private TextMeshProUGUI contentText;

    [Header("Content")]
    [Tooltip("Nội dung tiêu đề.")]
    [TextArea(1, 3)]
    [SerializeField] private string titleContent = "";

    [Tooltip("Nội dung giới thiệu sẽ hiển thị.")]
    [TextArea(3, 10)]
    [SerializeField] private string contentBody = "";

    [Header("Typing")]
    [Tooltip("Độ trễ trước khi bắt đầu gõ tiêu đề (giây).")]
    [SerializeField] private float titleStartDelay = 0f;

    [Tooltip("Độ trễ sau khi tiêu đề hoàn thành trước khi bắt đầu gõ nội dung (giây).")]
    [SerializeField] private float contentStartDelay = 0.5f;

    // Tốc độ typing (private, content nhanh hơn title)
    private const float titleTypingSpeed = 0.05f;
    private const float contentTypingSpeed = 0.03f;

    [Header("Scene Flow")]
    [Tooltip("Thời gian chạy intro trước khi chuyển scene (giây).")]
    [SerializeField] private float sceneDuration = 30f;

    [Tooltip("Tên scene tiếp theo sẽ được load.")]
    [SerializeField] private string nextSceneName = "";

    // Typing state
    private float sceneTimer;
    private float typingTimer;
    private int titleVisibleCharacters;
    private int contentVisibleCharacters;
    private bool titleTypingCompleted;
    private bool contentTypingCompleted;
    private bool allTypingCompleted;

    private void Start()
    {
        if (titleText == null || contentText == null)
        {
            Debug.LogError("[IntroUI] titleText hoặc contentText chưa được gán trong Inspector!");
            enabled = false;
            return;
        }

        InitializeTexts();
    }

    private void Update()
    {
        UpdateTyping();
        UpdateSceneTimer();
    }

    /// <summary>
    /// Khởi tạo các text component.
    /// </summary>
    private void InitializeTexts()
    {
        // Thiết lập tiêu đề
        titleText.text = titleContent ?? string.Empty;
        titleText.maxVisibleCharacters = 0;
        titleText.ForceMeshUpdate();
        titleVisibleCharacters = 0;
        titleTypingCompleted = string.IsNullOrEmpty(titleText.text);

        // Thiết lập nội dung
        contentText.text = contentBody ?? string.Empty;
        contentText.maxVisibleCharacters = 0;
        contentText.ForceMeshUpdate();
        contentVisibleCharacters = 0;
        contentTypingCompleted = string.IsNullOrEmpty(contentText.text);

        typingTimer = -titleStartDelay;
        allTypingCompleted = false;
    }

    /// <summary>
    /// Hiệu ứng typewriter không tạo GC (dùng maxVisibleCharacters).
    /// Gõ tiêu đề trước, sau đó gõ nội dung.
    /// </summary>
    private void UpdateTyping()
    {
        if (allTypingCompleted)
        {
            return;
        }

        if (titleTypingSpeed <= 0f && contentTypingSpeed <= 0f)
        {
            // Tốc độ 0 = hiện ngay tất cả
            if (!titleTypingCompleted && titleText != null)
            {
                titleText.maxVisibleCharacters = titleText.text.Length;
                titleTypingCompleted = true;
            }
            if (!contentTypingCompleted && contentText != null)
            {
                contentText.maxVisibleCharacters = contentText.text.Length;
                contentTypingCompleted = true;
            }
            allTypingCompleted = true;
            return;
        }

        typingTimer += Time.deltaTime;

        // Gõ tiêu đề
        if (!titleTypingCompleted && titleText != null && typingTimer >= 0f)
        {
            UpdateTextTyping(titleText, ref titleVisibleCharacters, ref titleTypingCompleted, titleTypingSpeed);
        }

        // Gõ nội dung (sau khi tiêu đề xong + delay)
        if (!contentTypingCompleted && contentText != null && titleTypingCompleted)
        {
            if (typingTimer >= contentStartDelay)
            {
                UpdateTextTyping(contentText, ref contentVisibleCharacters, ref contentTypingCompleted, contentTypingSpeed);
            }
        }

        allTypingCompleted = titleTypingCompleted && contentTypingCompleted;
    }

    /// <summary>
    /// Cập nhật typing cho một text component cụ thể.
    /// </summary>
    private void UpdateTextTyping(TextMeshProUGUI textComponent, ref int visibleCharacters, ref bool isCompleted, float speed)
    {
        float interval = speed;

        while (typingTimer >= 0f && (isCompleted == false))
        {
            visibleCharacters++;

            if (visibleCharacters >= textComponent.text.Length)
            {
                visibleCharacters = textComponent.text.Length;
                isCompleted = true;
                break;
            }

            typingTimer -= interval;
        }

        textComponent.maxVisibleCharacters = visibleCharacters;
    }

    /// <summary>
    /// Đếm thời gian và chuyển scene khi hết thời lượng.
    /// </summary>
    private void UpdateSceneTimer()
    {
        if (sceneDuration <= 0f)
        {
            return;
        }

        sceneTimer += Time.deltaTime;
        if (sceneTimer >= sceneDuration)
        {
            sceneTimer = 0f;
            if (!string.IsNullOrWhiteSpace(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class GoScene : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string sceneNameToLoad;
    [SerializeField] private bool useSceneIndex = false;
    [SerializeField] private int sceneIndex = 0;

    [Header("Loading Panel")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Image loadingBar;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    [Header("Loading Speed")]
    [SerializeField] private float minLoadingDuration = 1f; // Tối thiểu thời gian hiển thị loading
    [SerializeField] private bool showLoadingPercentage = true;

    private CanvasGroup loadingCanvasGroup;
    private bool isLoading = false;

    private void Start()
    {
        // Get CanvasGroup if loadingPanel is assigned
        if (loadingPanel != null)
        {
            loadingCanvasGroup = loadingPanel.GetComponent<CanvasGroup>();
            if (loadingCanvasGroup == null)
            {
                loadingCanvasGroup = loadingPanel.AddComponent<CanvasGroup>();
            }

            // Ensure the loading panel does not block raycasts while hidden
            loadingCanvasGroup.alpha = 0f;
            loadingCanvasGroup.interactable = false;
            loadingCanvasGroup.blocksRaycasts = false;

            // If the panel was left active in inspector, hide it at start
            loadingPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Load scene by name
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (!isLoading)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }
    }

    /// <summary>
    /// Load scene by index
    /// </summary>
    public void LoadSceneByIndex(int sceneIdx)
    {
        if (!isLoading)
        {
            StartCoroutine(LoadSceneAsyncByIndex(sceneIdx));
        }
    }

    /// <summary>
    /// Load the scene configured in inspector
    /// </summary>
    public void LoadConfiguredScene()
    {
        if (!isLoading)
        {
            if (useSceneIndex)
            {
                StartCoroutine(LoadSceneAsyncByIndex(sceneIndex));
            }
            else
            {
                StartCoroutine(LoadSceneAsync(sceneNameToLoad));
            }
        }
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        isLoading = true;

        // Show loading panel
        yield return StartCoroutine(FadeInLoadingPanel());

        // Start async scene loading
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        float startTime = Time.time;

        // Wait for scene to load
        while (!asyncLoad.isDone)
        {
            // Progress is between 0 and 0.9
            float progress = asyncLoad.progress;

            // Update loading bar and text
            if (loadingBar != null)
            {
                loadingBar.fillAmount = progress;
            }

            if (loadingText != null && showLoadingPercentage)
            {
                loadingText.text = $"Loading... {Mathf.Round(progress * 100f)}%";
            }

            // Allow scene activation after minimum loading duration
            if (asyncLoad.progress >= 0.9f && Time.time - startTime >= minLoadingDuration)
            {
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        // Ensure loading bar is full
        if (loadingBar != null)
        {
            loadingBar.fillAmount = 1f;
        }

        if (loadingText != null)
        {
            loadingText.text = "Loading... 100%";
        }

        // Fade out loading panel
        yield return StartCoroutine(FadeOutLoadingPanel());

        isLoading = false;
    }

    private IEnumerator LoadSceneAsyncByIndex(int sceneIdx)
    {
        isLoading = true;

        // Show loading panel
        yield return StartCoroutine(FadeInLoadingPanel());

        // Start async scene loading
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIdx);
        asyncLoad.allowSceneActivation = false;

        float startTime = Time.time;

        // Wait for scene to load
        while (!asyncLoad.isDone)
        {
            // Progress is between 0 and 0.9
            float progress = asyncLoad.progress;

            // Update loading bar and text
            if (loadingBar != null)
            {
                loadingBar.fillAmount = progress;
            }

            if (loadingText != null && showLoadingPercentage)
            {
                loadingText.text = $"Loading... {Mathf.Round(progress * 100f)}%";
            }

            // Allow scene activation after minimum loading duration
            if (asyncLoad.progress >= 0.9f && Time.time - startTime >= minLoadingDuration)
            {
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        // Ensure loading bar is full
        if (loadingBar != null)
        {
            loadingBar.fillAmount = 1f;
        }

        if (loadingText != null)
        {
            loadingText.text = "Loading... 100%";
        }

        // Fade out loading panel
        yield return StartCoroutine(FadeOutLoadingPanel());

        isLoading = false;
    }

    private IEnumerator FadeInLoadingPanel()
    {
        if (loadingPanel == null)
            yield break;

        loadingPanel.SetActive(true);

        if (loadingCanvasGroup != null)
        {
            // Enable raycasts while visible
            loadingCanvasGroup.blocksRaycasts = true;
            loadingCanvasGroup.interactable = true;

            loadingCanvasGroup.alpha = 0f;
            float elapsedTime = 0f;

            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                loadingCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
                yield return null;
            }

            loadingCanvasGroup.alpha = 1f;
        }

        // Reset progress bar
        if (loadingBar != null)
        {
            loadingBar.fillAmount = 0f;
        }

        if (loadingText != null)
        {
            loadingText.text = "Loading...";
        }
    }

    private IEnumerator FadeOutLoadingPanel()
    {
        if (loadingPanel == null)
            yield break;

        // If there's no CanvasGroup, just deactivate
        if (loadingCanvasGroup == null)
        {
            loadingPanel.SetActive(false);
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            loadingCanvasGroup.alpha = Mathf.Clamp01(1f - (elapsedTime / fadeOutDuration));
            yield return null;
        }

        loadingCanvasGroup.alpha = 0f;
        // Disable raycasts when hidden so it doesn't block UI
        loadingCanvasGroup.blocksRaycasts = false;
        loadingCanvasGroup.interactable = false;
        loadingPanel.SetActive(false);
    }

    /// <summary>
    /// Check if currently loading a scene
    /// </summary>
    public bool IsLoading()
    {
        return isLoading;
    }

    /// <summary>
    /// Reload current scene
    /// </summary>
    public void ReloadCurrentScene()
    {
        if (!isLoading)
        {
            StartCoroutine(LoadSceneAsync(SceneManager.GetActiveScene().name));
        }
    }

    /// <summary>
    /// Set loading panel manually (useful for custom loading UI)
    /// </summary>
    public void SetLoadingPanel(GameObject panel)
    {
        loadingPanel = panel;
        if (loadingPanel != null)
        {
            loadingCanvasGroup = loadingPanel.GetComponent<CanvasGroup>();
            if (loadingCanvasGroup == null)
            {
                loadingCanvasGroup = loadingPanel.AddComponent<CanvasGroup>();
            }

            // Initialize same as in Start: hidden and non-blocking
            loadingCanvasGroup.alpha = 0f;
            loadingCanvasGroup.interactable = false;
            loadingCanvasGroup.blocksRaycasts = false;
            loadingPanel.SetActive(false);
        }
    }
}

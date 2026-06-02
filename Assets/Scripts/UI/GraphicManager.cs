using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GraphicManager : MonoBehaviour
{
    public static GraphicManager Instance { get; private set; }

    private const string PREFS_GRAPHICS_QUALITY = "GraphicsQuality";

    [Header("UI")]
    public TMP_Dropdown qualityDropdown; // assign TextMeshPro Dropdown in inspector

    [Header("Scene Objects")]
    public GameObject[] volumetricCloudObjects; // assign objects that implement volumetric clouds
    public GameObject[] postProcessingObjects;  // assign post-processing volume/objects

    // Quality labels in Vietnamese
    private readonly string[] qualityLabels = new string[] { "Thấp", "Trung bình", "Cao" };

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Ensure dropdown options are set if assigned
        if (qualityDropdown != null)
            SetupDropdownOptions();

        // Load saved quality
        int saved = PlayerPrefs.HasKey(PREFS_GRAPHICS_QUALITY) ? PlayerPrefs.GetInt(PREFS_GRAPHICS_QUALITY) : -1;
        if (saved >= 0 && saved < qualityLabels.Length)
        {
            ApplyQuality(saved, notifyDropdown: false);
        }
        else
        {
            // No saved value: default to 'Trung bình' (medium)
            ApplyQuality(1, notifyDropdown: false);
        }
    }

    private void Start()
    {
        // If dropdown is assigned later, make sure listeners are connected
        if (qualityDropdown != null)
        {
            // Set initial value without notifying to avoid triggering change twice
            #if UNITY_2019_1_OR_NEWER
            qualityDropdown.SetValueWithoutNotify(QualitySettings.GetQualityLevel());
            #else
            qualityDropdown.value = QualitySettings.GetQualityLevel();
            #endif

            qualityDropdown.onValueChanged.AddListener(OnDropdownChanged);
        }
    }

    private void OnEnable()
    {
        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.AddListener(OnDropdownChanged);
    }

    private void OnDisable()
    {
        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.RemoveListener(OnDropdownChanged);
    }

    private void SetupDropdownOptions()
    {
        qualityDropdown.ClearOptions();
        var options = new System.Collections.Generic.List<string>(qualityLabels);
        qualityDropdown.AddOptions(options);
    }

    // Called by Dropdown OnValueChanged
    public void OnDropdownChanged(int index)
    {
        ApplyQuality(index, notifyDropdown: true);
    }

    // Apply quality settings and save to PlayerPrefs
    public void ApplyQuality(int index, bool notifyDropdown)
    {
        int clamped = Mathf.Clamp(index, 0, qualityLabels.Length - 1);

        // Map our 0..2 to engine quality levels if project has more levels.
        int targetLevel = clamped;
        int maxLevel = QualitySettings.names.Length - 1;
        if (maxLevel < 2)
        {
            targetLevel = Mathf.Clamp(clamped, 0, maxLevel);
        }

        QualitySettings.SetQualityLevel(targetLevel, true);

        // Additionally tweak texture/resolution/AA and enable/disable clouds & postprocessing
        switch (clamped)
        {
            case 0: // Thấp - super low textures, NO clouds, NO post-processing
                QualitySettings.globalTextureMipmapLimit = 3; // very low texture resolution
                QualitySettings.globalTextureMipmapLimit = 3;
                QualitySettings.antiAliasing = 0; // no AA
                QualitySettings.shadowDistance = 10f;
                SetVolumetricCloudsActive(false);
                SetPostProcessingActive(false);
                break;
            case 1: // Trung bình - low textures, clouds and post-processing enabled
                QualitySettings.globalTextureMipmapLimit = 1; // low textures
                QualitySettings.globalTextureMipmapLimit = 1;
                QualitySettings.antiAliasing = 2;
                QualitySettings.shadowDistance = 30f;
                SetVolumetricCloudsActive(true);
                SetPostProcessingActive(true);
                break;
            case 2: // Cao - high textures, clouds and post-processing enabled
                QualitySettings.globalTextureMipmapLimit = 0; // full textures
                QualitySettings.globalTextureMipmapLimit = 0;
                QualitySettings.antiAliasing = 4;
                QualitySettings.shadowDistance = 60f;
                SetVolumetricCloudsActive(true);
                SetPostProcessingActive(true);
                break;
        }

        // Save choice
        PlayerPrefs.SetInt(PREFS_GRAPHICS_QUALITY, clamped);
        PlayerPrefs.Save();

        // Update dropdown without recursive event
        if (qualityDropdown != null && notifyDropdown)
        {
            #if UNITY_2019_1_OR_NEWER
            qualityDropdown.SetValueWithoutNotify(clamped);
            #else
            qualityDropdown.onValueChanged.RemoveListener(OnDropdownChanged);
            qualityDropdown.value = clamped;
            qualityDropdown.onValueChanged.AddListener(OnDropdownChanged);
            #endif
        }
    }

    // Public helper to bind dropdown at runtime
    public void BindDropdown(TMP_Dropdown dropdown)
    {
        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.RemoveListener(OnDropdownChanged);

        qualityDropdown = dropdown;
        if (qualityDropdown != null)
        {
            SetupDropdownOptions();
            #if UNITY_2019_1_OR_NEWER
            qualityDropdown.SetValueWithoutNotify(PlayerPrefs.GetInt(PREFS_GRAPHICS_QUALITY, 1));
            #else
            qualityDropdown.value = PlayerPrefs.GetInt(PREFS_GRAPHICS_QUALITY, 1);
            #endif
            qualityDropdown.onValueChanged.AddListener(OnDropdownChanged);
        }
    }

    /// <summary>
    /// Scenes can call this to register their local volumetric cloud and post-processing objects.
    /// GraphicManager will store references and apply the current quality immediately.
    /// </summary>
    public void SetSceneObjects(GameObject[] clouds, GameObject[] posts)
    {
        volumetricCloudObjects = clouds;
        postProcessingObjects = posts;

        // Apply current saved quality so new scene objects are set correctly
        int saved = PlayerPrefs.GetInt(PREFS_GRAPHICS_QUALITY, 1);
        ApplyQuality(saved, notifyDropdown: false);
    }

    private void SetVolumetricCloudsActive(bool active)
    {
        if (volumetricCloudObjects == null) return;
        for (int i = 0; i < volumetricCloudObjects.Length; i++)
        {
            if (volumetricCloudObjects[i] != null)
                volumetricCloudObjects[i].SetActive(active);
        }
    }

    private void SetPostProcessingActive(bool active)
    {
        if (postProcessingObjects == null) return;
        for (int i = 0; i < postProcessingObjects.Length; i++)
        {
            if (postProcessingObjects[i] != null)
                postProcessingObjects[i].SetActive(active);
        }
    }
}

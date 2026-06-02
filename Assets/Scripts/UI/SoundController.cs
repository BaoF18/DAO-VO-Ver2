using UnityEngine;
using System;
using UnityEngine.UI;

public class SoundController : MonoBehaviour
{
    public static SoundController Instance { get; private set; }

    private const string PREFS_MASTER_VOLUME = "MasterVolume";

    [Range(0f, 1f)]
    [SerializeField]
    private float masterVolume = 1f;

    [Header("UI")]
    public Slider masterSlider; // Public so you can drag the UI Slider in inspector

    // Event invoked when volume changes (value between 0 and 1)
    public event Action<float> OnMasterVolumeChanged;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load saved volume (if available)
        masterVolume = PlayerPrefs.HasKey(PREFS_MASTER_VOLUME) ? PlayerPrefs.GetFloat(PREFS_MASTER_VOLUME) : masterVolume;
        ApplyVolume();
    }

    private void Start()
    {
        // Initialize slider if assigned
        if (masterSlider != null)
        {
            // Set initial slider value without invoking OnValueChanged
            #if UNITY_2019_1_OR_NEWER
            masterSlider.SetValueWithoutNotify(masterVolume);
            #else
            masterSlider.value = masterVolume;
            #endif

            // Add listener
            masterSlider.onValueChanged.AddListener(OnMasterVolumeSliderChanged);
        }
    }

    private void OnEnable()
    {
        // In case slider is assigned at runtime after Awake
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.AddListener(OnMasterVolumeSliderChanged);
        }
    }

    private void OnDisable()
    {
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.RemoveListener(OnMasterVolumeSliderChanged);
        }
    }

    /// <summary>
    /// Set master volume (0..1). Persists to PlayerPrefs and applies immediately.
    /// Can be called from UI Slider OnValueChanged.
    /// </summary>
    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        ApplyVolume();
        SaveVolume();
        OnMasterVolumeChanged?.Invoke(masterVolume);

        // Update slider display without invoking its listeners to avoid loops
        if (masterSlider != null)
        {
            #if UNITY_2019_1_OR_NEWER
            masterSlider.SetValueWithoutNotify(masterVolume);
            #else
            // temporarily remove listener
            masterSlider.onValueChanged.RemoveListener(OnMasterVolumeSliderChanged);
            masterSlider.value = masterVolume;
            masterSlider.onValueChanged.AddListener(OnMasterVolumeSliderChanged);
            #endif
        }
    }

    /// <summary>
    /// Handler for UI Slider (connect Slider OnValueChanged to this method).
    /// </summary>
    public void OnMasterVolumeSliderChanged(float value)
    {
        SetMasterVolume(value);
    }

    /// <summary>
    /// Get current master volume (0..1)
    /// </summary>
    public float GetMasterVolume()
    {
        return masterVolume;
    }

    /// <summary>
    /// Toggle mute on/off. When muting, remember previous volume so toggle restores it.
    /// </summary>
    private float previousVolume = 1f;
    public void ToggleMute()
    {
        if (masterVolume > 0f)
        {
            previousVolume = masterVolume;
            SetMasterVolume(0f);
        }
        else
        {
            SetMasterVolume(previousVolume);
        }
    }

    private void ApplyVolume()
    {
        // Apply global volume for all audio via AudioListener
        AudioListener.volume = masterVolume;
    }

    private void SaveVolume()
    {
        PlayerPrefs.SetFloat(PREFS_MASTER_VOLUME, masterVolume);
        PlayerPrefs.Save();
    }

    private void OnApplicationQuit()
    {
        // Ensure saved on exit
        SaveVolume();
    }

    /// <summary>
    /// Helper to assign a slider at runtime and initialize it.
    /// </summary>
    public void BindSlider(Slider slider)
    {
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.RemoveListener(OnMasterVolumeSliderChanged);
        }

        masterSlider = slider;

        if (masterSlider != null)
        {
            #if UNITY_2019_1_OR_NEWER
            masterSlider.SetValueWithoutNotify(masterVolume);
            #else
            masterSlider.value = masterVolume;
            #endif
            masterSlider.onValueChanged.AddListener(OnMasterVolumeSliderChanged);
        }
    }
}

using UnityEngine;

public class DynamicObject : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource targetAudioSource;
    [SerializeField] private bool autoDetectAudioSource = true;
    [SerializeField] private int frequencyBand = 0; // 0-7 (8 bands)
    
    [Header("Scale Settings")]
    [SerializeField] private Vector3 baseScale = Vector3.one;
    [SerializeField] private float scaleMultiplier = 2f;
    [SerializeField] private bool scaleX = false;
    [SerializeField] private bool scaleY = true;
    [SerializeField] private bool scaleZ = false;
    
    [Header("Smoothing")]
    [SerializeField] private float smoothSpeed = 8f;
    [SerializeField] private float sensitivity = 2f;
    [SerializeField] private float falloffSpeed = 5f; // T?c ?? gi?m v? scale g?c
    
    [Header("FFT Settings")]
    [SerializeField] private int fftWindow = 256; // 256, 512, 1024, 2048, etc.
    
    private float[] spectrumData;
    private Vector3 targetScale;
    private float currentScaleFactor = 1f;
    private float lastFrequencyValue = 0f;

    void Start()
    {
        // Auto-detect AudioSource if not assigned
        if (autoDetectAudioSource || targetAudioSource == null)
        {
            // First try to get AudioSource from the same GameObject
            targetAudioSource = GetComponent<AudioSource>();
            
            // If not found, try to find LoopMusic in scene
            if (targetAudioSource == null)
            {
                LoopMusic loopMusic = FindObjectOfType<LoopMusic>();
                if (loopMusic != null)
                {
                    targetAudioSource = loopMusic.GetComponent<AudioSource>();
                }
            }

            // Last resort: find any AudioSource in scene
            if (targetAudioSource == null)
            {
                targetAudioSource = FindObjectOfType<AudioSource>();
            }
        }

        // Initialize base scale
        baseScale = transform.localScale;
        targetScale = baseScale;
        spectrumData = new float[fftWindow];
        
        if (targetAudioSource == null)
        {
            Debug.LogWarning("DynamicObject: No AudioSource found! Please assign one in the Inspector or ensure LoopMusic exists in the scene.");
        }
    }

    void Update()
    {
        if (targetAudioSource == null || !targetAudioSource.isPlaying)
        {
            // Reset to base scale when no audio is playing
            currentScaleFactor = Mathf.Lerp(currentScaleFactor, 1f, Time.deltaTime * falloffSpeed);
            ApplyScale();
            return;
        }

        try
        {
            // Get frequency spectrum from the audio source
            targetAudioSource.GetSpectrumData(spectrumData, 0, FFTWindow.Rectangular);

            // Get the specific frequency band and apply sensitivity
            float frequencyValue = spectrumData[frequencyBand];
            
            // Amplify the frequency value
            frequencyValue = Mathf.Pow(frequencyValue, 0.5f) * sensitivity;
            frequencyValue = Mathf.Clamp01(frequencyValue);

            // Smooth transition to new scale
            currentScaleFactor = Mathf.Lerp(currentScaleFactor, 1f + (frequencyValue * scaleMultiplier), Time.deltaTime * smoothSpeed);

            lastFrequencyValue = frequencyValue;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("DynamicObject: Error reading spectrum data - " + ex.Message);
        }

        ApplyScale();
    }

    private void ApplyScale()
    {
        targetScale = baseScale;

        if (scaleX)
            targetScale.x = baseScale.x * currentScaleFactor;
        if (scaleY)
            targetScale.y = baseScale.y * currentScaleFactor;
        if (scaleZ)
            targetScale.z = baseScale.z * currentScaleFactor;

        transform.localScale = targetScale;
    }

    /// <summary>
    /// Set which frequency band to listen to (0-7)
    /// 0 = Bass, 7 = Treble
    /// </summary>
    public void SetFrequencyBand(int band)
    {
        frequencyBand = Mathf.Clamp(band, 0, 7);
    }

    /// <summary>
    /// Set which axes should scale
    /// </summary>
    public void SetScaleAxes(bool x, bool y, bool z)
    {
        scaleX = x;
        scaleY = y;
        scaleZ = z;
    }

    /// <summary>
    /// Set the audio source to use
    /// </summary>
    public void SetAudioSource(AudioSource audioSource)
    {
        targetAudioSource = audioSource;
    }

    /// <summary>
    /// Set scale multiplier (how much it scales)
    /// </summary>
    public void SetScaleMultiplier(float multiplier)
    {
        scaleMultiplier = Mathf.Max(0f, multiplier);
    }

    /// <summary>
    /// Set sensitivity of frequency detection
    /// </summary>
    public void SetSensitivity(float newSensitivity)
    {
        sensitivity = Mathf.Max(0.1f, newSensitivity);
    }

    /// <summary>
    /// Set smoothing speed
    /// </summary>
    public void SetSmoothSpeed(float speed)
    {
        smoothSpeed = Mathf.Max(1f, speed);
    }

    /// <summary>
    /// Set falloff speed (how fast it returns to base scale)
    /// </summary>
    public void SetFalloffSpeed(float speed)
    {
        falloffSpeed = Mathf.Max(1f, speed);
    }

    /// <summary>
    /// Get current frequency value (for debugging)
    /// </summary>
    public float GetCurrentFrequencyValue()
    {
        return lastFrequencyValue;
    }
}

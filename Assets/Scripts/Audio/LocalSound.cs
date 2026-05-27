using UnityEngine;

/// <summary>
/// LocalSound: Attach to any GameObject to play a sound that fades out with distance.
/// Example: Simulate a motorbike sound that gets quieter as the player/camera moves away.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class LocalSound : MonoBehaviour
{
    [Header("Sound Settings")]
    [Tooltip("Audio clip to play (looped)")]
    public AudioClip soundClip;
    [Range(0f, 1f)]
    [Tooltip("Maximum volume when listener is close")] 
    public float maxVolume = 1f;

    [Header("Distance Settings")]
    [Tooltip("Maximum distance to hear the sound")] 
    public float maxDistance = 30f;
    [Tooltip("Minimum distance for full volume")] 
    public float minDistance = 2f;

    [Header("Listener (optional)")]
    [Tooltip("Transform to measure distance from (default: Camera.main)")]
    public Transform listener;

    private AudioSource audioSource;

    void Start()
    {
        // Get or add AudioSource
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = soundClip;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f; // 2D, manual volume control
        audioSource.playOnAwake = false;
        audioSource.volume = maxVolume;

        if (soundClip != null)
            audioSource.Play();

        // Default to main camera if no listener assigned
        if (listener == null && Camera.main != null)
            listener = Camera.main.transform;
    }

    void Update()
    {
        if (listener == null) return;

        float distance = Vector3.Distance(transform.position, listener.position);

        // Volume logic: full volume if close, fade out with distance, silent if too far
        if (distance <= minDistance)
        {
            audioSource.volume = maxVolume;
        }
        else if (distance >= maxDistance)
        {
            audioSource.volume = 0f;
        }
        else
        {
            float t = 1f - ((distance - minDistance) / (maxDistance - minDistance));
            audioSource.volume = maxVolume * t;
        }
    }
}

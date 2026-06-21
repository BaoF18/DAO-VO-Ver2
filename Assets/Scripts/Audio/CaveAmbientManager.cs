using UnityEngine;
using System.Collections;

public class CaveAmbientManager : MonoBehaviour
{
    [Header("--- CAVE SOUNDS (MINECRAFT STYLE) ---")]
    [Tooltip("Kéo cục chứa AudioSource vào đây")]
    public AudioSource audioSource;
    [Tooltip("Audio sources")]
    public AudioClip[] ambientSounds;

    [Header("--- TIMING ---")]
    [Tooltip("Default = 120s")]
    public float delayTime = 120f;

    [Tooltip("Bật cái này lên để thời gian phát chênh lệch cho tự nhiên (khó đoán)")]
    public bool useRandomDelay = true;
    public float minDelay = 90f;  // 1 phút rưỡi
    public float maxDelay = 180f; // 3 phút

    void Start()
    {
        // Tự động lấy loa nếu sếp lười kéo thả
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        // Bắt đầu vòng lặp hù dọa
        StartCoroutine(AmbientLoop());
    }

    private IEnumerator AmbientLoop()
    {
        while (true) // Lặp vô tận cho đến khi tắt game
        {
            // Tính toán thời gian nín thở
            float waitTime = useRandomDelay ? Random.Range(minDelay, maxDelay) : delayTime;

            // Đợi...
            yield return new WaitForSeconds(waitTime);

            // BÙM! Phát âm thanh
            if (ambientSounds != null && ambientSounds.Length > 0 && audioSource != null)
            {
                AudioClip creepySound = ambientSounds[Random.Range(0, ambientSounds.Length)];


                audioSource.PlayOneShot(creepySound);
                Debug.Log($"java.lang.RuntimeException: entity.null.????? at {Time.time}s");
            }
        }
    }
}
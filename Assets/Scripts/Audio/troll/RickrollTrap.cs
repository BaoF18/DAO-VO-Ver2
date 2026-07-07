using UnityEngine;
using UnityEngine.Video;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class RickrollTrap : MonoBehaviour
{
    [Header("--- BAIT SETTINGS ---")]
    [Tooltip("Kéo cục mồi nhử (Hộp quà FBX) vào đây để nó biến mất khi dính bẫy")]
    public GameObject baitObject;

    [Header("--- RICKROLL SETTINGS ---")]
    [Tooltip("Kéo file video Rickroll (.mp4) vào đây")]
    public VideoClip rickrollVideo;
    [Tooltip("Thời gian chiếu (giây)")]
    public float playDuration = 5f;

    [Header("--- TARGET SIGN ---")]
    [Tooltip("Kéo cục Mesh chứa cái biển Sign3 vào đây")]
    public MeshRenderer signRenderer;
    public int materialIndex = 0;

    private string textureProperty = "_BaseColorMap";

    private VideoPlayer videoPlayer;
    private bool hasTriggered = false;
    private Texture originalTexture;

    void Start()
    {
        // 1. Tự động mọc ra cái máy phát Video
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.clip = rickrollVideo;

        // 2. Tự động mọc ra cái Loa 3D (Đứng gần mới nghe, đi xa nhỏ dần)
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        AudioSource audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.spatialBlend = 1f;
        audioSrc.minDistance = 2f;
        audioSrc.maxDistance = 15f;
        videoPlayer.SetTargetAudioSource(0, audioSrc);

        videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
        videoPlayer.targetMaterialRenderer = signRenderer;
        videoPlayer.targetMaterialProperty = textureProperty;

        if (signRenderer != null)
        {
            originalTexture = signRenderer.materials[materialIndex].GetTexture(textureProperty);
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Nếu chộp được Player và chưa từng kích hoạt
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(ExecuteRickroll());
        }
    }

    private IEnumerator ExecuteRickroll()
    {
        Debug.Log("🎶 Never gonna give you up...");

        if (baitObject != null)
        {
            baitObject.SetActive(false); // Ẩn hộp quà đi
        }

        videoPlayer.Play();

        yield return new WaitForSeconds(playDuration);

        videoPlayer.Stop();
        Debug.Log("Return to normal advertisement.");

        // Dán lại tấm ảnh gốc "Dịch vụ chuyển phát"
        if (signRenderer != null && originalTexture != null)
        {
            signRenderer.materials[materialIndex].SetTexture(textureProperty, originalTexture);
        }
    }
}

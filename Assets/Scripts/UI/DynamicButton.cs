using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class DynamicButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Audio")]
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioSource audioSource;

    [Header("Scale")]
    [SerializeField] private Vector3 hoverScale = new Vector3(1.08f, 1.08f, 1f);
    [SerializeField] private float scaleDuration = 0.08f;

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;

    void Awake()
    {
        originalScale = transform.localScale;
        if (audioSource == null)
        {
            // Add a temporary AudioSource if none assigned
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    // IPointerEnterHandler
    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayHover();
    }

    // IPointerExitHandler
    public void OnPointerExit(PointerEventData eventData)
    {
        StopHover();
    }

    // IPointerClickHandler
    public void OnPointerClick(PointerEventData eventData)
    {
        PlayClick();
    }

    // Public methods you can assign in the Inspector (Button OnClick / EventTrigger)
    public void PlayHover()
    {
        if (hoverClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverClip);
        }
        StartScale(hoverScale);
    }

    public void StopHover()
    {
        StartScale(originalScale);
    }

    public void PlayClick()
    {
        if (clickClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickClip);
        }
    }

    private void StartScale(Vector3 target)
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleRoutine(target));
    }

    private IEnumerator ScaleRoutine(Vector3 target)
    {
        Vector3 start = transform.localScale;
        float elapsed = 0f;
        // Use unscaled delta so UI feel instant even when timeScale changes
        while (elapsed < scaleDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(start, target, Mathf.Clamp01(elapsed / scaleDuration));
            yield return null;
        }
        transform.localScale = target;
        scaleCoroutine = null;
    }

    // Optional helpers to change clips at runtime
    public void SetHoverClip(AudioClip clip) => hoverClip = clip;
    public void SetClickClip(AudioClip clip) => clickClip = clip;
}

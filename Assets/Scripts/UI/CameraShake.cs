using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Wave Settings")]
    [Tooltip("Biên độ xoay (roll)")]
    public float rotationAmplitude = 2f;
    [Tooltip("Tốc độ xoay")]
    public float rotationFrequency = 0.5f;

    private Quaternion initialRotation;

    void Start()
    {
        initialRotation = transform.localRotation;
    }

    void Update()
    {
        float roll = Mathf.Sin(Time.time * rotationFrequency) * rotationAmplitude;
        transform.localRotation = initialRotation * Quaternion.Euler(0, 0, roll);
    }
}
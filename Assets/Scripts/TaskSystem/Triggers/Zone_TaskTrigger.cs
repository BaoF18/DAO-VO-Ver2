using UnityEngine;

public class Zone_TaskTrigger : MonoBehaviour
{
    [SerializeField] private string zoneId;
    [SerializeField] private string requiredTag = "Player";
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (other == null)
        {
            Debug.LogWarning("[Zone_TaskTrigger] Collider is NULL.");
            return;
        }

        if (triggerOnce && hasTriggered)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(requiredTag) && !other.CompareTag(requiredTag))
        {
            Debug.Log($"[Zone_TaskTrigger] Ignore collider {other.name} because tag is not {requiredTag}.");
            return;
        }

        if (string.IsNullOrWhiteSpace(zoneId))
        {
            Debug.LogWarning("[Zone_TaskTrigger] zoneId is empty. Event ignored.");
            return;
        }

        hasTriggered = true;

        Debug.Log($"[Zone_TaskTrigger] Enter zone => {zoneId}");
        TaskEvents.RaiseEnterZoneRequested(zoneId);
    }
}

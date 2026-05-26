using UnityEngine;

public class NPC_TaskTrigger : MonoBehaviour
{
    [SerializeField] private string npcId;
    private bool waitingForDialogueEnd;

    private void OnDisable()
    {
        if (waitingForDialogueEnd)
        {
            DialogueManager.DialogueEnded -= HandleDialogueEnded;
            waitingForDialogueEnd = false;
        }
    }

    public void Interact()
    {
        if (string.IsNullOrWhiteSpace(npcId))
        {
            Debug.LogWarning("[NPC_TaskTrigger] npcId is empty. Event ignored.");
            return;
        }

        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            return;
        }

        if (!waitingForDialogueEnd)
        {
            waitingForDialogueEnd = true;
            DialogueManager.DialogueEnded += HandleDialogueEnded;
        }
    }

    private void HandleDialogueEnded()
    {
        if (!waitingForDialogueEnd)
        {
            return;
        }

        waitingForDialogueEnd = false;
        DialogueManager.DialogueEnded -= HandleDialogueEnded;

        Debug.Log($"[NPC_TaskTrigger] Dialogue complete => {npcId}");
        TaskEvents.RaiseTalkToNpcRequested(npcId);
    }
}

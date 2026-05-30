using UnityEngine;

public class NPC_TaskTrigger : MonoBehaviour
{
    [SerializeField] private string npcId;
    private bool waitingForDialogueEnd;
    private float interactBlockUntilTime;
    private bool isBlockingInteraction;

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

        // ĐÃ XÓA KHÚC CHẶN HÀNH VI Ở ĐÂY

        if (isBlockingInteraction && Time.time < interactBlockUntilTime)
        {
            return;
        }

        isBlockingInteraction = false;

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

        // ĐÃ XÓA DÒNG SET TRUE Ở ĐÂY

        Debug.Log($"[NPC_TaskTrigger] Dialogue complete => {npcId}");

        // Bắn tín hiệu về cho Sếp TaskManager
        TaskEvents.RaiseTalkToNpcRequested(npcId);

        StartInteractionBlockForTaskDisplay();
    }

    private void StartInteractionBlockForTaskDisplay()
    {
        TaskManager taskManager = TaskManager.Instance;
        if (taskManager == null || taskManager.CurrentTask == null)
        {
            return;
        }

        TaskUI taskUI = FindObjectOfType<TaskUI>();
        if (taskUI == null)
        {
            return;
        }

        interactBlockUntilTime = Time.time + taskUI.DisplayDuration;
        isBlockingInteraction = true;
    }
}
using UnityEngine;

public class NPC_TaskTrigger : MonoBehaviour
{
    [SerializeField] private string npcId;
    private Coroutine waitForDialogueCoroutine;

    private void OnDisable()
    {
        StopWaitCoroutine();
    }

    public void Interact()
    {
        if (string.IsNullOrWhiteSpace(npcId))
        {
            Debug.LogWarning("[NPC_TaskTrigger] npcId is empty. Event ignored.");
            return;
        }

        StopWaitCoroutine();
        waitForDialogueCoroutine = StartCoroutine(WaitForDialogueThenRaiseRequest());
    }

    private System.Collections.IEnumerator WaitForDialogueThenRaiseRequest()
    {
        yield return null;

        while (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            yield return null;
        }

        waitForDialogueCoroutine = null;
        RaiseNpcTalkRequest();
    }

    private void StopWaitCoroutine()
    {
        if (waitForDialogueCoroutine == null)
        {
            return;
        }

        StopCoroutine(waitForDialogueCoroutine);
        waitForDialogueCoroutine = null;
    }

    private void RaiseNpcTalkRequest()
    {
        Debug.Log($"[NPC_TaskTrigger] Interact => {npcId}");
        TaskEvents.RaiseTalkToNpcRequested(npcId);
    }
}

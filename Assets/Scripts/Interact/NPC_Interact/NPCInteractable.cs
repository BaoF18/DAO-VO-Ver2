using UnityEngine;

/// <summary>
/// Cầu nối giữa Interact system và Dialogue/NPC system.
/// Implement IInteractable (qua Interactable base) để PlayerInteract gọi.
/// Dùng DialogueManager.Events thay vì poll trong Update().
/// </summary>
public class NPCInteractable : Interactable
{
    public DialogueData dialogueData;

    private NPCMovement npcMovement;
    private bool waitingForDialogueEnd;

    public override bool IsInteracting =>
        DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive && waitingForDialogueEnd;

    void Awake()
    {
        npcMovement = GetComponent<NPCMovement>();

        // NPC tĩnh (không có NPCMovement, ví dụ ngồi):
        // - Rigidbody kinematic để physics không xung đột với Animator
        // - Tắt Root Motion để animation không dịch chuyển Transform
        if (npcMovement == null)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }

            Animator anim = GetComponentInChildren<Animator>();
            if (anim != null)
            {
                anim.applyRootMotion = false;
            }
        }
    }

    void OnEnable()
    {
        DialogueManager.DialogueEnded += HandleDialogueEnded;
    }

    void OnDisable()
    {
        DialogueManager.DialogueEnded -= HandleDialogueEnded;
    }

    void HandleDialogueEnded()
    {
        if (!waitingForDialogueEnd) return;
        waitingForDialogueEnd = false;

        if (npcMovement != null)
        {
            npcMovement.Resume();
        }
    }

    public override void OnInteract()
    {
        if (dialogueData == null || DialogueManager.Instance == null) return;

        if (!DialogueManager.Instance.IsDialogueActive)
        {
            if (npcMovement != null)
            {
                npcMovement.Pause();
            }

            DialogueManager.Instance.StartDialogue(dialogueData);
            waitingForDialogueEnd = true;
        }
    }

    public override void OnInteractContinue()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.ContinueDialogue();
    }

    public override void OnInteractEnd()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.ForceEndDialogue();
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Cầu nối giữa Interact system và Dialogue/NPC system.
/// Implement IInteractable (qua Interactable base) để PlayerInteract gọi.
/// Dùng DialogueManager.Events thay vì poll trong Update().
/// </summary>
public class NPCInteractable : Interactable
{
    public DialogueData dialogueData;
    [SerializeField] private List<DialogueByTask> dialogueOverrides = new List<DialogueByTask>();
    [SerializeField] private NPC_TaskTrigger npcTaskTrigger;

    [Header("Events")]
    [SerializeField] private UnityEvent onInteractStarted;

    private NPCMovement npcMovement;
    private bool waitingForDialogueEnd;

    [Serializable]
    private class DialogueByTask
    {
        [SerializeField] private string taskId;
        [SerializeField] private string targetId;
        [SerializeField] private DialogueData dialogue;

        public string TaskId => taskId;
        public string TargetId => targetId;
        public DialogueData Dialogue => dialogue;
    }

    public override bool IsInteracting =>
        DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive && waitingForDialogueEnd;

    void Awake()
    {
        npcMovement = GetComponent<NPCMovement>();

        if (npcTaskTrigger == null)
        {
            npcTaskTrigger = GetComponent<NPC_TaskTrigger>();
        }

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
        onInteractStarted?.Invoke();

        DialogueData activeDialogue = GetDialogueForCurrentTask();
        if (activeDialogue == null || DialogueManager.Instance == null) return;

        if (!DialogueManager.Instance.IsDialogueActive)
        {
            if (npcMovement != null)
            {
                npcMovement.Pause();
            }

            npcTaskTrigger?.Interact();

            DialogueManager.Instance.StartDialogue(activeDialogue);
            waitingForDialogueEnd = true;
        }
    }

    private DialogueData GetDialogueForCurrentTask()
    {
        TaskData currentTask = TaskManager.Instance != null ? TaskManager.Instance.CurrentTask : null;
        if (currentTask != null && dialogueOverrides.Count > 0)
        {
            for (int i = 0; i < dialogueOverrides.Count; i++)
            {
                DialogueByTask entry = dialogueOverrides[i];
                if (entry == null || entry.Dialogue == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(entry.TaskId)
                    && string.Equals(entry.TaskId, currentTask.Id, StringComparison.Ordinal))
                {
                    return entry.Dialogue;
                }
            }

            for (int i = 0; i < dialogueOverrides.Count; i++)
            {
                DialogueByTask entry = dialogueOverrides[i];
                if (entry == null || entry.Dialogue == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.TaskId)
                    && !string.IsNullOrWhiteSpace(entry.TargetId)
                    && string.Equals(entry.TargetId, currentTask.TargetId, StringComparison.Ordinal))
                {
                    return entry.Dialogue;
                }
            }
        }

        return dialogueData;
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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class TaskManager : MonoBehaviour
{
    [Header("Database")]
    [SerializeField] private TaskDatabase taskDatabase;

    [Header("Task Chain")]
    [SerializeField] private List<TaskData> tasks = new List<TaskData>();

    [Header("Flow")]
    [SerializeField] [Min(0f)] private float nextTaskDelayAfterComplete = 1.5f;
    [SerializeField] [Min(0f)] private float panelRevealDelayAfterFirstActivity = 5f;

    public static TaskManager Instance { get; private set; }
    public TaskData CurrentTask { get; private set; }

    private int currentTaskIndex = -1;
    private Coroutine activateNextTaskCoroutine;
    private Coroutine revealPanelCoroutine;
    private bool revealTriggered;

#if ENABLE_INPUT_SYSTEM
    private InputAction anyKeyAction;
#endif

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[TaskManager] Duplicate TaskManager detected. Destroying duplicate instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

#if ENABLE_INPUT_SYSTEM
        anyKeyAction = new InputAction("TaskPanelRevealAnyKey", InputActionType.Button, "<Keyboard>/anyKey");
#endif
    }

    private void OnEnable()
    {
        TaskEvents.OnTalkToNpcRequested += HandleTalkToNpc;
        TaskEvents.OnEnterZoneRequested += HandleEnterZone;

#if ENABLE_INPUT_SYSTEM
        if (anyKeyAction != null)
        {
            anyKeyAction.performed += HandleAnyKeyPerformed;
            anyKeyAction.Enable();
        }
#endif
    }

    private void OnDisable()
    {
        TaskEvents.OnTalkToNpcRequested -= HandleTalkToNpc;
        TaskEvents.OnEnterZoneRequested -= HandleEnterZone;

#if ENABLE_INPUT_SYSTEM
        if (anyKeyAction != null)
        {
            anyKeyAction.performed -= HandleAnyKeyPerformed;
            anyKeyAction.Disable();
        }
#endif

        if (activateNextTaskCoroutine != null)
        {
            StopCoroutine(activateNextTaskCoroutine);
            activateNextTaskCoroutine = null;
        }

        if (revealPanelCoroutine != null)
        {
            StopCoroutine(revealPanelCoroutine);
            revealPanelCoroutine = null;
        }
    }

    private void Start()
    {
        LoadTasksFromDatabase();
        InitializeChain();
    }

    private void LoadTasksFromDatabase()
    {
        if (taskDatabase == null || taskDatabase.Tasks == null || taskDatabase.Tasks.Count == 0)
        {
            return;
        }

        tasks.Clear();
        for (int i = 0; i < taskDatabase.Tasks.Count; i++)
        {
            TaskData task = taskDatabase.Tasks[i];
            if (task != null)
            {
                // =======================================================
                // TỰ ĐỘNG RESET TRẠNG THÁI TASK KHI BẮT ĐẦU GAME
                if (i == 0)
                {
                    task.State = TaskState.Active; // Task đầu tiên mở
                }
                else
                {
                    task.State = TaskState.Locked; // Các task sau khóa lại
                }
                // =======================================================

                tasks.Add(task);
            }
        }
    }

    [ContextMenu("Load Example Task Chain")]
    private void LoadExampleTaskChain()
    {
        tasks = new List<TaskData>
        {
            new TaskData("task_01", "Talk to NPC A", TaskType.TalkToNPC, "NPC_A"),
            new TaskData("task_02", "Go to Zone B", TaskType.GoToLocation, "ZONE_B")
        };

        Debug.Log("[TaskManager] Example task chain loaded.");
    }

    private void InitializeChain()
    {
        if (tasks == null || tasks.Count == 0)
        {
            Debug.LogWarning("[TaskManager] No tasks configured.");
            TaskEvents.RaiseTaskUpdated(null);
            return;
        }

        int firstIncomplete = -1;

        for (int i = 0; i < tasks.Count; i++)
        {
            TaskData task = tasks[i];

            if (task == null)
            {
                Debug.LogWarning($"[TaskManager] Task at index {i} is NULL.");
                continue;
            }

            if (task.State == TaskState.Completed)
            {
                continue;
            }

            if (firstIncomplete < 0)
            {
                firstIncomplete = i;
                task.State = TaskState.Active;
            }
            else
            {
                task.State = TaskState.Locked;
            }
        }

        if (firstIncomplete < 0)
        {
            currentTaskIndex = -1;
            CurrentTask = null;
            Debug.Log("[TaskManager] All tasks are already completed.");
            TaskEvents.RaiseTaskUpdated(null);
            return;
        }

        currentTaskIndex = firstIncomplete;
        CurrentTask = tasks[currentTaskIndex];

        Debug.Log($"[TaskManager] Start task => {CurrentTask.Id}");
        TaskEvents.RaiseTaskStart(CurrentTask);
        TaskEvents.RaiseTaskActivated(CurrentTask);
        TaskEvents.RaiseTaskUpdated(CurrentTask);
    }

    private void HandleTalkToNpc(string npcId)
    {
#if !ENABLE_INPUT_SYSTEM
        RegisterFirstPlayerActivity("npc-interaction");
#endif

        if (CurrentTask == null)
        {
            Debug.Log($"[TaskManager] Ignore NPC interaction ({npcId}) because there is no active task.");
            return;
        }

        if (CurrentTask.Type != TaskType.TalkToNPC)
        {
            Debug.Log($"[TaskManager] Ignore NPC interaction ({npcId}) - expected task type {CurrentTask.Type} for task {CurrentTask.Id}.");
            return;
        }

        if (!string.Equals(CurrentTask.TargetId, npcId, StringComparison.Ordinal))
        {
            Debug.Log($"[TaskManager] Ignore NPC interaction ({npcId}) - expected target {CurrentTask.TargetId} for task {CurrentTask.Id}.");
            return;
        }

        TryCompleteByAction(TaskType.TalkToNPC, npcId);
    }

    private void HandleEnterZone(string zoneId)
    {
        RegisterFirstPlayerActivity("zone-interaction");

        TryCompleteByAction(TaskType.GoToLocation, zoneId);
    }

#if ENABLE_INPUT_SYSTEM
    private void HandleAnyKeyPerformed(InputAction.CallbackContext context)
    {
        RegisterFirstPlayerActivity("input");
    }
#endif

    private void RegisterFirstPlayerActivity(string source)
    {
        if (revealTriggered)
        {
            return;
        }

        revealTriggered = true;

        if (revealPanelCoroutine != null)
        {
            StopCoroutine(revealPanelCoroutine);
        }

        revealPanelCoroutine = StartCoroutine(RevealTaskPanelWithDelay(source));
    }

    private IEnumerator RevealTaskPanelWithDelay(string source)
    {
        if (panelRevealDelayAfterFirstActivity > 0f)
        {
            yield return new WaitForSeconds(panelRevealDelayAfterFirstActivity);
        }

        revealPanelCoroutine = null;
        Debug.Log($"[TaskManager] Reveal task panel from first player activity: {source}");
        TaskEvents.RaiseTaskPanelRevealRequested();
    }

    private void TryCompleteByAction(TaskType actionType, string targetId)
    {
        if (CurrentTask == null)
        {
            Debug.Log($"[TaskManager] Ignore action {actionType} ({targetId}) because there is no active task.");
            return;
        }

        if (CurrentTask.IsCompleted)
        {
            Debug.Log($"[TaskManager] Ignore action {actionType} ({targetId}) because current task already completed.");
            return;
        }

        if (CurrentTask.State != TaskState.Active)
        {
            Debug.Log($"[TaskManager] Ignore action {actionType} ({targetId}) because current task is not active.");
            return;
        }

        if (CurrentTask.Type != actionType)
        {
            Debug.Log($"[TaskManager] Ignore action {actionType} ({targetId}) - expected {CurrentTask.Type} for task {CurrentTask.Id}.");
            return;
        }

        if (!string.Equals(CurrentTask.TargetId, targetId, StringComparison.Ordinal))
        {
            Debug.Log($"[TaskManager] Ignore action {actionType} ({targetId}) - expected target {CurrentTask.TargetId} for task {CurrentTask.Id}.");
            return;
        }

        CompleteTask(CurrentTask);
    }

    public void CompleteTask(TaskData task)
    {
        if (task == null)
        {
            Debug.LogWarning("[TaskManager] CompleteTask called with NULL task.");
            return;
        }

        if (CurrentTask == null)
        {
            Debug.Log($"[TaskManager] Ignore complete task {task.Id} because there is no active task.");
            return;
        }

        if (!string.Equals(task.Id, CurrentTask.Id, StringComparison.Ordinal))
        {
            Debug.Log($"[TaskManager] Ignore complete task {task.Id} because current task is {CurrentTask.Id}.");
            return;
        }

        if (task.IsCompleted)
        {
            Debug.Log($"[TaskManager] Ignore complete task {task.Id} because task already completed.");
            return;
        }

        task.State = TaskState.Completed;
        CurrentTask = task;

        Debug.Log($"[TaskManager] Completed task => {task.Id}");
        TaskEvents.RaiseTaskCompleted(task);
        TaskEvents.RaiseTaskUpdated(task);

        if (activateNextTaskCoroutine != null)
        {
            StopCoroutine(activateNextTaskCoroutine);
        }

        activateNextTaskCoroutine = StartCoroutine(ActivateNextTaskWithDelay());
    }

    private IEnumerator ActivateNextTaskWithDelay()
    {
        if (nextTaskDelayAfterComplete > 0f)
        {
            yield return new WaitForSeconds(nextTaskDelayAfterComplete);
        }

        activateNextTaskCoroutine = null;
        ActivateNextTask();
    }

    private void ActivateNextTask()
    {
        if (tasks == null || tasks.Count == 0)
        {
            CurrentTask = null;
            currentTaskIndex = -1;
            TaskEvents.RaiseTaskUpdated(null);
            return;
        }

        int nextIndex = currentTaskIndex + 1;

        while (nextIndex < tasks.Count && tasks[nextIndex] == null)
        {
            Debug.LogWarning($"[TaskManager] Skip NULL task at index {nextIndex}.");
            nextIndex++;
        }

        if (nextIndex >= tasks.Count)
        {
            CurrentTask = null;
            currentTaskIndex = -1;
            Debug.Log("[TaskManager] Quest chain completed.");
            TaskEvents.RaiseTaskUpdated(null);
            return;
        }

        currentTaskIndex = nextIndex;
        CurrentTask = tasks[currentTaskIndex];

        if (CurrentTask.State != TaskState.Completed)
        {
            CurrentTask.State = TaskState.Active;
        }

        Debug.Log($"[TaskManager] Start task => {CurrentTask.Id}");
        TaskEvents.RaiseTaskStart(CurrentTask);
        TaskEvents.RaiseTaskActivated(CurrentTask);
        TaskEvents.RaiseTaskUpdated(CurrentTask);
    }
}

using System.Collections;
using TMPro;
using UnityEngine;

public class TaskUI : MonoBehaviour
{
    [SerializeField] private TMP_Text taskDescriptionText;
    [SerializeField] private TMP_Text taskStateText;
    [SerializeField] private TaskPanelClickToggle panelToggle;

    [Header("Display Durations")]
    [SerializeField] [Min(0f)] private float displayDuration = 3f;
    [SerializeField] [Min(0f)] private float completedDisplayDuration = 1.5f;

    [Header("Fallback")]
    [SerializeField] private string unavailableDescription = "Task system not ready";
    [SerializeField] private string unavailableState = "-";

    [Header("State Labels")]
    [SerializeField] private string completedStateLabel = "Completed";

    private Coroutine displayRoutine;
    private int displayRoutineToken;
    private string lastRenderedTaskId;
    private string lastRenderedDescription;
    private TaskState? lastRenderedState;
    private bool lastRenderedWasNull;
    private bool hasRendered;

    private void Awake()
    {
        if (panelToggle == null)
        {
            panelToggle = GetComponentInParent<TaskPanelClickToggle>(true);
        }

        TaskEvents.OnTaskActivated += HandleTaskActivated;
        TaskEvents.OnTaskCompleted += HandleTaskCompleted;
        TaskEvents.OnTaskUpdated += HandleTaskUpdated;
        TaskEvents.OnTaskPanelRevealRequested += HandleTaskPanelRevealRequested;
    }

    private void OnEnable()
    {
        SyncFromManager(startDisplayFlow: false, source: "OnEnable");
    }

    private void OnDestroy()
    {
        TaskEvents.OnTaskActivated -= HandleTaskActivated;
        TaskEvents.OnTaskCompleted -= HandleTaskCompleted;
        TaskEvents.OnTaskUpdated -= HandleTaskUpdated;
        TaskEvents.OnTaskPanelRevealRequested -= HandleTaskPanelRevealRequested;

        StopDisplayRoutine("OnDestroy");
    }

    private void HandleTaskActivated(TaskData task)
    {
        Debug.Log($"[TaskUI] HandleTaskActivated => {task?.Id ?? "NULL"}");
        HandleDisplayEvent(task, displayDuration, "OnTaskActivated", suppressIfUnchanged: true);
    }

    private void HandleTaskCompleted(TaskData task)
    {
        Debug.Log($"[TaskUI] HandleTaskCompleted => {task?.Id ?? "NULL"}");
        HandleDisplayEvent(task, completedDisplayDuration, "OnTaskCompleted", suppressIfUnchanged: true);
    }

    private void HandleTaskUpdated(TaskData task)
    {
        Debug.Log($"[TaskUI] HandleTaskUpdated => {task?.Id ?? "NONE"}");

        float duration = task != null && task.State == TaskState.Completed
            ? completedDisplayDuration
            : displayDuration;

        HandleDisplayEvent(task, duration, "OnTaskUpdated", suppressIfUnchanged: true);
    }

    private void HandleTaskPanelRevealRequested()
    {
        SyncFromManager(startDisplayFlow: true, source: "OnTaskPanelRevealRequested");
    }

    public void SyncFromManager()
    {
        SyncFromManager(startDisplayFlow: false, source: "SyncFromManager");
    }

    private void SyncFromManager(bool startDisplayFlow, string source)
    {
        if (TaskManager.Instance == null)
        {
            Debug.LogWarning("[TaskUI] TaskManager.Instance is NULL. Render fallback text.");
            RenderUnavailable();
            return;
        }

        TaskData currentTask = TaskManager.Instance.CurrentTask;

        if (startDisplayFlow)
        {
            float duration = currentTask != null && currentTask.State == TaskState.Completed
                ? completedDisplayDuration
                : displayDuration;

            StartDisplayFlow(currentTask, duration, source);
            return;
        }

        Render(currentTask);
        UpdateRenderCache(currentTask);
    }

    private void HandleDisplayEvent(TaskData task, float duration, string source, bool suppressIfUnchanged)
    {
        if (suppressIfUnchanged && !HasTaskChanged(task))
        {
            Debug.Log($"[TaskUI] Ignore duplicate display event ({source}) for task {task?.Id ?? "NONE"}.");
            return;
        }

        StartDisplayFlow(task, duration, source);
    }

    private void StartDisplayFlow(TaskData task, float duration, string source)
    {
        // Bắt buộc stop routine cũ trước khi chạy routine mới.
        // Nếu không, nhiều routine cùng chạy sẽ làm panel bị tắt sớm hoặc chồng trạng thái UI.
        StopDisplayRoutine($"Restart from {source}");

        ShowPanel();
        Render(task);
        UpdateRenderCache(task);

        Debug.Log($"[TaskUI] Start display from {source} => task: {task?.Id ?? "NONE"}, state: {(task != null ? task.State.ToString() : "NONE")}, duration: {duration:0.00}s");

        if (duration <= 0f)
        {
            Debug.Log("[TaskUI] Duration <= 0, skip auto hide.");
            return;
        }

        displayRoutineToken++;
        int token = displayRoutineToken;
        displayRoutine = StartCoroutine(HidePanelAfterDelay(duration, token));
        Debug.Log($"[TaskUI] Start display coroutine token={token}");
    }

    private IEnumerator HidePanelAfterDelay(float delay, int token)
    {
        // Flow hiển thị:
        // - Task mới: render description, đợi ~3s rồi ẩn panel.
        // - Task completed: render "Completed", đợi 1-2s rồi ẩn panel.
        // Khi event mới tới, routine này sẽ bị stop và reset timer từ đầu.
        yield return new WaitForSeconds(delay);

        if (token != displayRoutineToken)
        {
            Debug.Log($"[TaskUI] Skip hide for stale coroutine token={token}, current={displayRoutineToken}");
            yield break;
        }

        displayRoutine = null;
        Debug.Log($"[TaskUI] Hide panel from display coroutine token={token}");
        HidePanel();
    }

    private void StopDisplayRoutine(string reason)
    {
        if (displayRoutine == null)
        {
            return;
        }

        Debug.Log($"[TaskUI] Stop display coroutine token={displayRoutineToken}. Reason: {reason}");
        StopCoroutine(displayRoutine);
        displayRoutine = null;
    }

    private void ShowPanel()
    {
        if (panelToggle != null)
        {
            panelToggle.ShowPanel();
        }
    }

    private void HidePanel()
    {
        if (panelToggle != null)
        {
            panelToggle.HidePanel();
            return;
        }

        Debug.LogWarning("[TaskUI] panelToggle is NULL. Cannot hide panel.");
    }

    private void RenderUnavailable()
    {
        if (taskDescriptionText != null)
        {
            taskDescriptionText.text = unavailableDescription;
        }

        if (taskStateText != null)
        {
            taskStateText.text = unavailableState;
        }
    }

    private void Render(TaskData task)
    {
        if (taskDescriptionText == null && taskStateText == null)
        {
            return;
        }

        if (taskDescriptionText != null && taskStateText != null && taskDescriptionText == taskStateText)
        {
            RenderCombined(task);
            return;
        }

        if (task == null)
        {
            if (taskDescriptionText != null)
            {
                taskDescriptionText.text = "All tasks completed";
            }

            if (taskStateText != null)
            {
                taskStateText.text = "✔";
            }

            return;
        }

        if (task.State == TaskState.Completed)
        {
            if (taskDescriptionText != null)
            {
                taskDescriptionText.text = string.Empty;
            }

            if (taskStateText != null)
            {
                taskStateText.text = completedStateLabel;
            }

            return;
        }

        if (taskDescriptionText != null)
        {
            taskDescriptionText.text = task.Description;
        }

        if (taskStateText != null)
        {
            taskStateText.text = string.Empty;
        }
    }

    private void RenderCombined(TaskData task)
    {
        if (taskDescriptionText == null)
        {
            return;
        }

        if (task == null)
        {
            taskDescriptionText.text = "All tasks completed\n✔";
            return;
        }

        if (task.State == TaskState.Completed)
        {
            taskDescriptionText.text = completedStateLabel;
            return;
        }

        taskDescriptionText.text = task.Description;
    }

    private bool HasTaskChanged(TaskData task)
    {
        if (!hasRendered)
        {
            return true;
        }

        if (task == null)
        {
            return !lastRenderedWasNull;
        }

        if (lastRenderedWasNull)
        {
            return true;
        }

        return lastRenderedTaskId != task.Id
            || lastRenderedState != task.State
            || lastRenderedDescription != task.Description;
    }

    private void UpdateRenderCache(TaskData task)
    {
        hasRendered = true;

        if (task == null)
        {
            lastRenderedWasNull = true;
            lastRenderedTaskId = null;
            lastRenderedDescription = null;
            lastRenderedState = null;
            return;
        }

        lastRenderedWasNull = false;
        lastRenderedTaskId = task.Id;
        lastRenderedDescription = task.Description;
        lastRenderedState = task.State;
    }
}

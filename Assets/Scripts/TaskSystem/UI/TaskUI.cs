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
    private string lastRenderedTaskId;
    private string lastRenderedDescription;
    private TaskState? lastRenderedState;
    private bool lastRenderedWasNull;
    private bool hasRendered;

    private void Awake()
    {
        TaskEvents.OnTaskActivated += HandleTaskActivated;
        TaskEvents.OnTaskCompleted += HandleTaskCompleted;
        TaskEvents.OnTaskUpdated += HandleTaskUpdated;
        TaskEvents.OnTaskPanelRevealRequested += HandleTaskPanelRevealRequested;
    }

    private void OnEnable()
    {
        SyncFromManager(startDisplayFlow: false);
    }

    private void OnDestroy()
    {
        TaskEvents.OnTaskActivated -= HandleTaskActivated;
        TaskEvents.OnTaskCompleted -= HandleTaskCompleted;
        TaskEvents.OnTaskUpdated -= HandleTaskUpdated;
        TaskEvents.OnTaskPanelRevealRequested -= HandleTaskPanelRevealRequested;

        StopDisplayRoutine();
    }

    private void HandleTaskActivated(TaskData task)
    {
        Debug.Log($"[TaskUI] HandleTaskActivated => {task?.Id ?? "NULL"}");
        StartDisplayFlow(task, displayDuration);
    }

    private void HandleTaskCompleted(TaskData task)
    {
        Debug.Log($"[TaskUI] HandleTaskCompleted => {task?.Id ?? "NULL"}");
        StartDisplayFlow(task, completedDisplayDuration);
    }

    private void HandleTaskUpdated(TaskData task)
    {
        Debug.Log($"[TaskUI] HandleTaskUpdated => {task?.Id ?? "NONE"}");

        if (!HasTaskChanged(task))
        {
            return;
        }

        float duration = task != null && task.State == TaskState.Completed
            ? completedDisplayDuration
            : displayDuration;

        StartDisplayFlow(task, duration);
    }

    private void HandleTaskPanelRevealRequested()
    {
        SyncFromManager(startDisplayFlow: true);
    }

    public void SyncFromManager()
    {
        SyncFromManager(startDisplayFlow: false);
    }

    private void SyncFromManager(bool startDisplayFlow)
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

            StartDisplayFlow(currentTask, duration);
            return;
        }

        Render(currentTask);
        UpdateRenderCache(currentTask);
    }

    private void StartDisplayFlow(TaskData task, float duration)
    {
        // Bắt buộc stop routine cũ trước khi chạy routine mới.
        // Nếu không, nhiều routine cùng chạy sẽ làm panel bị tắt sớm hoặc chồng trạng thái UI.
        StopDisplayRoutine();

        ShowPanel();
        Render(task);
        UpdateRenderCache(task);

        if (duration <= 0f)
        {
            return;
        }

        displayRoutine = StartCoroutine(HidePanelAfterDelay(duration));
    }

    private IEnumerator HidePanelAfterDelay(float delay)
    {
        // Flow hiển thị:
        // - Task mới: render description, đợi ~3s rồi ẩn panel.
        // - Task completed: render "Completed", đợi 1-2s rồi ẩn panel.
        // Khi event mới tới, routine này sẽ bị stop và reset timer từ đầu.
        yield return new WaitForSeconds(delay);
        displayRoutine = null;
        HidePanel();
    }

    private void StopDisplayRoutine()
    {
        if (displayRoutine == null)
        {
            return;
        }

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
        }
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

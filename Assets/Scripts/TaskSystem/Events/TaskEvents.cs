using System;
using UnityEngine;

public static class TaskEvents
{
    public static event Action<TaskData> OnTaskStart;
    public static event Action<TaskData> OnTaskActivated;
    public static event Action<TaskData> OnTaskCompleted;
    public static event Action<TaskData> OnTaskUpdated;
    public static event Action OnTaskPanelRevealRequested;

    public static event Action<string> OnTalkToNpcRequested;
    public static event Action<string> OnEnterZoneRequested;

    public static void RaiseTaskStart(TaskData task)
    {
        Debug.Log($"[TaskEvents] OnTaskStart => {task?.Id ?? "NULL"}");
        OnTaskStart?.Invoke(task);
    }

    public static void RaiseTaskActivated(TaskData task)
    {
        Debug.Log($"[TaskEvents] OnTaskActivated => {task?.Id ?? "NULL"}");
        OnTaskActivated?.Invoke(task);
    }

    public static void RaiseTaskCompleted(TaskData task)
    {
        Debug.Log($"[TaskEvents] OnTaskCompleted => {task?.Id ?? "NULL"}");
        OnTaskCompleted?.Invoke(task);
    }

    public static void RaiseTaskUpdated(TaskData currentTask)
    {
        Debug.Log($"[TaskEvents] OnTaskUpdated => Current: {currentTask?.Id ?? "NONE"}");
        OnTaskUpdated?.Invoke(currentTask);
    }

    public static void RaiseTaskPanelRevealRequested()
    {
        Debug.Log("[TaskEvents] OnTaskPanelRevealRequested");
        OnTaskPanelRevealRequested?.Invoke();
    }

    public static void RaiseTalkToNpcRequested(string npcId)
    {
        Debug.Log($"[TaskEvents] OnTalkToNpcRequested => {npcId}");
        OnTalkToNpcRequested?.Invoke(npcId);
    }

    public static void RaiseEnterZoneRequested(string zoneId)
    {
        Debug.Log($"[TaskEvents] OnEnterZoneRequested => {zoneId}");
        OnEnterZoneRequested?.Invoke(zoneId);
    }
}

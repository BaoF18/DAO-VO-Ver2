using System;
using UnityEngine;

[Serializable]
public enum TaskType
{
    TalkToNPC = 0,
    GoToLocation = 1,
    Collect = 2,
    Kill = 3
}

[Serializable]
public enum TaskState
{
    Locked = 0,
    Displaying = 1,
    Active = 2,
    Completed = 3
}

[Serializable]
public class TaskData
{
    [SerializeField] private string id;
    [SerializeField] private string description;
    [SerializeField] private TaskType type;
    [SerializeField] private string targetId;
    [SerializeField] private TaskState state = TaskState.Locked;

    public string Id => id;
    public string Description => description;
    public TaskType Type => type;
    public string TargetId => targetId;

    public TaskState State
    {
        get => state;
        set => state = value;
    }

    public bool IsCompleted => state == TaskState.Completed;

    public TaskData(string id, string description, TaskType type, string targetId)
    {
        this.id = id;
        this.description = description;
        this.type = type;
        this.targetId = targetId;
        state = TaskState.Locked;
    }
}

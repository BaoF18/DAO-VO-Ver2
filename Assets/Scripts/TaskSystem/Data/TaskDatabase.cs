using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Task System/Task Database", fileName = "TaskDatabase")]
public class TaskDatabase : ScriptableObject
{
    [SerializeField] private List<TaskData> tasks = new List<TaskData>();

    public IReadOnlyList<TaskData> Tasks => tasks;
}

using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TaskManager : MonoBehaviour
{
    public static TaskManager instance;

    [Header("UI")]
    public TextMeshProUGUI taskListText;

    [Header("Görev Listesi")]
    public List<TaskData> tasks = new List<TaskData>();

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        UpdateTaskUI();
    }

    public void CheckBaseCapture(BaseController baseController)
    {
        foreach (var task in tasks)
        {
            if (task.isCompleted) continue;

            if (task.targetBase == baseController &&
                baseController.owner == Team.Player)
            {
                task.isCompleted = true;
                UpdateTaskUI();
            }
        }
    }

    void UpdateTaskUI()
    {
        taskListText.text = "";

        foreach (var task in tasks)
        {
            string status = task.isCompleted ? "<color=green>✓</color>" : "<color=red>•</color>";
            taskListText.text += $"{status} {task.taskName}\n";
        }
    }
}

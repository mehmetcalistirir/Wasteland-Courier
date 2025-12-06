using UnityEngine;

[System.Serializable]
public class TaskData
{
    public string taskName;
    public BaseController targetBase; 
    public bool isCompleted = false;
}

using System.Collections.Generic;
using UnityEngine;

public class Taskas : MonoBehaviour
{
    [Header("UI References")]
    public Transform taskContainer;
    public GameObject taskPrefab;
    TaskManager taskManager;
    
    [Header("Task Data")]
    public List<Task> tasks = new List<Task>();
    
    private List<GameObject> taskUIElements = new List<GameObject>();

    void Start()
    {
        taskManager.tasks = tasks;
        //InitializeTasks();
        CreateTaskUI();
    }
    //
    void InitializeTasks()
    {
        taskManager.tasks.Add(new Task(1, "Battle Royal Season 2", "Check out the rewards you can earn by climbing up the tiers! The rewards are not additive and you will receive..."));
        taskManager.tasks.Add(new Task(2, "Mad Killer Event", "The Mad Killer Event is back! Kill zombies in survival and earn exclusive rewards including rare skins..."));
        taskManager.tasks.Add(new Task(3, "Halloween Event", "Spooky season is here! Complete Halloween challenges to unlock limited-time costumes and decorations..."));
        taskManager.tasks.Add(new Task(4, "Daily Login Bonus", "Login daily to receive coins, gems and exclusive items. Don't miss your streak!"));
    }
    
    void CreateTaskUI()
    {
        foreach (Transform child in taskContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (Task task in tasks)
        {
            Debug.Log($"Creating UI for task: {task.taskName}");
            GameObject taskUI = Instantiate(taskPrefab, taskContainer);
            TaskUI taskUIScript = taskUI.GetComponent<TaskUI>();
            Debug.Log($"TaskUI component found: {(taskUIScript != null ? "Yes" : "No")}");
            taskUIScript.SetupTask(task, taskManager);
            taskUIElements.Add(taskUI);
        }
    }
    
    public void ToggleTaskCompletion(int taskId)
    {
        Task task = tasks.Find(t => t.id == taskId);
        if (task != null)
        {
            task.isCompleted = !task.isCompleted;
            UpdateTaskUI(taskId);
        }
    }
    
    void UpdateTaskUI(int taskId)
    {
        foreach (GameObject taskUI in taskUIElements)
        {
            TaskUI taskUIScript = taskUI.GetComponent<TaskUI>();
            if (taskUIScript.GetTaskId() == taskId)
            {
                taskUIScript.UpdateTaskDisplay();
                break;
            }
        }
    }
    
    public void CompleteTask(int taskId)
    {
        Task task = tasks.Find(t => t.id == taskId);
        if (task != null && !task.isCompleted)
        {
            task.isCompleted = true;
            UpdateTaskUI(taskId);
            Debug.Log($"Task completed: {task.taskName}");
        }
    }
}
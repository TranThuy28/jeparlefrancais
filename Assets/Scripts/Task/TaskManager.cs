using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TaskManager : MonoBehaviour
{
    // Singleton pattern
    public static TaskManager Instance { get; private set; }
    
    [Header("UI References")]
    public Transform taskContainer;
    public GameObject taskPrefab;
    
    [Header("Notification Settings")]
    public Canvas notificationCanvas;
    
    [Header("Task Data")]
    public List<Task> tasks = new List<Task>();
    
    private List<GameObject> taskUIElements = new List<GameObject>();
    private NotificationManager notificationManager;
    private bool isInitialized = false;
    
    void Awake()
    {
        // Singleton setup - sẽ chạy ngay cả khi inactive
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ TaskManager qua scenes
            InitializeTaskManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeTaskManager()
    {
        if (isInitialized) return;
        
        Debug.Log("TaskManager initializing...");
        SetupNotificationManager();
        isInitialized = true;
        Debug.Log("TaskManager initialized successfully!");
    }
    
    void Start()
    {
        // Chỉ tạo UI khi GameObject active
        if (gameObject.activeInHierarchy)
        {
            CreateTaskUI();
        }
    }
    
    void OnEnable()
    {
        // Tạo UI khi GameObject được active
        if (isInitialized)
        {
            CreateTaskUI();
        }
    }
    
    void SetupNotificationManager()
    {
        if (notificationCanvas == null)
        {
            GameObject canvasObj = new GameObject("NotificationCanvas");
            notificationCanvas = canvasObj.AddComponent<Canvas>();
            notificationCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            notificationCanvas.sortingOrder = 999;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        GameObject notificationObj = new GameObject("NotificationManager");
        notificationObj.transform.SetParent(notificationCanvas.transform);
        notificationManager = notificationObj.AddComponent<NotificationManager>();
        notificationManager.Initialize(notificationCanvas);
        
        Debug.Log("NotificationManager created and initialized.");
    }
    
    void CreateTaskUI()
    {
        if (taskContainer == null)
        {
            Debug.LogWarning("Task Container is null - UI not created");
            return;
        }
        
        // Clear existing UI
        foreach (Transform child in taskContainer)
        {
            Destroy(child.gameObject);
        }
        taskUIElements.Clear();
        
        // Create new UI
        foreach (Task task in tasks)
        {
            if (taskPrefab == null) continue;
            
            GameObject taskUI = Instantiate(taskPrefab, taskContainer);
            TaskUI taskUIScript = taskUI.GetComponent<TaskUI>();
            if (taskUIScript != null)
            {
                taskUIScript.SetupTask(task, this);
                taskUIElements.Add(taskUI);
            }
        }
        
        Debug.Log($"Created UI for {taskUIElements.Count} tasks");
    }
    
    // Public method để force initialize từ bên ngoài
    public static void EnsureInitialized()
    {
        if (Instance == null)
        {
            // Tìm TaskManager trong scene (kể cả inactive)
            TaskManager[] allManagers = Resources.FindObjectsOfTypeAll<TaskManager>();
            foreach (TaskManager manager in allManagers)
            {
                if (manager.gameObject.scene.isLoaded) // Chỉ lấy trong scene hiện tại
                {
                    manager.gameObject.SetActive(true); // Active để trigger Awake
                    break;
                }
            }
        }
    }
    
    public void CompleteTask(int taskId)
    {
        // Đảm bảo đã initialize
        if (!isInitialized) InitializeTaskManager();
        
        Task task = tasks.Find(t => t.id == taskId);
        if (task != null && !task.isCompleted)
        {
            task.isCompleted = true;
            UpdateTaskUI(taskId);
            
            Debug.Log($"Task '{task.taskName}' marked as completed.");
            
            if (notificationManager != null)
            {
                notificationManager.ShowTaskCompletionNotification(task);
            }
        }
    }
    
    public void ToggleTaskCompletion(int taskId)
    {
        Task task = tasks.Find(t => t.id == taskId);
        if (task != null)
        {
            if (!task.isCompleted)
            {
                CompleteTask(taskId);
            }
            else
            {
                task.isCompleted = false;
                UpdateTaskUI(taskId);
            }
        }
    }
    
    void UpdateTaskUI(int taskId)
    {
        foreach (GameObject taskUI in taskUIElements)
        {
            if (taskUI == null) continue;
            
            TaskUI taskUIScript = taskUI.GetComponent<TaskUI>();
            if (taskUIScript != null && taskUIScript.GetTaskId() == taskId)
            {
                taskUIScript.UpdateTaskDisplay();
                break;
            }
        }
    }
    
    // Method để hiển thị quest panel
    public void ShowQuestPanel()
    {
        gameObject.SetActive(true);
    }
    
    // Method để ẩn quest panel (nhưng vẫn giữ instance)
    public void HideQuestPanel()
    {
        if (taskContainer != null && taskContainer.gameObject != taskContainer.root.gameObject)
        {
            // Chỉ ẩn UI container, không ẩn toàn bộ TaskManager
            taskContainer.gameObject.SetActive(false);
        }
    }
}

// Static class để access TaskManager từ bất kỳ đâu
public static class TaskSystem
{
    public static void CompleteTask(int taskId)
    {
        TaskManager.EnsureInitialized();
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.CompleteTask(taskId);
        }
    }
    
    public static void ShowQuests()
    {
        TaskManager.EnsureInitialized();
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.ShowQuestPanel();
        }
    }
}

// Rest of the classes remain the same...
public class NotificationManager : MonoBehaviour
{
    private Canvas canvas;
    private List<GameObject> activeNotifications = new List<GameObject>();
    
    public void Initialize(Canvas notificationCanvas)
    {
        canvas = notificationCanvas;
        Debug.Log("NotificationManager initialized");
    }
    
    public void ShowTaskCompletionNotification(Task task)
    {
        if (canvas == null) return;
        
        GameObject notification = CreateNotificationUI(task);
        activeNotifications.Add(notification);
        StartCoroutine(AnimateNotification(notification));
    }
    
    GameObject CreateNotificationUI(Task task)
    {
        GameObject notificationObj = new GameObject("TaskNotification");
        notificationObj.transform.SetParent(canvas.transform, false);
        
        RectTransform rectTransform = notificationObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(400, 100);
        rectTransform.anchorMin = new Vector2(1, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(1, 1);
        
        float yOffset = -120 * activeNotifications.Count;
        rectTransform.anchoredPosition = new Vector2(20, -50 + yOffset);
        
        Image background = notificationObj.AddComponent<Image>();
        background.color = new Color(0.2f, 0.8f, 0.2f, 0.9f);
        
        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(notificationObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(340, 80);
        textRect.anchorMin = new Vector2(0, 0.5f);
        textRect.anchorMax = new Vector2(1, 0.5f);
        textRect.anchoredPosition = new Vector2(30, 0);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = $"<b>Tâche terminée!</b>\n{task.taskName}";
        text.fontSize = 20;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.verticalAlignment = VerticalAlignmentOptions.Middle;
        
        return notificationObj;
    }
    
    IEnumerator AnimateNotification(GameObject notification)
    {
        RectTransform rect = notification.GetComponent<RectTransform>();
        Vector2 targetPos = rect.anchoredPosition;
        Vector2 startPos = new Vector2(450, targetPos.y);
        
        rect.anchoredPosition = startPos;
        
        float duration = 0.5f;
        float elapsed = 0;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            //  float t = Mathf.SmoothStep(0, elapsed / duration);
            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, elapsed / duration);
            yield return null;
        }
        
        rect.anchoredPosition = targetPos;
        
        yield return new WaitForSeconds(3f);
        
        if (notification != null)
        {
            StartCoroutine(HideNotification(notification));
        }
    }
    
    IEnumerator HideNotification(GameObject notification)
    {
        if (notification == null) yield break;
        
        activeNotifications.Remove(notification);
        Destroy(notification);
        yield return null;
    }
}

[System.Serializable]
public class Task
{
    public int id;
    public string taskName;
    public string description;
    public bool isCompleted;

    public Task(int id, string name, string desc)
    {
        this.id = id;
        this.taskName = name;
        this.description = desc;
        this.isCompleted = false;
    }
}
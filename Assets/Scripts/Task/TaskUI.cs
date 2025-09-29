using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TaskUI : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI taskNameText;
    public TextMeshProUGUI taskDescriptionText;
    public Button taskButton;
    public Image backgroundImage;
    public Image checkmarkImage;
    public CanvasGroup canvasGroup;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color completedColor = Color.gray;
    public Color checkmarkColor = Color.green;

    private Task currentTask;
    private TaskManager taskManager;

    public void SetupTask(Task task, TaskManager manager)
    {
        currentTask = task;
        taskManager = manager;

        taskNameText.text = task.taskName;
        taskDescriptionText.text = task.description;

        //taskButton.onClick.AddListener(() => taskManager.ToggleTaskCompletion(task.id));

        UpdateTaskDisplay();
    }

    public void UpdateTaskDisplay()
    {
        if (currentTask.isCompleted)
        {
            // Hiệu ứng khi hoàn thành: làm xám, hiện checkmark
            backgroundImage.color = completedColor;
            canvasGroup.alpha = 0.6f;
            //            checkmarkImage.gameObject.SetActive(true);
            //            checkmarkImage.color = checkmarkColor;

            // Gạch ngang text
            taskNameText.fontStyle = FontStyles.Strikethrough;
            taskDescriptionText.fontStyle = FontStyles.Strikethrough;
        }
        else
        {
            // Trạng thái bình thường
            backgroundImage.color = normalColor;
            canvasGroup.alpha = 1f;
            //            checkmarkImage.gameObject.SetActive(false);

            // Bỏ gạch ngang
            taskNameText.fontStyle = FontStyles.Normal;
            taskDescriptionText.fontStyle = FontStyles.Normal;
        }
    }

    public int GetTaskId()
    {
        return currentTask.id;
    }
}

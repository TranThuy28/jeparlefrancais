using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestNotificationManager : MonoBehaviour
{
    [Header("Notification UI")]
    public GameObject notificationPrefab;
    public Transform notificationParent;
    public Canvas notificationCanvas;
    
    [Header("Quest Progress Notification")]
    public GameObject progressNotificationPrefab;
    public Transform progressNotificationParent;
    
    [Header("Audio")]
    public AudioClip questCompletedSound;
    public AudioClip questProgressSound;
    public AudioClip rewardClaimedSound;
    
    private Queue<NotificationData> notificationQueue = new Queue<NotificationData>();
    private List<GameObject> activeNotifications = new List<GameObject>();
    private AudioSource audioSource;
    private bool isShowingNotification = false;

    [System.Serializable]
    public class NotificationData
    {
        public string title;
        public string message;
        public Sprite icon;
        public Color backgroundColor = Color.white;
        public float duration = 3f;
        public NotificationType type;
    }

    public enum NotificationType
    {
        QuestCompleted,
        QuestProgress,
        RewardClaimed,
        DailyMissionCompleted
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnEnable()
    {
        QuestManager.OnQuestCompleted += ShowQuestCompletedNotification;
        QuestManager.OnQuestProgressUpdated += ShowQuestProgressNotification;
        QuestManager.OnQuestClaimed += ShowRewardClaimedNotification;
    }

    private void OnDisable()
    {
        QuestManager.OnQuestCompleted -= ShowQuestCompletedNotification;
        QuestManager.OnQuestProgressUpdated -= ShowQuestProgressNotification;
        QuestManager.OnQuestClaimed -= ShowRewardClaimedNotification;
    }

    private void ShowQuestCompletedNotification(Quest quest)
    {
        NotificationData notification = new NotificationData
        {
            title = "Quest Completed!",
            message = quest.title,
            icon = quest.icon,
            backgroundColor = Color.green,
            duration = 4f,
            type = NotificationType.QuestCompleted
        };

        QueueNotification(notification);
        PlayNotificationSound(questCompletedSound);
    }

    private void ShowQuestProgressNotification(Quest quest)
    {
        if (quest.IsCompleted) return; // Don't show progress for completed quests

        // Show progress notification (smaller, less intrusive)
        StartCoroutine(ShowProgressNotification(quest));
        PlayNotificationSound(questProgressSound);
    }

    private void ShowRewardClaimedNotification(Quest quest)
    {
        string rewardText = "";
        if (quest.reward.coins > 0) rewardText += $"{quest.reward.coins} coins ";
        if (quest.reward.gems > 0) rewardText += $"{quest.reward.gems} gems ";
        if (quest.reward.experience > 0) rewardText += $"{quest.reward.experience} XP ";

        NotificationData notification = new NotificationData
        {
            title = "Rewards Claimed!",
            message = rewardText.Trim(),
            icon = quest.icon,
            backgroundColor = Color.yellow,
            duration = 3f,
            type = NotificationType.RewardClaimed
        };

        QueueNotification(notification);
        PlayNotificationSound(rewardClaimedSound);
    }

    private void QueueNotification(NotificationData notification)
    {
        notificationQueue.Enqueue(notification);
        
        if (!isShowingNotification)
        {
            StartCoroutine(ProcessNotificationQueue());
        }
    }

    private IEnumerator ProcessNotificationQueue()
    {
        isShowingNotification = true;

        while (notificationQueue.Count > 0)
        {
            NotificationData notification = notificationQueue.Dequeue();
            yield return StartCoroutine(ShowNotification(notification));
            yield return new WaitForSeconds(0.5f); // Brief pause between notifications
        }

        isShowingNotification = false;
    }

    private IEnumerator ShowNotification(NotificationData notification)
    {
        GameObject notificationObj = Instantiate(notificationPrefab, notificationParent);
        QuestNotificationUI notificationUI = notificationObj.GetComponent<QuestNotificationUI>();
        
        if (notificationUI != null)
        {
            notificationUI.SetupNotification(notification);
            activeNotifications.Add(notificationObj);
        }

        // Animate in
        notificationObj.transform.localScale = Vector3.zero;
        LeanTween.scale(notificationObj, Vector3.one, 0.3f)
            .setEase(LeanTweenType.easeOutBack);

        yield return new WaitForSeconds(notification.duration);

        // Animate out
        LeanTween.scale(notificationObj, Vector3.zero, 0.2f)
            .setEase(LeanTweenType.easeInBack)
            .setOnComplete(() => {
                activeNotifications.Remove(notificationObj);
                Destroy(notificationObj);
            });

        yield return new WaitForSeconds(0.2f);
    }

    private IEnumerator ShowProgressNotification(Quest quest)
    {
        GameObject progressObj = Instantiate(progressNotificationPrefab, progressNotificationParent);
        QuestProgressNotificationUI progressUI = progressObj.GetComponent<QuestProgressNotificationUI>();
        
        if (progressUI != null)
        {
            progressUI.SetupProgressNotification(quest);
        }

        // Slide in from right
        RectTransform rectTransform = progressObj.GetComponent<RectTransform>();
        Vector3 startPos = rectTransform.anchoredPosition;
        startPos.x += 300f;
        rectTransform.anchoredPosition = startPos;

        LeanTween.moveX(rectTransform, startPos.x - 300f, 0.3f)
            .setEase(LeanTweenType.easeOutQuad);

        yield return new WaitForSeconds(2f);

        // Slide out to right
        LeanTween.moveX(rectTransform, startPos.x, 0.3f)
            .setEase(LeanTweenType.easeInQuad)
            .setOnComplete(() => Destroy(progressObj));
    }

    private void PlayNotificationSound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void ClearAllNotifications()
    {
        StopAllCoroutines();
        
        foreach (GameObject notification in activeNotifications)
        {
            if (notification != null)
                Destroy(notification);
        }
        
        activeNotifications.Clear();
        notificationQueue.Clear();
        isShowingNotification = false;
    }

    // Method to manually show custom notification
    public void ShowCustomNotification(string title, string message, Sprite icon = null, Color? bgColor = null, float duration = 3f)
    {
        NotificationData notification = new NotificationData
        {
            title = title,
            message = message,
            icon = icon,
            backgroundColor = bgColor ?? Color.white,
            duration = duration,
            type = NotificationType.QuestCompleted
        };

        QueueNotification(notification);
    }
}

public class QuestNotificationUI : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public Image iconImage;
    public Image backgroundImage;
    public Button closeButton;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseNotification);
    }

    public void SetupNotification(QuestNotificationManager.NotificationData data)
    {
        if (titleText != null)
            titleText.text = data.title;
        
        if (messageText != null)
            messageText.text = data.message;
        
        if (iconImage != null && data.icon != null)
        {
            iconImage.sprite = data.icon;
            iconImage.gameObject.SetActive(true);
        }
        else if (iconImage != null)
        {
            iconImage.gameObject.SetActive(false);
        }
        
        if (backgroundImage != null)
            backgroundImage.color = data.backgroundColor;
    }

    private void CloseNotification()
    {
        LeanTween.scale(gameObject, Vector3.zero, 0.2f)
            .setEase(LeanTweenType.easeInBack)
            .setOnComplete(() => Destroy(gameObject));
    }
}

public class QuestProgressNotificationUI : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI questTitleText;
    public TextMeshProUGUI progressText;
    public Slider progressBar;
    public Image questIcon;

    public void SetupProgressNotification(Quest quest)
    {
        if (questTitleText != null)
            questTitleText.text = quest.title;
        
        if (progressText != null)
            progressText.text = $"{quest.currentProgress}/{quest.requiredProgress}";
        
        if (progressBar != null)
            progressBar.value = quest.ProgressPercentage;
        
        if (questIcon != null && quest.icon != null)
            questIcon.sprite = quest.icon;
    }
}
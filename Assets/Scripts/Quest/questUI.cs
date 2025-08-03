using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestUIItem : MonoBehaviour
{
    [Header("UI Components")]
    public Image questIcon;
    public TextMeshProUGUI questTitle;
    public TextMeshProUGUI questDescription;
    public TextMeshProUGUI progressText;
    public Slider progressBar;
    public Button claimButton;
    public GameObject progressPanel;
    public GameObject completedPanel;
    
    [Header("Reward Display")]
    public TextMeshProUGUI coinRewardText;
    public TextMeshProUGUI gemRewardText;
    public TextMeshProUGUI expRewardText;
    public Image coinIcon;
    public Image gemIcon;
    public Image expIcon;
    
    [Header("Visual States")]
    public Color normalColor = Color.white;
    public Color completedColor = Color.green;
    public Color claimedColor = Color.gray;

    private Quest currentQuest;
    private QuestManager questManager;
    private Image backgroundImage;

    private void Awake()
    {
        backgroundImage = GetComponent<Image>();
        SetupClaimButton();
    }

    private void SetupClaimButton()
    {
        if (claimButton != null)
        {
            claimButton.onClick.AddListener(ClaimReward);
        }
    }

    public void SetupQuestItem(Quest quest, QuestManager manager)
    {
        currentQuest = quest;
        questManager = manager;
        UpdateQuestDisplay();
    }

    public void UpdateQuestDisplay()
    {
        if (currentQuest == null) return;

        // Cập nhật thông tin cơ bản
        if (questTitle != null)
            questTitle.text = currentQuest.title;
        
        if (questDescription != null)
            questDescription.text = currentQuest.description;

        // Cập nhật icon
        if (questIcon != null && currentQuest.icon != null)
            questIcon.sprite = currentQuest.icon;

        // Cập nhật progress
        UpdateProgress();
        
        // Cập nhật reward display
        UpdateRewardDisplay();
        
        // Cập nhật visual state
        UpdateVisualState();
    }

    public void UpdateProgress()
    {
        if (currentQuest == null) return;

        // Cập nhật progress bar
        if (progressBar != null)
        {
            progressBar.value = currentQuest.ProgressPercentage;
        }

        // Cập nhật progress text
        if (progressText != null)
        {
            progressText.text = $"{currentQuest.currentProgress}/{currentQuest.requiredProgress}";
        }

        // Hiển thị panel tương ứng
        UpdatePanelVisibility();
    }

    private void UpdateRewardDisplay()
    {
        if (currentQuest?.reward == null) return;

        // Hiển thị coin reward
        if (coinRewardText != null && currentQuest.reward.coins > 0)
        {
            coinRewardText.text = currentQuest.reward.coins.ToString();
            coinIcon.gameObject.SetActive(true);
        }
        else if (coinIcon != null)
        {
            coinIcon.gameObject.SetActive(false);
        }

        // Hiển thị gem reward
        if (gemRewardText != null && currentQuest.reward.gems > 0)
        {
            gemRewardText.text = currentQuest.reward.gems.ToString();
            gemIcon.gameObject.SetActive(true);
        }
        else if (gemIcon != null)
        {
            gemIcon.gameObject.SetActive(false);
        }

        // Hiển thị exp reward
        if (expRewardText != null && currentQuest.reward.experience > 0)
        {
            expRewardText.text = currentQuest.reward.experience.ToString();
            expIcon.gameObject.SetActive(true);
        }
        else if (expIcon != null)
        {
            expIcon.gameObject.SetActive(false);
        }
    }

    private void UpdatePanelVisibility()
    {
        if (currentQuest == null) return;

        switch (currentQuest.status)
        {
            case QuestStatus.InProgress:
                if (progressPanel != null) progressPanel.SetActive(true);
                if (completedPanel != null) completedPanel.SetActive(false);
                if (claimButton != null) claimButton.gameObject.SetActive(false);
                break;

            case QuestStatus.Completed:
                if (progressPanel != null) progressPanel.SetActive(false);
                if (completedPanel != null) completedPanel.SetActive(true);
                if (claimButton != null) 
                {
                    claimButton.gameObject.SetActive(true);
                    claimButton.interactable = true;
                }
                break;

            case QuestStatus.Claimed:
                if (progressPanel != null) progressPanel.SetActive(false);
                if (completedPanel != null) completedPanel.SetActive(true);
                if (claimButton != null) 
                {
                    claimButton.gameObject.SetActive(true);
                    claimButton.interactable = false;
                }
                break;
        }
    }

    private void UpdateVisualState()
    {
        if (currentQuest == null || backgroundImage == null) return;

        switch (currentQuest.status)
        {
            case QuestStatus.InProgress:
                backgroundImage.color = normalColor;
                break;
            case QuestStatus.Completed:
                backgroundImage.color = completedColor;
                break;
            case QuestStatus.Claimed:
                backgroundImage.color = claimedColor;
                break;
        }
    }

    private void ClaimReward()
    {
        if (currentQuest != null && questManager != null && currentQuest.CanClaim)
        {
            questManager.ClaimQuestReward(currentQuest);
            UpdateQuestDisplay();
            
            // Hiệu ứng claim reward
            ShowClaimEffect();
        }
    }

    private void ShowClaimEffect()
    {
        // Implement claim effect (animation, particles, etc.)
        if (claimButton != null)
        {
            // Tạo hiệu ứng scale
            LeanTween.scale(claimButton.gameObject, Vector3.one * 1.2f, 0.1f)
                .setEase(LeanTweenType.easeOutQuad)
                .setOnComplete(() => {
                    LeanTween.scale(claimButton.gameObject, Vector3.one, 0.1f)
                        .setEase(LeanTweenType.easeInQuad);
                });
        }
    }

    public string GetQuestId()
    {
        return currentQuest?.id ?? "";
    }

    // Method để test quest progress (có thể gọi từ button trong editor)
    [ContextMenu("Test Quest Progress")]
    public void TestQuestProgress()
    {
        if (currentQuest != null && questManager != null)
        {
            questManager.UpdateQuestProgress(currentQuest.type, 10);
        }
    }
}
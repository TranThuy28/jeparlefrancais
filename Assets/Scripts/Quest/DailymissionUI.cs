using UnityEngine;
using UnityEngine.UI;
using TMPro;
using InventoryPlus;

public class DailyMissionUIItem : MonoBehaviour
{
    [Header("UI Components")]
    public Image missionIcon;
    public TextMeshProUGUI missionTitle;
    public TextMeshProUGUI missionDescription;
    public TextMeshProUGUI progressText;
    public Slider progressBar;
    public Button claimButton;
    public GameObject completedIndicator;
    
    [Header("Reward Display")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI gemText;
    public TextMeshProUGUI expText;
    public Image coinIcon;
    public Image gemIcon;
    public Image expIcon;
    
    [Header("Visual Effects")]
    public ParticleSystem completionEffect;
    public Image backgroundImage;
    public Color normalColor = Color.white;
    public Color completedColor = Color.yellow;
    public Color claimedColor = Color.gray;

    private GameManager gameManager;
    private Inventory inventory;
    // Thêm cấu hình cho các loại task
    [System.Serializable]
    public struct TaskTypeConfig
    {
        public QuestType type;
        public Sprite icon;
        //public Sprite progressBarColor;
    }

    [Header("Task Type Configurations")]
    [SerializeField] private TaskTypeConfig[] taskTypeConfigs;

    public Quest currentMission;
    private DailyMissionManager missionManager;

    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        inventory = FindFirstObjectByType<Inventory>();
        if (claimButton != null)
            claimButton.onClick.AddListener(ClaimMissionReward);
    }

    private TaskTypeConfig GetTaskTypeConfig(QuestType type)
    {
        foreach (var config in taskTypeConfigs)
        {
            if (config.type == type)
                return config;
        }
        // Trả về config mặc định nếu không tìm thấy
        return new TaskTypeConfig 
        { 
            type = type, 
            icon = null, 
            //progressBarColor = null,
        };
    }
    
    public void UpdateTaskTypeVisuals()
    {
        // Tìm config phù hợp với loại task
        TaskTypeConfig config = GetTaskTypeConfig(currentMission.type);

        // Cập nhật icon
        if (missionIcon != null && config.icon != null)
        {
            missionIcon.sprite = config.icon;
            missionIcon.gameObject.SetActive(true);
        }
        else
        {
            missionIcon?.gameObject.SetActive(false);
        }
    }
    public void SetupMission(Quest mission, DailyMissionManager manager)
    {
        currentMission = mission;
        missionManager = manager;
        UpdateMissionDisplay();
    }
    
    private void UpdateMissionDisplay()
    {
        if (currentMission == null) return;

        // Update basic info
        if (missionTitle != null)
            missionTitle.text = currentMission.title;
        // Debug.Log("Mission Title: " + (missionTitle != null));
        // if (missionDescription != null)
        //     missionDescription.text = currentMission.description;

        // Update progress
        UpdateProgress();

        // Update rewards
        UpdateRewardDisplay();

        // Update visual state
        UpdateVisualState();
    }

    private void UpdateProgress()
    {
        if (currentMission == null) return;

        // Update progress bar
        if (progressBar != null)
        {
            progressBar.value = currentMission.ProgressPercentage;
        }

        // Update progress text
        if (progressText != null)
        {
            progressText.text = $"{currentMission.currentProgress}/{currentMission.requiredProgress}";
        }
    }

    private void UpdateRewardDisplay()
    {
        if (currentMission?.reward == null) return;

        // Coin reward
        if (coinText != null && currentMission.reward.coins > 0)
        {
            coinText.text = currentMission.reward.coins.ToString();
            coinIcon?.gameObject.SetActive(true);
        }
        else
        {
            coinIcon?.gameObject.SetActive(false);
        }

        // Gem reward
        // if (gemText != null && currentMission.reward.gems > 0)
        // {
        //     gemText.text = currentMission.reward.gems.ToString();
        //     gemIcon?.gameObject.SetActive(true);
        // }
        // else
        // {
        //     gemIcon?.gameObject.SetActive(false);
        // }

        // // Experience reward
        // if (expText != null && currentMission.reward.experience > 0)
        // {
        //     expText.text = currentMission.reward.experience.ToString();
        //     expIcon?.gameObject.SetActive(true);
        // }
        // else
        // {
        //     expIcon?.gameObject.SetActive(false);
        // }
    }

    private void UpdateVisualState()
    {
        if (currentMission == null) return;

        // Update button state
        if (claimButton != null)
        {
            //claimButton.gameObject.interactable = currentMission.IsCompleted;
            claimButton.interactable = currentMission.CanClaim;

            if (currentMission.status == QuestStatus.Claimed)
            {
                claimButton.GetComponentInChildren<TextMeshProUGUI>().text = "Claimed";
            }
            else if (currentMission.CanClaim)
            {
                claimButton.GetComponentInChildren<TextMeshProUGUI>().text = "Claim";
            }
            else
            {
                claimButton.GetComponentInChildren<TextMeshProUGUI>().text = "In Progress";
            }
        }

        // Update completed indicator
        if (completedIndicator != null)
        {
            completedIndicator.SetActive(currentMission.IsCompleted);
        }

        // Update background color
        if (backgroundImage != null)
        {
            switch (currentMission.status)
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
    }

    private void ClaimMissionReward()
    {
        if (currentMission != null && missionManager != null && currentMission.CanClaim)
        {
            missionManager.ClaimDailyMissionReward(currentMission);
            gameManager.currencyManager.AddCoins(currentMission.reward.coins);
            gameManager.currencyManager.AddGems(currentMission.reward.gems);
            if (currentMission.reward.items != null)
            {
                foreach (var item in currentMission.reward.items)
                {
                    inventory.AddInventory(item.item, item.quantity, 1f, false);
                }
            }
            //gameManager.currencyManager.AddExperience(currentMission.reward.experience);

            UpdateMissionDisplay();
            
            // Play claim effect
            PlayClaimEffect();
        }
    }

    private void PlayClaimEffect()
    {
        // Play particle effect
        if (completionEffect != null)
            completionEffect.Play();

        // Scale animation
        if (claimButton != null)
        {
            LeanTween.scale(claimButton.gameObject, Vector3.one * 1.1f, 0.1f)
                .setEase(LeanTweenType.easeOutBack)
                .setOnComplete(() => {
                    LeanTween.scale(claimButton.gameObject, Vector3.one, 0.1f);
                });
        }
    }

    public void RefreshDisplay()
    {
        UpdateMissionDisplay();
    }
}
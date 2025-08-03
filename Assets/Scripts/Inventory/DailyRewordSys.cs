using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using InventoryPlus;
public class DailyRewardSystem : MonoBehaviour
{
    [Header("Daily Reward UI")]
    public GameObject dailyRewardPanel;
    public Button dailyRewardButton;
    public Button claimAllButton;
    public TextMeshProUGUI completionText;
    public Transform rewardItemsParent;
    public GameObject rewardItemPrefab;
    public Inventory inventory;
    public GameManager gameManager;

    [Header("Reward Configuration")]
    public QuestReward questReward; // Cấu hình phần thưởng hàng ngày
    
    [Header("Animation")]
    public ParticleSystem claimEffect;
    public AudioClip claimSound;
    public DailyMissionManager dailyMissionManager;
    private AudioSource audioSource;
    private DailyRewardItem rewardItem;
    
    [System.Serializable]
    public class DailyRewardTier
    {
        public string tierName;
        public int requiredCompletedMissions;
        public Sprite rewardIcon;
        public int rewardAmount;
        public RewardType rewardType;
        public Color tierColor = Color.white;
    }
    
    public enum RewardType
    {
        Coins,
        Gems,
        Items,
        Experience
    }
    
    private void Start()
    {
        dailyMissionManager = FindFirstObjectByType<DailyMissionManager>();
        inventory = FindFirstObjectByType<Inventory>();
        gameManager = FindFirstObjectByType<GameManager>();
        SetRewardClaimed(false);
        Debug.Log("DailyRewardSystem initialized with DailyMissionManager: " + (dailyMissionManager != null));
        audioSource = GetComponent<AudioSource>();
        questReward = new QuestReward { coins = 300 };
        SetupUI();
        InitializeRewardItems();
    }
    
    private void SetupUI()
    {
        if (dailyRewardButton != null)
            dailyRewardButton.onClick.AddListener(OpenDailyRewardPanel);
            
        if (claimAllButton != null)
            claimAllButton.onClick.AddListener(ClaimAllAvailableRewards);
    }
    
    private void InitializeRewardItems()
    {
        // Clear existing items
        foreach (Transform child in rewardItemsParent)
        {
            Destroy(child.gameObject);
        }
        
        // Create reward item based on questReward
        if (questReward != null && rewardItemPrefab != null)
        {
            GameObject itemObj = Instantiate(rewardItemPrefab, rewardItemsParent);
            rewardItem = itemObj.GetComponent<DailyRewardItem>();
            
            if (rewardItem != null)
            {
                // Initialize với QuestReward trực tiếp
                rewardItem.Initialize(questReward, this);
            }
        }
        
        UpdateRewardState();
    }
    
    public void UpdateRewardState()
    {
        if (dailyMissionManager == null) return;
        
        int completedMissions = GetCompletedMissionsCount();
        int totalMissions = GetTotalMissionsCount();
        
        // Update completion text
        if (completionText != null)
        {
            completionText.text = $"Complete All Daily Missions: {completedMissions}/{totalMissions}";
        }
        
        // Update reward item state - chỉ có thể claim khi hoàn thành TẤT CẢ nhiệm vụ
        if (rewardItem != null)
        {
            bool allMissionsCompleted = completedMissions >= totalMissions;
            bool canClaim = allMissionsCompleted;
            bool alreadyClaimed = IsRewardClaimed();
            
            rewardItem.UpdateState(canClaim, alreadyClaimed, completedMissions, totalMissions);
        }
        
        // Update claim all button
        UpdateClaimAllButton();
    }
    
    private void UpdateClaimAllButton()
    {
        if (claimAllButton == null) return;
        
        int completedMissions = GetCompletedMissionsCount();
        int totalMissions = GetTotalMissionsCount();
        
        // Chỉ cho phép claim khi hoàn thành TẤT CẢ nhiệm vụ
        bool allMissionsCompleted = completedMissions >= totalMissions;
        bool canClaim = allMissionsCompleted && !IsRewardClaimed();
        
        claimAllButton.interactable = canClaim;
        
        // Change button appearance based on state
        // Image buttonImage = claimAllButton.GetComponent<Image>();
        // if (buttonImage != null)
        // {
        //     buttonImage.color = canClaim ? Color.white : Color.gray;
        // }
        
        // Update button text to show requirement
        TextMeshProUGUI buttonText = claimAllButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            if (IsRewardClaimed())
            {
                buttonText.text = "Claimed";
            }
            else if (allMissionsCompleted)
            {
                buttonText.text = "Claim";
            }
        }
    }
    
    public void ClaimReward()
    {
        if (IsRewardClaimed()) 
        {
            Debug.Log("Daily reward already claimed today!");
            return;
        }
        
        int completedMissions = GetCompletedMissionsCount();
        int totalMissions = GetTotalMissionsCount();
        
        // Kiểm tra xem đã hoàn thành TẤT CẢ nhiệm vụ chưa
        if (completedMissions < totalMissions)
        {
            Debug.Log($"Cannot claim reward! Complete all missions first. ({completedMissions}/{totalMissions})");
            return;
        }

        // Kiểm tra xem đã claim reward hôm nay chưa
        gameManager.currencyManager.AddCoins(questReward.coins);
        gameManager.currencyManager.AddGems(questReward.gems);
        if (questReward.items != null && questReward.items.Count > 0)
        {
            foreach (var itemEntry in questReward.items)
            {
                if (itemEntry.item != null && itemEntry.quantity > 0)
                {
                    inventory.AddInventory(itemEntry.item, itemEntry.quantity, 1f, false);
                }
            }
        }

        // Give quest reward to player
        GiveQuestRewardToPlayer(questReward);
        
        // Mark as claimed
        SetRewardClaimed(true);
        
        // Play effects
        PlayClaimEffects();
        
        // Update UI
        UpdateRewardState();
        
        Debug.Log($"Claimed daily quest rewards: {questReward.coins} coins, {questReward.gems} gems, {questReward.experience} exp");
    }
    
    public void ClaimAllAvailableRewards()
    {
        ClaimReward();
    }
    
    private void GiveQuestRewardToPlayer(QuestReward reward)
    {
        if (reward == null) return;
        
        // Give coins
        if (reward.coins > 0)
        {
            // GameManager.Instance.AddCoins(reward.coins);
            PlayerPrefs.SetInt("PlayerCoins", PlayerPrefs.GetInt("PlayerCoins", 0) + reward.coins);
            Debug.Log($"Added {reward.coins} coins from daily quest reward");
        }
        
        // Give gems
        if (reward.gems > 0)
        {
            // GameManager.Instance.AddGems(reward.gems);
            PlayerPrefs.SetInt("PlayerGems", PlayerPrefs.GetInt("PlayerGems", 0) + reward.gems);
            Debug.Log($"Added {reward.gems} gems from daily quest reward");
        }
        
        // Give experience
        if (reward.experience > 0)
        {
            // PlayerManager.Instance.AddExperience(reward.experience);
            PlayerPrefs.SetInt("PlayerExp", PlayerPrefs.GetInt("PlayerExp", 0) + reward.experience);
            Debug.Log($"Added {reward.experience} experience from daily quest reward");
        }

        // Give items
        if (reward.items != null && reward.items.Count > 0)
        {
            foreach (var itemEntry in reward.items)
            {
                if (itemEntry.item != null && itemEntry.quantity > 0)
                {
                    // InventoryManager.Instance.AddItem(itemEntry.item, itemEntry.quantity);
                    Debug.Log($"Added {itemEntry.quantity}x {itemEntry.item.name} from daily quest reward");
                }
            }
        }
        
    }
    
    private void PlayClaimEffects()
    {
        if (claimEffect != null)
            claimEffect.Play();
            
        if (audioSource != null && claimSound != null)
            audioSource.PlayOneShot(claimSound);
    }
    
    private int GetCompletedMissionsCount()
    {
        if (dailyMissionManager == null) return 0;
        
        // Bạn cần implement phương thức này để lấy số nhiệm vụ đã hoàn thành
        // Ví dụ: return dailyMissionManager.GetCompletedMissionsCount();
        
        int completed = 0;
        // Tạm thời trả về 0, bạn cần kết nối với DailyMissionManager thực tế
        return completed;
    }
    
    // Method để lấy tổng số missions (public để DailyRewardItem có thể sử dụng)
    public int GetTotalMissionsCount()
    {
        if (dailyMissionManager != null) return 0; // Default value

        return dailyMissionManager.dailyMissions.Count;
    }
    
    private bool IsRewardClaimed()
    {
        string key = $"DailyReward_{DateTime.Now:yyyy-MM-dd}";
        return PlayerPrefs.GetInt(key, 0) == 1;
    }
    
    private void SetRewardClaimed(bool claimed)
    {
        string key = $"DailyReward_{DateTime.Now:yyyy-MM-dd}";
        PlayerPrefs.SetInt(key, claimed ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    public void OpenDailyRewardPanel()
    {
        if (dailyRewardPanel != null)
        {
            dailyRewardPanel.SetActive(true);
            UpdateRewardState();
        }
    }
    
    public void CloseDailyRewardPanel()
    {
        if (dailyRewardPanel != null)
            dailyRewardPanel.SetActive(false);
    }
    
    // Reset rewards at midnight (called by DailyMissionManager)
    public void ResetDailyRewards()
    {
        UpdateRewardState();
    }
    
    // Method để kiểm tra xem có thể claim reward không (public để UI có thể sử dụng)
    public bool CanClaimReward()
    {
        int completedMissions = GetCompletedMissionsCount();
        int totalMissions = GetTotalMissionsCount();
        return completedMissions >= totalMissions && !IsRewardClaimed();
    }
    
    // Method để lấy thông tin tiến độ (public để UI có thể sử dụng)
    public (int completed, int total) GetMissionProgress()
    {
        return (GetCompletedMissionsCount(), GetTotalMissionsCount());
    }
}
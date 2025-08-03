using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
[Serializable]
public class DailyMissionManager : MonoBehaviour
{
    [Header("Daily Mission UI")]
    public GameObject dailyMissionPanel;
    public Transform dailyMissionParent;
    public GameObject dailyMissionItemPrefab;
    public TextMeshProUGUI timeRemainingText;
    public Button refreshButton;
    public Slider overallProgressSlider;
    public TextMeshProUGUI overallProgressText;

    public GameObject dailyRewardItemPrefab;

    public List<Quest> dailyMissions = new List<Quest>();
    private DateTime nextResetTime;
    private QuestManager questManager;

    private void Start()
    {
        questManager = FindFirstObjectByType<QuestManager>();
        InitializeDailyMissions();
        SetupUI();
        CalculateNextResetTime();
    }

    private void Update()
    {
        UpdateTimeRemaining();
        CheckForReset();
    }

    private void SetupUI()
    {
        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshDailyMissions);
    }

    private void InitializeDailyMissions()
    {
        CreateDailyMissions();
        RefreshDailyMissionUI();
    }

    private void CreateDailyMissions()
    {
        dailyMissions.Clear();

        // Daily Mission 1: Kill enemies
        Quest dailyKill = new Quest
        {
            id = "daily_kill_enemies",
            title = "Daily Slayer",
            description = "Defeat 50 enemies today",
            type = QuestType.KillMonsters,
            status = QuestStatus.InProgress,
            currentProgress = 50,
            requiredProgress = 50,
            reward = new QuestReward { coins = 500},
            isDaily = true,
            expiryTime = nextResetTime
        };

        // Daily Mission 2: Collect gold
        Quest dailyGold = new Quest
        {
            id = "daily_collect_gold",
            title = "Gold Collector",
            description = "Collect 1000 gold coins",
            type = QuestType.CollectItems,
            status = QuestStatus.InProgress,
            currentProgress = 350,
            requiredProgress = 1000,
            reward = new QuestReward { coins = 300},
            isDaily = true,
            expiryTime = nextResetTime
        };

        Quest dailyG = new Quest
        {
            id = "daily_collect_gold",
            title = "Gold Collector",
            description = "Collect 1000 gold coins",
            type = QuestType.CollectItems,
            status = QuestStatus.InProgress,
            currentProgress = 350,
            requiredProgress = 1000,
            reward = new QuestReward { coins = 300},
            isDaily = true,
            expiryTime = nextResetTime
        };

        // Daily Mission 3: Complete stages
        Quest dailyStages = new Quest
        {
            id = "daily_complete_stages",
            title = "Stage Master",
            description = "Complete 3 different stages",
            type = QuestType.Build,
            status = QuestStatus.InProgress,
            currentProgress = 1,
            requiredProgress = 3,
            reward = new QuestReward { coins = 800 },
            isDaily = true,
            expiryTime = nextResetTime
        };

        dailyMissions.AddRange(new[] { dailyKill, dailyGold, dailyG, dailyStages });
    }

    private void RefreshDailyMissionUI()
    {
        // Clear existing UI items
        foreach (Transform child in dailyMissionParent)
        {
            Destroy(child.gameObject);
        }

        // Create new UI items for each daily mission
        foreach (Quest mission in dailyMissions)
        {
            GameObject missionItem = Instantiate(dailyMissionItemPrefab, dailyMissionParent);
            DailyMissionUIItem uiItem = missionItem.GetComponent<DailyMissionUIItem>();

            if (uiItem != null)
                uiItem.SetupMission(mission, this);
            uiItem.UpdateTaskTypeVisuals();
        }
        // Create reward tiers
        UpdateOverallProgress();
    }

    private void UpdateOverallProgress()
    {
        int completedMissions = 0;
        int totalMissions = dailyMissions.Count;

        foreach (Quest mission in dailyMissions)
        {
            if (mission.IsCompleted)
                completedMissions++;
        }

        if (overallProgressSlider != null)
        {
            overallProgressSlider.value = (float)completedMissions / totalMissions;
        }

        if (overallProgressText != null)
        {
            overallProgressText.text = $"{completedMissions}/{totalMissions} Completed";
        }

        // UpdateRewardTiers(completedMissions, totalMissions);
    }

    // private void UpdateRewardTiers(int completed, int total)
    // {
    //     // Cập nhật reward tiers dựa trên số mission hoàn thành
    //     for (int i = 0; i < rewardTierButtons.Length; i++)
    //     {
    //         int requiredForTier = (i + 1) * (total / rewardTierButtons.Length);
    //         bool tierUnlocked = completed >= requiredForTier;
            
    //         if (rewardTierButtons[i] != null)
    //             rewardTierButtons[i].interactable = tierUnlocked;
            
    //         if (rewardProgressImages[i] != null)
    //             rewardProgressImages[i].fillAmount = tierUnlocked ? 1f : (float)completed / requiredForTier;
    //     }
    // }

    private void CalculateNextResetTime()
    {
        DateTime now = DateTime.Now;
        nextResetTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0).AddDays(1);
    }

    private void UpdateTimeRemaining()
    {
        if (timeRemainingText != null)
        {
            TimeSpan timeLeft = nextResetTime - DateTime.Now;
            if (timeLeft.TotalSeconds > 0)
            {
                timeRemainingText.text = $"Resets in: {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";
            }
            else
            {
                timeRemainingText.text = "Resetting...";
            }
        }
    }

    private void CheckForReset()
    {
        if (DateTime.Now >= nextResetTime)
        {
            ResetDailyMissions();
        }
    }

    private void ResetDailyMissions()
    {
        // Reset all daily missions
        foreach (Quest mission in dailyMissions)
        {
            mission.currentProgress = 0;
            mission.status = QuestStatus.InProgress;
        }

        CalculateNextResetTime();
        RefreshDailyMissionUI();
        
        Debug.Log("Daily missions have been reset!");
    }

    public void RefreshDailyMissions()
    {
        // Allow manual refresh (could cost gems)
        CreateDailyMissions();
        RefreshDailyMissionUI();
    }

    public void UpdateDailyMissionProgress(QuestType questType, int amount = 1)
    {
        foreach (Quest mission in dailyMissions)
        {
            if (mission.type == questType && mission.status == QuestStatus.InProgress)
            {
                mission.currentProgress = Mathf.Min(mission.currentProgress + amount, mission.requiredProgress);
                
                if (mission.IsCompleted)
                {
                    mission.status = QuestStatus.Completed;
                    ShowMissionCompletedEffect(mission);
                }
            }
        }
        
        RefreshDailyMissionUI();
    }

    public void ClaimDailyMissionReward(Quest mission)
    {
        if (mission.CanClaim)
        {
            // Give rewards
            if (questManager != null)
                questManager.ClaimQuestReward(mission);
            
            mission.status = QuestStatus.Claimed;
            RefreshDailyMissionUI();
        }
    }

    private void ShowMissionCompletedEffect(Quest mission)
    {
        Debug.Log($"Daily Mission Completed: {mission.title}");
        // Implement completion effect
    }

    public void OpenDailyMissionPanel()
    {
        dailyMissionPanel.SetActive(true);
        RefreshDailyMissionUI();
    }

    public void CloseDailyMissionPanel()
    {
        dailyMissionPanel.SetActive(false);
    }

    // Save/Load daily mission progress
    public void SaveDailyProgress()
    {
        string saveData = JsonUtility.ToJson(new DailyMissionSaveData
        {
            missions = dailyMissions,
            nextResetTime = nextResetTime.ToBinary()
        });
        
        PlayerPrefs.SetString("DailyMissions", saveData);
    }

    public void LoadDailyProgress()
    {
        if (PlayerPrefs.HasKey("DailyMissions"))
        {
            string saveData = PlayerPrefs.GetString("DailyMissions");
            DailyMissionSaveData data = JsonUtility.FromJson<DailyMissionSaveData>(saveData);
            
            dailyMissions = data.missions;
            nextResetTime = DateTime.FromBinary(data.nextResetTime);
        }
    }
}

[System.Serializable]
public class DailyMissionSaveData
{
    public List<Quest> missions;
    public long nextResetTime;
}
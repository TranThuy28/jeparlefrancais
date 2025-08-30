using System;
using System.Collections.Generic;
using UnityEngine;
using InventoryPlus;
[System.Serializable]
public enum QuestType
{
    KillMonsters,
    CollectItems,
    Build,
    ClimbRanks,
    OpenNewStage,
    DailyMission
}

[System.Serializable]
public enum QuestStatus
{
    NotStarted,
    InProgress,
    Completed,
    Claimed
}

[System.Serializable]
public class QuestReward : ScriptableObject
{
    public int coins;
    public int gems;
    public int experience;
    public List<RewardItemEntry> items;
}

[System.Serializable]
public class RewardItemEntry
{
    public Item item;         // Tham chiếu đến Item (ScriptableObject hoặc Prefab)
    public int quantity;      // Số lượng item
}

[System.Serializable]
public class Quest : ScriptableObject
{
    public string id;
    public string title;
    public string description;
    public QuestType type;
    public QuestStatus status;
    public int currentProgress;
    public int requiredProgress;
    public QuestReward reward;
    public Sprite icon;
    public bool isDaily;
    public DateTime expiryTime;

    public float ProgressPercentage => (float)currentProgress / requiredProgress;
    public bool IsCompleted => currentProgress >= requiredProgress;
    public bool CanClaim => IsCompleted && status == QuestStatus.InProgress;
}

public class QuestManager : MonoBehaviour
{
    [Header("Quest Configuration")]
    public List<Quest> allQuests = new List<Quest>();
    public List<Quest> activeQuests = new List<Quest>();
    public List<Quest> dailyQuests = new List<Quest>();

    // [Header("Events")]
    public static event Action<Quest> OnQuestCompleted;
    public static event Action<Quest> OnQuestProgressUpdated;
    public static event Action<Quest> OnQuestClaimed;
    public static event Action OnQuestListUpdated;

    private void Start()
    {
        InitializeQuests();
        LoadQuestProgress();
    }

    private void InitializeQuests()
    {
        // Tạo các quest mẫu
        CreateSampleQuests();
        RefreshDailyQuests();
    }

    private void CreateSampleQuests()
    {
        // Kill 100 monsters quest
        Quest killQuest = new Quest
        {
            id = "kill_100_monsters",
            title = "Kill 100 monsters",
            description = "Defeat 100 enemies to complete this quest",
            type = QuestType.KillMonsters,
            status = QuestStatus.InProgress,
            currentProgress = 45,
            requiredProgress = 100,
            reward = new QuestReward { coins = 1000, experience = 500 },
            isDaily = false
        };

        // Open New Stage quest
        Quest stageQuest = new Quest
        {
            id = "open_new_stage",
            title = "Open New Stage",
            description = "Unlock and access a new stage",
            type = QuestType.OpenNewStage,
            status = QuestStatus.InProgress,
            currentProgress = 1,
            requiredProgress = 1,
            reward = new QuestReward { gems = 50, experience = 200 },
            isDaily = false
        };

        // Collect Golds quest
        Quest goldQuest = new Quest
        {
            id = "collect_golds",
            title = "Collect Golds",
            description = "Gather gold coins from battles",
            type = QuestType.CollectItems,
            status = QuestStatus.InProgress,
            currentProgress = 750,
            requiredProgress = 1000,
            reward = new QuestReward { coins = 500, gems = 25 },
            isDaily = false
        };

        // Level Up quest
        Quest levelQuest = new Quest
        {
            id = "level_up",
            title = "Level Up",
            description = "Reach the next character level",
            type = QuestType.Build,
            status = QuestStatus.InProgress,
            currentProgress = 80,
            requiredProgress = 100,
            reward = new QuestReward { experience = 1000, gems = 100 },
            isDaily = false
        };

        // Climb the ranks quest
        Quest rankQuest = new Quest
        {
            id = "climb_ranks",
            title = "Climb the ranks",
            description = "Improve your ranking position",
            type = QuestType.KillMonsters,
            status = QuestStatus.NotStarted,
            currentProgress = 0,
            requiredProgress = 5,
            reward = new QuestReward { coins = 2000 },
            isDaily = false
        };

        allQuests.AddRange(new[] { killQuest, stageQuest, goldQuest, levelQuest, rankQuest });
        activeQuests.AddRange(allQuests);
    }

    public void UpdateQuestProgress(QuestType questType, int amount = 1)
    {
        foreach (Quest quest in activeQuests)
        {
            if (quest.type == questType && quest.status == QuestStatus.InProgress)
            {
                quest.currentProgress = Mathf.Min(quest.currentProgress + amount, quest.requiredProgress);
                OnQuestProgressUpdated?.Invoke(quest);

                if (quest.IsCompleted && quest.status == QuestStatus.InProgress)
                {
                    quest.status = QuestStatus.Completed;
                    OnQuestCompleted?.Invoke(quest);
                }
            }
        }
        
        OnQuestListUpdated?.Invoke();
    }

    public void ClaimQuestReward(Quest quest)
    {
        if (quest.CanClaim)
        {
            // Thêm reward vào inventory/player stats
            GiveReward(quest.reward);
            quest.status = QuestStatus.Claimed;
            OnQuestClaimed?.Invoke(quest);
            OnQuestListUpdated?.Invoke();
        }
    }

    private void GiveReward(QuestReward reward)
    {
        // Implement reward giving logic here
        // PlayerManager.Instance.AddCoins(reward.coins);
        // PlayerManager.Instance.AddGems(reward.gems);
        // PlayerManager.Instance.AddExperience(reward.experience);
        Debug.Log($"Rewarded: {reward.coins} coins, {reward.gems} gems, {reward.experience} XP");
    }

    public List<Quest> GetActiveQuests()
    {
        return activeQuests.FindAll(q => q.status != QuestStatus.Claimed);
    }

    public List<Quest> GetCompletedQuests()
    {
        return activeQuests.FindAll(q => q.status == QuestStatus.Completed);
    }

    public List<Quest> GetDailyQuests()
    {
        return dailyQuests;
    }

    private void RefreshDailyQuests()
    {
        // Logic để refresh daily quests
        dailyQuests.Clear();
        // Tạo daily quests mới
    }

    public void LoadQuestProgress()
    {
        // Load quest progress from PlayerPrefs or save file
    }

    public void SaveQuestProgress()
    {
        // Save quest progress to PlayerPrefs or save file
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            SaveQuestProgress();
    }
}
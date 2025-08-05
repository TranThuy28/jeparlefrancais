using UnityEngine;

// Script này sẽ tích hợp quest system vào game của bạn
public class GameQuestIntegration : MonoBehaviour
{
    [Header("Quest References")]
    public QuestManager questManager;
    public QuestUIManager questUIManager;
    public DailyMissionManager dailyMissionManager;
    public QuestNotificationManager notificationManager;
    
    [Header("Game References")]
    public PlayerController playerController;
    public GameManager gameManager;
    public EnemyManager enemyManager;
    public LootManager lootManager;
    public LevelManager levelManager;

    private void Start()
    {
        SetupQuestIntegration();
    }

    private void SetupQuestIntegration()
    {
        // Đăng ký các sự kiện game với quest system
        if (playerController != null)
        {
            playerController.OnEnemyKilled += OnEnemyKilled;
            //playerController.OnLevelUp += OnPlayerLevelUp;
            playerController.OnGoldCollected += OnGoldCollected;
        }

        if (levelManager != null)
        {
            levelManager.OnStageCompleted += OnStageCompleted;
            levelManager.OnNewStageUnlocked += OnNewStageUnlocked;
        }
    }

    #region Game Event Handlers

    private void OnEnemyKilled(GameObject enemy)
    {
        // Cập nhật quest progress khi giết enemy
        if (questManager != null)
        {
            questManager.UpdateQuestProgress(QuestType.KillMonsters, 1);
        }

        if (dailyMissionManager != null)
        {
            dailyMissionManager.UpdateDailyMissionProgress(QuestType.KillMonsters, 1);
        }

        Debug.Log($"Enemy killed: {enemy.name}. Quest progress updated.");
    }

    private void OnGoldCollected(int amount)
    {
        // Cập nhật quest progress khi thu thập gold
        if (questManager != null)
        {
            questManager.UpdateQuestProgress(QuestType.CollectItems, amount);
        }

        if (dailyMissionManager != null)
        {
            dailyMissionManager.UpdateDailyMissionProgress(QuestType.CollectItems, amount);
        }

        Debug.Log($"Gold collected: {amount}. Quest progress updated.");
    }

    // private void OnPlayerLevelUp(int newLevel)
    // {
    //     // Cập nhật quest progress khi level up
    //     if (questManager != null)
    //     {
    //         questManager.UpdateQuestProgress(QuestType.ReachLevel, 1);
    //     }

    //     if (dailyMissionManager != null)
    //     {
    //         dailyMissionManager.UpdateDailyMissionProgress(QuestType.ReachLevel, 1);
    //     }

    //     Debug.Log($"Player leveled up to {newLevel}. Quest progress updated.");
    // }

    private void OnStageCompleted(int stageNumber)
    {
        // Cập nhật quest progress khi hoàn thành stage
        if (questManager != null)
        {
            questManager.UpdateQuestProgress(QuestType.OpenNewStage, 1);
        }

        if (dailyMissionManager != null)
        {
            dailyMissionManager.UpdateDailyMissionProgress(QuestType.OpenNewStage, 1);
        }

        Debug.Log($"Stage {stageNumber} completed. Quest progress updated.");
    }

    private void OnNewStageUnlocked(int stageNumber)
    {
        // Có thể tạo quest mới khi unlock stage mới
        Debug.Log($"New stage {stageNumber} unlocked!");
    }

    private void OnRankImproved(int newRank)
    {
        // Cập nhật quest progress khi cải thiện rank
        if (questManager != null)
        {
            questManager.UpdateQuestProgress(QuestType.ClimbRanks, 1);
        }

        if (dailyMissionManager != null)
        {
            dailyMissionManager.UpdateDailyMissionProgress(QuestType.ClimbRanks, 1);
        }

        Debug.Log($"Rank improved to {newRank}. Quest progress updated.");
    }

    #endregion

    #region UI Integration Methods

    public void OpenQuestMenu()
    {
        if (questUIManager != null)
        {
            questUIManager.OpenQuestPanel();
        }
    }

    public void OpenDailyMissions()
    {
        if (dailyMissionManager != null)
        {
            dailyMissionManager.OpenDailyMissionPanel();
        }
    }

    public void ShowQuestNotification(string title, string message)
    {
        if (notificationManager != null)
        {
            notificationManager.ShowCustomNotification(title, message);
        }
    }

    #endregion

    #region Save/Load Integration

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveAllQuestData();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveAllQuestData();
        }
    }

    private void SaveAllQuestData()
    {
        if (questManager != null)
            questManager.SaveQuestProgress();

        if (dailyMissionManager != null)
            dailyMissionManager.SaveDailyProgress();

        Debug.Log("All quest data saved.");
    }

    private void LoadAllQuestData()
    {
        if (questManager != null)
            questManager.LoadQuestProgress();

        if (dailyMissionManager != null)
            dailyMissionManager.LoadDailyProgress();

        Debug.Log("All quest data loaded.");
    }

    #endregion

    #region Debug Methods (for testing)

    [ContextMenu("Test Kill Enemy")]
    public void TestKillEnemy()
    {
        OnEnemyKilled(new GameObject("Test Enemy"));
    }

    [ContextMenu("Test Collect Gold")]
    public void TestCollectGold()
    {
        OnGoldCollected(100);
    }

    // [ContextMenu("Test Level Up")]
    // public void TestLevelUp()
    // {
    //     OnPlayerLevelUp(10);
    // }

    [ContextMenu("Test Complete Stage")]
    public void TestCompleteStage()
    {
        OnStageCompleted(1);
    }

    [ContextMenu("Test Rank Improvement")]
    public void TestRankImprovement()
    {
        OnRankImproved(5);
    }

    #endregion
}

// Các interface và class giả định cho game của bạn
public class PlayerController : MonoBehaviour
{
    public System.Action<GameObject> OnEnemyKilled;
    public System.Action<int> OnLevelUp;
    public System.Action<int> OnGoldCollected;

    // Gọi các event này từ code game của bạn
    public void KillEnemy(GameObject enemy)
    {
        OnEnemyKilled?.Invoke(enemy);
    }

    public void LevelUp(int newLevel)
    {
        OnLevelUp?.Invoke(newLevel);
    }

    public void CollectGold(int amount)
    {
        OnGoldCollected?.Invoke(amount);
    }
}

public class EnemyManager : MonoBehaviour
{
    // Enemy management logic
}

public class LootManager : MonoBehaviour
{
    // Loot management logic
}

public class LevelManager : MonoBehaviour
{
    public System.Action<int> OnStageCompleted;
    public System.Action<int> OnNewStageUnlocked;

    public void CompleteStage(int stageNumber)
    {
        OnStageCompleted?.Invoke(stageNumber);
    }

    public void UnlockNewStage(int stageNumber)
    {
        OnNewStageUnlocked?.Invoke(stageNumber);
    }
}
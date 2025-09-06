using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
[Serializable]
public class DailyMissionManager : MonoBehaviour
{
    [Header("Interface Missions Quotidiennes")]
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

    public DailyMissionSpawner missionSpawner;

    private void InitializeSpawner()
    {
        missionSpawner = GetComponent<DailyMissionSpawner>();
        if (missionSpawner == null)
        {
            missionSpawner = gameObject.AddComponent<DailyMissionSpawner>();
        }
        Debug.Log("Mission Spawner initialisé: " + (missionSpawner != null));
    }

    // Updated CreateDailyMissions method
    private void CreateDailyMissionsWithSpawner()
    {
        if (missionSpawner == null)
        {
            Debug.LogError("Mission Spawner est null! Initialisation...");
            InitializeSpawner();
        }

        if (missionSpawner == null)
        {
            Debug.LogError("Impossible d'initialiser Mission Spawner! Utilisation des missions par défaut.");
            CreateDailyMissions();
            return;
        }

        dailyMissions.Clear();
        dailyMissions = missionSpawner.SpawnDailyMissions();
        Debug.Log($"Généré {dailyMissions.Count} missions quotidiennes avec le spawner.");
    }

    // Method to spawn special event missions
    public void StartSpecialEvent(string eventName)
    {
        if (missionSpawner == null)
        {
            Debug.LogError("Mission Spawner non disponible pour l'événement!");
            return;
        }
        
        List<Quest> eventMissions = missionSpawner.SpawnEventMissions(eventName);
        dailyMissions.AddRange(eventMissions);
        RefreshDailyMissionUI();
    }
    
    private void Start()
    {
        questManager = FindFirstObjectByType<QuestManager>();
        
        // Initialiser le spawner EN PREMIER
        InitializeSpawner();
        
        // Ensuite initialiser les missions
        CalculateNextResetTime();
        SetupUI();
        InitializeDailyMissions();
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
        // Essayer de charger les données sauvegardées d'abord
        LoadDailyProgress();
        
        // Si pas de données sauvegardées ou missions expirées, créer nouvelles missions
        if (dailyMissions.Count == 0 || DateTime.Now >= nextResetTime)
        {
            CreateDailyMissionsWithSpawner();
            SaveDailyProgress();
        }
        
        RefreshDailyMissionUI();
    }

    private void CreateDailyMissions()
    {
        Debug.Log("Création des missions par défaut (fallback)");
        dailyMissions.Clear();

        // Daily Mission 1: Kill enemies
        Quest dailyKill = new Quest
        {
            id = "quotidien_tuer_ennemis",
            title = "Tueur Quotidien",
            description = "Vaincre 50 ennemis aujourd'hui",
            type = QuestType.KillMonsters,
            status = QuestStatus.InProgress,
            currentProgress = 0,
            requiredProgress = 50,
            reward = new QuestReward { coins = 500},
            isDaily = true,
            expiryTime = nextResetTime
        };

        // Daily Mission 2: Collect gold
        Quest dailyGold = new Quest
        {
            id = "quotidien_collecter_or",
            title = "Collectionneur d'Or",
            description = "Collecter 1000 pièces d'or",
            type = QuestType.CollectItems,
            status = QuestStatus.InProgress,
            currentProgress = 0,
            requiredProgress = 1000,
            reward = new QuestReward { coins = 300},
            isDaily = true,
            expiryTime = nextResetTime
        };

        // Daily Mission 3: Complete stages
        Quest dailyStages = new Quest
        {
            id = "quotidien_terminer_niveaux",
            title = "Maître des Niveaux",
            description = "Terminer 3 niveaux différents",
            type = QuestType.Build,
            status = QuestStatus.InProgress,
            currentProgress = 0,
            requiredProgress = 3,
            reward = new QuestReward { coins = 800 },
            isDaily = true,
            expiryTime = nextResetTime
        };

        dailyMissions.AddRange(new[] { dailyKill, dailyGold, dailyStages });
        Debug.Log($"Créé {dailyMissions.Count} missions par défaut");
    }

    private void RefreshDailyMissionUI()
    {
        CreateDailyMissions();
        Debug.Log($"Rafraîchissement de l'interface avec {dailyMissions.Count} missions");
        
        // Clear existing UI items
        foreach (Transform child in dailyMissionParent)
        {
            Destroy(child.gameObject);
        }

        // Vérifier si nous avons des missions à afficher
        if (dailyMissions.Count == 0)
        {
            Debug.LogWarning("Aucune mission quotidienne à afficher!");
            return;
        }

        // Create new UI items for each daily mission
        foreach (Quest mission in dailyMissions)
        {
            if (dailyMissionItemPrefab == null)
            {
                Debug.LogError("dailyMissionItemPrefab est null!");
                continue;
            }

            GameObject missionItem = Instantiate(dailyMissionItemPrefab, dailyMissionParent);
            DailyMissionUIItem uiItem = missionItem.GetComponent<DailyMissionUIItem>();

            if (uiItem != null)
            {
                uiItem.SetupMission(mission, this);
                uiItem.UpdateTaskTypeVisuals();
            }
            else
            {
                Debug.LogError("DailyMissionUIItem component non trouvé sur le prefab!");
            }
        }
        
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
            overallProgressSlider.value = totalMissions > 0 ? (float)completedMissions / totalMissions : 0f;
        }

        if (overallProgressText != null)
        {
            overallProgressText.text = $"{completedMissions}/{totalMissions} Terminées";
        }
    }

    private void CalculateNextResetTime()
    {
        DateTime now = DateTime.Now;
        nextResetTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0).AddDays(1);
        Debug.Log($"Prochaine réinitialisation: {nextResetTime}");
    }

    private void UpdateTimeRemaining()
    {
        if (timeRemainingText != null)
        {
            TimeSpan timeLeft = nextResetTime - DateTime.Now;
            if (timeLeft.TotalSeconds > 0)
            {
                timeRemainingText.text = $"Réinitialise dans: {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";
            }
            else
            {
                timeRemainingText.text = "Réinitialisation...";
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
        Debug.Log("Réinitialisation des missions quotidiennes!");
        
        // Créer de nouvelles missions
        CreateDailyMissionsWithSpawner();
        
        CalculateNextResetTime();
        SaveDailyProgress();
        RefreshDailyMissionUI();
        
        Debug.Log("Les missions quotidiennes ont été réinitialisées!");
    }

    public void RefreshDailyMissions()
    {
        // Allow manual refresh (could cost gems)
        Debug.Log("Rafraîchissement manuel des missions");
        CreateDailyMissionsWithSpawner();
        SaveDailyProgress();
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
        
        SaveDailyProgress();
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
            SaveDailyProgress();
            RefreshDailyMissionUI();
        }
    }

    private void ShowMissionCompletedEffect(Quest mission)
    {
        Debug.Log($"Mission Quotidienne Terminée: {mission.title}");
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
        try
        {
            string saveData = JsonUtility.ToJson(new DailyMissionSaveData
            {
                missions = dailyMissions,
                nextResetTime = nextResetTime.ToBinary()
            });
            
            PlayerPrefs.SetString("MissionsQuotidiennes", saveData);
            PlayerPrefs.Save();
            Debug.Log("Progrès des missions sauvegardé");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur lors de la sauvegarde: {e.Message}");
        }
    }

    public void LoadDailyProgress()
    {
        try
        {
            if (PlayerPrefs.HasKey("MissionsQuotidiennes"))
            {
                string saveData = PlayerPrefs.GetString("MissionsQuotidiennes");
                DailyMissionSaveData data = JsonUtility.FromJson<DailyMissionSaveData>(saveData);
                
                if (data != null && data.missions != null)
                {
                    dailyMissions = data.missions;
                    nextResetTime = DateTime.FromBinary(data.nextResetTime);
                    Debug.Log($"Progrès chargé: {dailyMissions.Count} missions");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur lors du chargement: {e.Message}");
            dailyMissions.Clear();
        }
    }
    
    // Method for debugging
    [ContextMenu("Forcer Création Missions")]
    public void ForceCreateMissions()
    {
        CreateDailyMissionsWithSpawner();
        RefreshDailyMissionUI();
    }
    
    [ContextMenu("Afficher Info Debug")]
    public void ShowDebugInfo()
    {
        Debug.Log($"Spawner: {(missionSpawner != null ? "OK" : "NULL")}");
        Debug.Log($"Missions: {dailyMissions.Count}");
        Debug.Log($"Panel Parent: {(dailyMissionParent != null ? "OK" : "NULL")}");
        Debug.Log($"Prefab: {(dailyMissionItemPrefab != null ? "OK" : "NULL")}");
    }
}

[System.Serializable]
public class DailyMissionSaveData
{
    public List<Quest> missions;
    public long nextResetTime;
}
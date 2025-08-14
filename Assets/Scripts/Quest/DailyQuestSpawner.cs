using System;
using System.Collections.Generic;
using UnityEngine;

public class DailyMissionSpawner : MonoBehaviour
{
    [Header("Mission Templates")]
    public List<MissionTemplate> missionTemplates = new List<MissionTemplate>();

    [Header("Spawn Settings")]
    public int dailyMissionCount = 10;
    public int minDifficulty = 1;
    public int maxDifficulty = 3;

    private DailyMissionManager missionManager;

    private void Start()
    {
        missionManager = GetComponent<DailyMissionManager>();
        InitializeMissionTemplates();
    }

    private void InitializeMissionTemplates()
    {
        // Combat Missions
        missionTemplates.Add(new MissionTemplate
        {
            id = "kill_enemies",
            title = "Monster Hunter",
            description = "Defeat {0} enemies",
            type = QuestType.KillMonsters,
            minProgress = 20,
            maxProgress = 100,
            minReward = 300,
            maxReward = 800,
            weight = 10
        });

        missionTemplates.Add(new MissionTemplate
        {
            id = "kill_bosses",
            title = "Boss Slayer",
            description = "Defeat {0} bosses",
            type = QuestType.KillMonsters,
            minProgress = 2,
            maxProgress = 5,
            minReward = 500,
            maxReward = 1200,
            weight = 5
        });

        // Collection Missions
        missionTemplates.Add(new MissionTemplate
        {
            id = "collect_gold",
            title = "Gold Collector",
            description = "Collect {0} gold coins",
            type = QuestType.CollectItems,
            minProgress = 500,
            maxProgress = 2000,
            minReward = 200,
            maxReward = 600,
            weight = 8
        });

        missionTemplates.Add(new MissionTemplate
        {
            id = "collect_items",
            title = "Item Gatherer",
            description = "Collect {0} items",
            type = QuestType.CollectItems,
            minProgress = 10,
            maxProgress = 50,
            minReward = 300,
            maxReward = 700,
            weight = 7
        });

        // Building/Crafting Missions
        missionTemplates.Add(new MissionTemplate
        {
            id = "build_structures",
            title = "Master Builder",
            description = "Build {0} structures",
            type = QuestType.Build,
            minProgress = 3,
            maxProgress = 10,
            minReward = 400,
            maxReward = 900,
            weight = 6
        });

        missionTemplates.Add(new MissionTemplate
        {
            id = "upgrade_items",
            title = "Equipment Enhancer",
            description = "Upgrade {0} items",
            type = QuestType.Build,
            minProgress = 2,
            maxProgress = 8,
            minReward = 350,
            maxReward = 750,
            weight = 5
        });

        // Stage/Level Missions
        missionTemplates.Add(new MissionTemplate
        {
            id = "complete_stages",
            title = "Stage Master",
            description = "Complete {0} stages",
            type = QuestType.Build,
            minProgress = 2,
            maxProgress = 5,
            minReward = 600,
            maxReward = 1000,
            weight = 8
        });

        // Survival Missions
        missionTemplates.Add(new MissionTemplate
        {
            id = "survive_time",
            title = "Survivor",
            description = "Survive for {0} minutes",
            type = QuestType.KillMonsters,
            minProgress = 10,
            maxProgress = 30,
            minReward = 400,
            maxReward = 800,
            weight = 6
        });
    }

    public List<Quest> SpawnDailyMissions()
    {
        List<Quest> newMissions = new List<Quest>();
        List<MissionTemplate> availableTemplates = new List<MissionTemplate>(missionTemplates);
        DateTime nextResetTime = CalculateNextResetTime();
        Debug.Log($"Daily mission count: {availableTemplates.Count}");
        for (int i = 0; i < dailyMissionCount; i++)
        {
            if (availableTemplates.Count == 0) break;

            MissionTemplate selectedTemplate = SelectRandomTemplate(availableTemplates);
            Quest newMission = CreateMissionFromTemplate(selectedTemplate, nextResetTime);
            Debug.Log($"Spawned daily mission: {newMission.title}");
            newMissions.Add(newMission);
            availableTemplates.Remove(selectedTemplate); // Prevent duplicate missions
        }

        return newMissions;
    }

    private MissionTemplate SelectRandomTemplate(List<MissionTemplate> templates)
    {
        // Weighted random selection
        int totalWeight = 0;
        foreach (var template in templates)
        {
            totalWeight += template.weight;
        }

        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var template in templates)
        {
            currentWeight += template.weight;
            if (randomValue < currentWeight)
            {
                return template;
            }
        }

        return templates[0]; // Fallback
    }

    private Quest CreateMissionFromTemplate(MissionTemplate template, DateTime expiryTime)
    {
        int difficulty = UnityEngine.Random.Range(minDifficulty, maxDifficulty + 1);

        // Scale progress and reward based on difficulty
        float difficultyMultiplier = difficulty / (float)maxDifficulty;

        int requiredProgress = Mathf.RoundToInt(
            Mathf.Lerp(template.minProgress, template.maxProgress, difficultyMultiplier)
        );

        int rewardCoins = Mathf.RoundToInt(
            Mathf.Lerp(template.minReward, template.maxReward, difficultyMultiplier)
        );

        return new Quest
        {
            id = $"daily_{template.id}_{DateTime.Now.Ticks}",
            title = template.title,
            description = string.Format(template.description, requiredProgress),
            type = template.type,
            status = QuestStatus.InProgress,
            currentProgress = 0,
            requiredProgress = requiredProgress,
            reward = new QuestReward { coins = rewardCoins },
            isDaily = true,
            expiryTime = expiryTime
        };
    }

    private DateTime CalculateNextResetTime()
    {
        DateTime now = DateTime.Now;
        return new DateTime(now.Year, now.Month, now.Day, 0, 0, 0).AddDays(1);
    }

    // Method to generate special event missions
    public List<Quest> SpawnEventMissions(string eventName)
    {
        List<Quest> eventMissions = new List<Quest>();
        DateTime eventEndTime = DateTime.Now.AddDays(7); // Event lasts 1 week

        switch (eventName.ToLower())
        {
            case "combat_event":
                eventMissions.AddRange(GenerateCombatEventMissions(eventEndTime));
                break;
            case "collection_event":
                eventMissions.AddRange(GenerateCollectionEventMissions(eventEndTime));
                break;
            case "building_event":
                eventMissions.AddRange(GenerateBuildingEventMissions(eventEndTime));
                break;
        }

        return eventMissions;
    }

    private List<Quest> GenerateCombatEventMissions(DateTime endTime)
    {
        return new List<Quest>
        {
            new Quest
            {
                id = "event_combat_elite",
                title = "Elite Destroyer",
                description = "Defeat 20 elite enemies",
                type = QuestType.KillMonsters,
                status = QuestStatus.InProgress,
                currentProgress = 0,
                requiredProgress = 20,
                reward = new QuestReward { coins = 1500 },
                isDaily = false,
                expiryTime = endTime
            }
        };
    }

    private List<Quest> GenerateCollectionEventMissions(DateTime endTime)
    {
        return new List<Quest>
        {
            new Quest
            {
                id = "event_rare_collect",
                title = "Rare Collector",
                description = "Collect 10 rare items",
                type = QuestType.CollectItems,
                status = QuestStatus.InProgress,
                currentProgress = 0,
                requiredProgress = 10,
                reward = new QuestReward { coins = 1200 },
                isDaily = false,
                expiryTime = endTime
            }
        };
    }

    private List<Quest> GenerateBuildingEventMissions(DateTime endTime)
    {
        return new List<Quest>
        {
            new Quest
            {
                id = "event_master_builder",
                title = "Master Architect",
                description = "Build 15 structures",
                type = QuestType.Build,
                status = QuestStatus.InProgress,
                currentProgress = 0,
                requiredProgress = 15,
                reward = new QuestReward { coins = 2000 },
                isDaily = false,
                expiryTime = endTime
            }
        };
    }
}

[Serializable]
public class MissionTemplate
{
    public string id;
    public string title;
    public string description;
    public QuestType type;
    public int minProgress;
    public int maxProgress;
    public int minReward;
    public int maxReward;
    public int weight; // Probability weight for spawning
}
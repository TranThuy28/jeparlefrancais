using System;
using UnityEngine;


[System.Serializable]
public class ReputationManager : MonoBehaviour
{
    [Header("Reputation Settings")]
    public int reputation = 0;
    public string reputationRank = "Neutral";
    
    [Header("Reputation Ranks")]
    public string[] reputationRanks = {
        "Hated", "Disliked", "Neutral", "Liked", "Respected", "Revered", "Legendary"
    };
    
    public int[] rankThresholds = {
        -1000, -500, 0, 500, 1000, 2000, 5000
    };
    
    public static ReputationManager Instance;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadReputation();
            UpdateRank();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void AddReputation(int amount)
    {
        reputation += amount;
        UpdateRank();
        SaveReputation();
        OnReputationChanged?.Invoke(reputation);
    }
    
    public void SetReputation(int amount)
    {
        reputation = amount;
        UpdateRank();
        SaveReputation();
        OnReputationChanged?.Invoke(reputation);
    }
    
    private void UpdateRank()
    {
        string previousRank = reputationRank;
        
        for (int i = rankThresholds.Length - 1; i >= 0; i--)
        {
            if (reputation >= rankThresholds[i])
            {
                reputationRank = reputationRanks[i];
                break;
            }
        }
        
        if (previousRank != reputationRank)
        {
            OnRankChanged?.Invoke(reputationRank);
        }
    }
    
    // Events
    public System.Action<int> OnReputationChanged;
    public System.Action<string> OnRankChanged;
    
    // Save/Load
    private void SaveReputation()
    {
        PlayerPrefs.SetInt("Reputation", reputation);
        PlayerPrefs.SetString("ReputationRank", reputationRank);
    }
    
    private void LoadReputation()
    {
        reputation = PlayerPrefs.GetInt("Reputation", 0);
        reputationRank = PlayerPrefs.GetString("ReputationRank", "Neutral");
    }
}
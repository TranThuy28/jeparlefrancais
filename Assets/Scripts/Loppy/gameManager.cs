using System;
using UnityEngine;

// ===== GAME MANAGER CHÍNH =====
public class GameManager : MonoBehaviour
{
    [Header("Managers")]
    public CurrencyManager currencyManager;
    public ScoreManager scoreManager;
    public ReputationManager reputationManager;
    public GameTimeManager timeManager;
    
    public static GameManager Instance;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManagers();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeManagers()
    {
        // Tự động tìm các manager nếu chưa được gán
        if (currencyManager == null)
            currencyManager = FindAnyObjectByType<CurrencyManager>();
        if (scoreManager == null)
            scoreManager = FindAnyObjectByType<ScoreManager>();
        if (reputationManager == null)
            reputationManager = FindAnyObjectByType<ReputationManager>();
        if (timeManager == null)
            timeManager = FindAnyObjectByType<GameTimeManager>();
    }
    
    // Các phương thức tiện ích
    public void RewardPlayer(int coins, int gems, int experience, int reputation)
    {
        if (currencyManager != null)
        {
            currencyManager.AddCoins(coins);
            currencyManager.AddGems(gems);
        }
        
        if (scoreManager != null)
        {
            scoreManager.AddExperience(experience);
        }
        
        if (reputationManager != null)
        {
            reputationManager.AddReputation(reputation);
        }
    }
    
    public void SaveAllData()
    {
        // Dữ liệu sẽ tự động được lưu trong mỗi manager
        PlayerPrefs.Save();
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveAllData();
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveAllData();
        }
    }
}
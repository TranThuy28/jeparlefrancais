using System;
using UnityEngine;

// ===== TIME MANAGER =====
[System.Serializable]
public class GameTimeManager : MonoBehaviour
{
    [Header("Time Settings")]
    public float timeScale = 1f;
    public bool isPaused = false;
    
    [Header("Game Time")]
    public int gameDay = 1;
    public int gameHour = 6;
    public int gameMinute = 0;
    public float realSecondsPerGameMinute = 1f;
    
    [Header("Daily System")]
    public bool hasCheckedInToday = false;
    public string lastLoginDate = "";
    public int consecutiveDays = 0;
    
    private float timeAccumulator = 0f;
    
    public static GameTimeManager Instance;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadTimeData();
            CheckDailyLogin();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        if (!isPaused)
        {
            UpdateGameTime();
        }
    }
    
    private void UpdateGameTime()
    {
        timeAccumulator += Time.deltaTime * timeScale;
        
        if (timeAccumulator >= realSecondsPerGameMinute)
        {
            timeAccumulator -= realSecondsPerGameMinute;
            gameMinute++;
            
            if (gameMinute >= 60)
            {
                gameMinute = 0;
                gameHour++;
                OnHourChanged?.Invoke(gameHour);
                
                if (gameHour >= 24)
                {
                    gameHour = 0;
                    gameDay++;
                    OnDayChanged?.Invoke(gameDay);
                    hasCheckedInToday = false;
                }
            }
            
            OnTimeChanged?.Invoke(gameHour, gameMinute);
            SaveTimeData();
        }
    }
    
    public void PauseTime()
    {
        isPaused = true;
        Time.timeScale = 0f;
    }
    
    public void ResumeTime()
    {
        isPaused = false;
        Time.timeScale = 1f;
    }
    
    public void SetTimeScale(float scale)
    {
        timeScale = scale;
    }
    
    public string GetFormattedTime()
    {
        return string.Format("Day {0}, {1:00}:{2:00}", gameDay, gameHour, gameMinute);
    }
    
    public string GetTimeOfDay()
    {
        if (gameHour >= 6 && gameHour < 12) return "Morning";
        else if (gameHour >= 12 && gameHour < 18) return "Afternoon";
        else if (gameHour >= 18 && gameHour < 22) return "Evening";
        else return "Night";
    }
    
    // Daily Check-in System
    public bool DailyCheckIn()
    {
        if (!hasCheckedInToday)
        {
            hasCheckedInToday = true;
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            
            if (lastLoginDate == DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"))
            {
                consecutiveDays++;
            }
            else if (lastLoginDate != today)
            {
                consecutiveDays = 1;
            }
            
            lastLoginDate = today;
            SaveTimeData();
            OnDailyCheckIn?.Invoke(consecutiveDays);
            
            // Reward cho daily check-in
            int coinReward = 100 + (consecutiveDays * 10);
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddCoins(coinReward);
            }
            
            return true;
        }
        return false;
    }
    
    private void CheckDailyLogin()
    {
        string today = DateTime.Now.ToString("yyyy-MM-dd");
        if (lastLoginDate != today)
        {
            hasCheckedInToday = false;
        }
    }
    
    // Events
    public System.Action<int, int> OnTimeChanged;
    public System.Action<int> OnHourChanged;
    public System.Action<int> OnDayChanged;
    public System.Action<int> OnDailyCheckIn;
    
    // Save/Load
    private void SaveTimeData()
    {
        PlayerPrefs.SetInt("GameDay", gameDay);
        PlayerPrefs.SetInt("GameHour", gameHour);
        PlayerPrefs.SetInt("GameMinute", gameMinute);
        PlayerPrefs.SetString("LastLoginDate", lastLoginDate);
        PlayerPrefs.SetInt("ConsecutiveDays", consecutiveDays);
        PlayerPrefs.SetInt("HasCheckedIn", hasCheckedInToday ? 1 : 0);
    }
    
    private void LoadTimeData()
    {
        gameDay = PlayerPrefs.GetInt("GameDay", 1);
        gameHour = PlayerPrefs.GetInt("GameHour", 6);
        gameMinute = PlayerPrefs.GetInt("GameMinute", 0);
        lastLoginDate = PlayerPrefs.GetString("LastLoginDate", "");
        consecutiveDays = PlayerPrefs.GetInt("ConsecutiveDays", 0);
        hasCheckedInToday = PlayerPrefs.GetInt("HasCheckedIn", 0) == 1;
    }
}

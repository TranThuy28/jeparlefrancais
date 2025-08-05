using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

// ===== UI CONTROLLER MẪU =====
public class GameUIController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI gemText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI reputationText;
    public TextMeshProUGUI timeText;
    public Button checkInButton;

    private void Start()
    {
        // Đăng ký events
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged += UpdateCurrencyUI;
        }
        
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += UpdateScoreUI;
            ScoreManager.Instance.OnLevelUp += OnPlayerLevelUp;
        }
        
        if (ReputationManager.Instance != null)
        {
            ReputationManager.Instance.OnReputationChanged += UpdateReputationUI;
        }
        
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnTimeChanged += UpdateTimeUI;
            GameTimeManager.Instance.OnDailyCheckIn += OnDailyCheckIn;
        }
        
        // Setup button
        if (checkInButton != null)
        {
            checkInButton.onClick.AddListener(DailyCheckIn);
        }
        
        UpdateAllUI();
    }
    
    private void UpdateAllUI()
    {
        UpdateCurrencyUI();
        UpdateScoreUI(0);
        UpdateReputationUI(0);
        UpdateTimeUI(0, 0);
    }
    
    private void UpdateCurrencyUI()
    {
        if (CurrencyManager.Instance != null)
        {
            if (coinText != null) coinText.text = CurrencyManager.Instance.coins.ToString();
            if (gemText != null) gemText.text = CurrencyManager.Instance.gems.ToString();
            if (goldText != null) goldText.text = CurrencyManager.Instance.gold.ToString();
            //Debug.Log("Coins: " + CurrencyManager.Instance.coins);
        }
    }
    
    private void UpdateScoreUI(int score)
    {
        if (ScoreManager.Instance != null)
        {
            if (scoreText != null) scoreText.text = "Score: " + ScoreManager.Instance.currentScore;
            if (levelText != null) levelText.text = "Level: " + ScoreManager.Instance.level;
        }
    }
    
    private void UpdateReputationUI(int reputation)
    {
        if (ReputationManager.Instance != null)
        {
            if (reputationText != null)
                reputationText.text = "Reputation: " + ReputationManager.Instance.reputationRank;
        }
    }
    
    private void UpdateTimeUI(int hour, int minute)
    {
        if (GameTimeManager.Instance != null && timeText != null)
        {
            timeText.text = "day " + GameTimeManager.Instance.gameDay.ToString();
        }
    }
    
    private void OnPlayerLevelUp(int newLevel)
    {
        Debug.Log("Player leveled up to " + newLevel);
        // Có thể thêm hiệu ứng level up ở đây
    }
    
    private void OnDailyCheckIn(int consecutiveDays)
    {
        Debug.Log("Daily check-in successful! Consecutive days: " + consecutiveDays);
    }
    
    private void DailyCheckIn()
    {
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.DailyCheckIn();
        }
    }
}
using System;
using UnityEngine;

// ===== CURRENCY MANAGER =====
[System.Serializable]
public class CurrencyManager : MonoBehaviour
{
    [Header("Currency Settings")]
    public int coins = 100000;
    public int gems = 50;
    public int gold = 100;
    
    public static CurrencyManager Instance;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //Instance.AddCoins(100000);
            DontDestroyOnLoad(gameObject);
            LoadCurrency();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Thêm tiền
    public void AddCoins(int amount)
    {
        coins += amount;
        SaveCurrency();
        OnCurrencyChanged?.Invoke();
    }
    
    public void AddGems(int amount)
    {
        gems += amount;
        SaveCurrency();
        OnCurrencyChanged?.Invoke();
    }
    
    public void AddGold(int amount)
    {
        gold += amount;
        SaveCurrency();
        OnCurrencyChanged?.Invoke();
    }
    
    // Tiêu tiền
    public bool SpendCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            SaveCurrency();
            OnCurrencyChanged?.Invoke();
            return true;
        }
        return false;
    }
    
    public bool SpendGems(int amount)
    {
        if (gems >= amount)
        {
            gems -= amount;
            SaveCurrency();
            OnCurrencyChanged?.Invoke();
            return true;
        }
        return false;
    }
    
    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            SaveCurrency();
            OnCurrencyChanged?.Invoke();
            return true;
        }
        return false;
    }
    
    // Events
    public System.Action OnCurrencyChanged;
    
    // Save/Load
    private void SaveCurrency()
    {
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.SetInt("Gems", gems);
        PlayerPrefs.SetInt("Gold", gold);
    }
    
    private void LoadCurrency()
    {
        coins = PlayerPrefs.GetInt("Coins", 1000);
        gems = PlayerPrefs.GetInt("Gems", 50);
        gold = PlayerPrefs.GetInt("Gold", 100);
    }
}
using System;
using UnityEngine;

// ===== SCORE MANAGER =====
[System.Serializable]
public class ScoreManager : MonoBehaviour
{
    [Header("Score Settings")]
    public int currentScore = 0;
    public int highScore = 0;
    public int level = 1;
    public int experience = 0;
    public int experienceToNext = 100;
    
    public static ScoreManager Instance;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadScore();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void AddScore(int points)
    {
        currentScore += points;
        if (currentScore > highScore)
        {
            highScore = currentScore;
        }
        SaveScore();
        OnScoreChanged?.Invoke(currentScore);
    }
    
    public void AddExperience(int exp)
    {
        experience += exp;
        CheckLevelUp();
        SaveScore();
        OnExperienceChanged?.Invoke(experience);
    }
    
    private void CheckLevelUp()
    {
        while (experience >= experienceToNext)
        {
            experience -= experienceToNext;
            level++;
            experienceToNext = Mathf.RoundToInt(experienceToNext * 1.2f);
            OnLevelUp?.Invoke(level);
        }
    }
    
    public void ResetCurrentScore()
    {
        currentScore = 0;
        OnScoreChanged?.Invoke(currentScore);
    }
    
    // Events
    public System.Action<int> OnScoreChanged;
    public System.Action<int> OnExperienceChanged;
    public System.Action<int> OnLevelUp;
    
    // Save/Load
    private void SaveScore()
    {
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.SetInt("Level", level);
        PlayerPrefs.SetInt("Experience", experience);
        PlayerPrefs.SetInt("ExperienceToNext", experienceToNext);
    }
    
    private void LoadScore()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        level = PlayerPrefs.GetInt("Level", 1);
        experience = PlayerPrefs.GetInt("Experience", 0);
        experienceToNext = PlayerPrefs.GetInt("ExperienceToNext", 100);
    }
}
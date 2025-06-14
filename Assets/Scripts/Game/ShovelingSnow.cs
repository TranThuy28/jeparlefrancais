// SnowGameManager.cs - Main game controller
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SnowGameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public float gameTime = 60f;
    public int targetScore = 100;
    
    [Header("UI References")]
    public Text scoreText;
    public Text timeText;
    public Text gameOverText;
    public Button restartButton;
    public RectTransform gameArea;
    
    [Header("Prefabs")]
    public GameObject snowflakePrefab;
    public GameObject shovelPrefab;
    
    [Header("Snow Settings")]
    public float snowSpawnRate = 2f;
    public float snowFallSpeed = 50f;
    public int maxSnowOnGround = 20;
    public int maxTotalSnow = 50;
    
    private int score = 0;
    private float currentTime;
    private bool gameActive = true;
    private List<GameObject> snowOnGround = new List<GameObject>();
    private List<GameObject> fallingSnow = new List<GameObject>();
    private GameObject shovel;
    private float nextSnowSpawn = 0f;
    
    void Start()
    {
        currentTime = gameTime;
        CreateShovel();
        UpdateUI();
        
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
            restartButton.gameObject.SetActive(false);
        }
        
        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);
    }
    
    void Update()
    {
        if (!gameActive) return;
        
        // Update timer
        currentTime -= Time.deltaTime;
        if (currentTime <= 0)
        {
            EndGame();
            return;
        }
        
        // Spawn snow continuously
        if (Time.time >= nextSnowSpawn)
        {
            SpawnSnow();
            nextSnowSpawn = Time.time + (1f / snowSpawnRate);
        }
        
        // Clean up destroyed snow from lists
        CleanupSnowLists();
        
        UpdateUI();
    }
    
    void CreateShovel()
    {
        if (shovelPrefab != null && gameArea != null)
        {
            shovel = Instantiate(shovelPrefab, gameArea);
            shovel.GetComponent<ShovelController>().SetGameManager(this);
        }
    }
    
    void SpawnSnow()
    {
        // Limit total snow to prevent performance issues
        if (snowOnGround.Count + fallingSnow.Count >= maxTotalSnow)
            return;
            
        if (snowflakePrefab != null && gameArea != null)
        {
            GameObject snow = Instantiate(snowflakePrefab, gameArea);
            
            // Random position at top of game area
            RectTransform snowRect = snow.GetComponent<RectTransform>();
            float randomX = Random.Range(-gameArea.rect.width / 2, gameArea.rect.width / 2);
            snowRect.anchoredPosition = new Vector2(randomX, gameArea.rect.height / 2);
            
            // Add to falling snow list and set up snow behavior
            fallingSnow.Add(snow);
            snow.GetComponent<Snowflake>().SetGameManager(this);
            snow.GetComponent<Snowflake>().SetFallSpeed(snowFallSpeed);
        }
    }
    
    void CleanupSnowLists()
    {
        // Remove null references from lists
        snowOnGround.RemoveAll(snow => snow == null);
        fallingSnow.RemoveAll(snow => snow == null);
    }
    
    public void AddSnowToGround(GameObject snow)
    {
        // Move from falling to ground list
        fallingSnow.Remove(snow);
        snowOnGround.Add(snow);
    }
    
    public void RemoveSnowFromGround(GameObject snow)
    {
        snowOnGround.Remove(snow);
    }
    
    public void RemoveFallingSnow(GameObject snow)
    {
        fallingSnow.Remove(snow);
    }
    
    public void AddScore(int points)
    {
        score += points;
        
        if (score >= targetScore)
        {
            EndGame(true);
        }
    }
    
    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
            
        if (timeText != null)
            timeText.text = "Time: " + Mathf.Max(0, Mathf.CeilToInt(currentTime));
    }
    
    void EndGame(bool won = false)
    {
        gameActive = false;
        
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            if (won)
                gameOverText.text = "You Win!\nScore: " + score;
            else
                gameOverText.text = "Time's Up!\nScore: " + score;
        }
        
        if (restartButton != null)
            restartButton.gameObject.SetActive(true);
    }
    
    public void RestartGame()
    {
        // Clean up existing snow
        foreach (GameObject snow in snowOnGround)
        {
            if (snow != null) Destroy(snow);
        }
        snowOnGround.Clear();
        
        foreach (GameObject snow in fallingSnow)
        {
            if (snow != null) Destroy(snow);
        }
        fallingSnow.Clear();
        
        // Clean up any remaining snow
        Snowflake[] allSnow = FindObjectsOfType<Snowflake>();
        foreach (Snowflake snow in allSnow)
        {
            Destroy(snow.gameObject);
        }
        
        // Reset game state
        score = 0;
        currentTime = gameTime;
        gameActive = true;
        nextSnowSpawn = Time.time + 1f; // Small delay before first spawn
        
        // Reset UI
        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);
        if (restartButton != null)
            restartButton.gameObject.SetActive(false);
            
        UpdateUI();
    }
    
    public bool IsGameActive()
    {
        return gameActive;
    }
    
    public RectTransform GetGameArea()
    {
        return gameArea;
    }
}

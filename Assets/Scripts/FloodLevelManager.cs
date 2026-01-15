using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Data-driven level manager that manages 12 progressive levels with win conditions.
/// Controls flood levels, leak activation, and level progression.
/// </summary>
public class FloodLevelManager : MonoBehaviour
{
    /// <summary>
    /// Serializable class that defines level data.
    /// </summary>
    [System.Serializable]
    public class LevelData
    {
        [Tooltip("Level name displayed in UI (e.g., 'Day 1', 'Day 2')")]
        public string levelName = "Level 1";
        
        [Tooltip("Story text shown before level starts (displayed in level start panel).")]
        [TextArea(3, 5)]
        public string startMessage = "The flood is rising...";
        
        [Tooltip("Water color for this level (mood/atmosphere).")]
        public Color waterColor = Color.blue;
        
        [Tooltip("Sky/lighting color for this level (mood/atmosphere).")]
        public Color skyColor = Color.cyan;
        
        [Tooltip("Speed at which water rises (flood level progression speed)")]
        public float waterRisingSpeed = 1f;
        
        [Tooltip("Maximum number of leaks that can be active at once (difficulty)")]
        public int maxActiveLeaks = 2;
        
        [Tooltip("Level duration in seconds (win condition: survive this long). Set to 0 to use repair count instead.")]
        public float levelDuration = 60f;
        
        [Tooltip("Number of leaks to repair to win (win condition). Set to 0 to use time duration instead.")]
        public int leaksToRepairTarget = 0;
    }
    
    [Header("References")]
    [Tooltip("Reference to the FloodController that manages water levels.")]
    public FloodController floodController;
    
    [Tooltip("Player Transform reference for position reset.")]
    public Transform playerTransform;
    
    [Tooltip("Spawn point where player should be reset at the start of each level.")]
    public Transform playerSpawnPoint;
    
    [Header("UI References")]
    [Tooltip("Text component to display the current leak count.")]
    public TextMeshProUGUI leakCountText;
    
    [Tooltip("Panel shown at the start of each level (blocks input, shows story message).")]
    public GameObject levelStartPanel;
    
    [Tooltip("Text component that displays the level start message/story.")]
    public TextMeshProUGUI levelMessageText;
    
    [Header("Environment References")]
    [Tooltip("Water material to apply color changes (for mood/atmosphere).")]
    public Material waterMaterial;
    
    [Tooltip("Directional light to apply sky color changes (for mood/atmosphere).")]
    public Light directionalLight;
    
    [Header("Level Data")]
    [Tooltip("List of 12 levels with their configurations.")]
    public List<LevelData> allLevels = new List<LevelData>();
    
    [Header("Current Level")]
    [Tooltip("Current level index (0-based).")]
    public int currentLevelIndex = 0;
    
    [Header("Leak Management")]
    [Tooltip("List of all LeakSpot components found in the scene.")]
    private List<LeakSpot> leakSpots = new List<LeakSpot>();
    
    // Level state tracking
    private float levelStartTime = 0f;
    private int leaksRepairedThisLevel = 0;
    private bool isLevelActive = false;
    private bool isLevelPaused = false; // Paused during level start message
    private Coroutine leakActivationCoroutine = null;
    private Coroutine waterRisingCoroutine = null;
    
    private void Awake()
    {
        // Initialize UI as hidden when game begins
        if (leakCountText != null)
        {
            leakCountText.gameObject.SetActive(false);
        }
        
        if (levelStartPanel != null)
        {
            levelStartPanel.SetActive(false);
        }
    }
    
    private void Start()
    {
        // Verify references
        VerifyReferences();
        
        // Find all leaks in the scene
        FindAllLeaks();
        
        // If this is a restart, StartGameplayLoop will be called from GameplayToCutsceneManager2
        // Otherwise, wait for cutscene signal
    }
    
    private void OnEnable()
    {
        // Subscribe to leak repair events
        LeakSpot.OnLeakRepaired += OnLeakRepaired;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from leak repair events to prevent memory leaks
        LeakSpot.OnLeakRepaired -= OnLeakRepaired;
    }
    
    private void Update()
    {
        // Safety check: If level start panel is active, ensure cursor stays unlocked
        if (levelStartPanel != null && levelStartPanel.activeSelf)
        {
            // Force unlock cursor in case player controller or other scripts try to lock it
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        
        // Check win condition if level is active and not paused
        if (isLevelActive && !isLevelPaused)
        {
            CheckWinCondition();
        }
    }
    
    /// <summary>
    /// Verifies all required references are set.
    /// </summary>
    private void VerifyReferences()
    {
        if (floodController == null)
        {
            floodController = FindObjectOfType<FloodController>();
            if (floodController == null)
            {
                Debug.LogWarning("[FloodLevelManager] FloodController not found. Please assign it manually.");
            }
        }
        
        if (playerTransform == null)
        {
            // Try to find player by tag or ManualCharacterController
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null)
            {
                ManualCharacterController controller = FindObjectOfType<ManualCharacterController>();
                if (controller != null)
                {
                    playerObj = controller.gameObject;
                }
            }
            
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("[FloodLevelManager] Player Transform not found. Please assign it manually.");
            }
        }
        
        if (playerSpawnPoint == null)
        {
            Debug.LogWarning("[FloodLevelManager] PlayerSpawnPoint not assigned. Player position will not be reset between levels.");
        }
        
        if (allLevels == null || allLevels.Count == 0)
        {
            Debug.LogWarning("[FloodLevelManager] No level data configured! Please add levels to allLevels list.");
        }
    }
    
    /// <summary>
    /// Finds all LeakSpot components in the scene.
    /// </summary>
    private void FindAllLeaks()
    {
        leakSpots.Clear();
        
        GameObject[] leakObjects = GameObject.FindGameObjectsWithTag("Leak");
        
        foreach (GameObject leakObj in leakObjects)
        {
            LeakSpot leakSpot = leakObj.GetComponent<LeakSpot>();
            if (leakSpot != null)
            {
                leakSpots.Add(leakSpot);
            }
            else
            {
                Debug.LogWarning($"[FloodLevelManager] GameObject '{leakObj.name}' has tag 'Leak' but no LeakSpot component found.");
            }
        }
        
        Debug.Log($"[FloodLevelManager] Found {leakSpots.Count} LeakSpot(s) in the scene.");
    }
    
    /// <summary>
    /// Initializes the manager and starts the gameplay loop.
    /// Call this method from your Cutscene Timeline Signal or Game Manager when gameplay should begin.
    /// </summary>
    public void StartGameplayLoop()
    {
        // Start with level 0
        StartLevel(0);
    }
    
    /// <summary>
    /// Starts a specific level by index.
    /// </summary>
    /// <param name="index">Level index (0-based)</param>
    public void StartLevel(int index)
    {
        // Check if all levels are completed
        if (index >= allLevels.Count)
        {
            Debug.Log("[FloodLevelManager] All levels completed! Triggering end game...");
            if (GameOverManager.Instance != null)
            {
                GameOverManager.Instance.TriggerEndGame();
            }
            return;
        }
        
        // Validate index
        if (index < 0 || index >= allLevels.Count)
        {
            Debug.LogError($"[FloodLevelManager] Invalid level index: {index}. Valid range: 0-{allLevels.Count - 1}");
            return;
        }
        
        Debug.Log($"[FloodLevelManager] Starting Level {index + 1}: {allLevels[index].levelName}");
        
        // Set current level index
        currentLevelIndex = index;
        
        // Reset level state
        levelStartTime = Time.time;
        leaksRepairedThisLevel = 0;
        isLevelActive = false; // Will be set to true after player confirms start
        isLevelPaused = true; // Pause until player clicks start
        
        // Stop any existing coroutines
        if (leakActivationCoroutine != null)
        {
            StopCoroutine(leakActivationCoroutine);
        }
        if (waterRisingCoroutine != null)
        {
            StopCoroutine(waterRisingCoroutine);
        }
        
        // Reset water height to minimum (level 0)
        if (floodController != null)
        {
            floodController.SetFloodLevel(0);
        }
        
        // Reset player position to spawn point
        if (playerTransform != null && playerSpawnPoint != null)
        {
            playerTransform.position = playerSpawnPoint.position;
            playerTransform.rotation = playerSpawnPoint.rotation;
            Debug.Log($"[FloodLevelManager] Player reset to spawn point: {playerSpawnPoint.position}");
        }
        
        // Clear/deactivate all existing leaks
        ClearAllLeaks();
        
        // Get current level data
        LevelData currentLevel = allLevels[index];
        
        // PAUSE GAME LOGIC - Show level start message
        Time.timeScale = 0f; // Freeze time
        
        // FORCE UNLOCK CURSOR FIRST (before showing panel, to prevent other scripts from locking it)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Show level start panel
        if (levelStartPanel != null)
        {
            levelStartPanel.SetActive(true);
        }
        
        // Set story message text
        if (levelMessageText != null)
        {
            levelMessageText.text = currentLevel.startMessage;
        }
        else
        {
            Debug.LogWarning("[FloodLevelManager] LevelMessageText not assigned! Story message will not display.");
        }
        
        // Apply environment changes (water color and sky color)
        ApplyEnvironmentChanges(currentLevel);
        
        // Ensure cursor stays unlocked (safety check)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log($"[FloodLevelManager] Level {index + 1} ready. Waiting for player to confirm start...");
    }
    
    /// <summary>
    /// Applies environment changes (water color and sky color) from level data.
    /// </summary>
    private void ApplyEnvironmentChanges(LevelData levelData)
    {
        // Apply water color
        if (waterMaterial != null)
        {
            waterMaterial.color = levelData.waterColor;
            Debug.Log($"[FloodLevelManager] Applied water color: {levelData.waterColor}");
        }
        else
        {
            Debug.LogWarning("[FloodLevelManager] WaterMaterial not assigned! Water color will not change.");
        }
        
        // Apply sky/lighting color
        if (directionalLight != null)
        {
            directionalLight.color = levelData.skyColor;
            Debug.Log($"[FloodLevelManager] Applied sky color: {levelData.skyColor}");
        }
        else
        {
            Debug.LogWarning("[FloodLevelManager] DirectionalLight not assigned! Sky color will not change.");
        }
    }
    
    /// <summary>
    /// Called from UI "START" button to confirm level start and begin gameplay.
    /// Hides level start panel and unpauses the game.
    /// </summary>
    public void ConfirmStartLevel()
    {
        if (!isLevelPaused)
        {
            Debug.LogWarning("[FloodLevelManager] ConfirmStartLevel called but level is not paused!");
            return;
        }
        
        Debug.Log("[FloodLevelManager] Player confirmed level start. Beginning gameplay...");
        
        // Hide level start panel FIRST
        if (levelStartPanel != null)
        {
            levelStartPanel.SetActive(false);
        }
        
        // LOCK CURSOR FOR GAMEPLAY (before unfreezing time)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Unfreeze time
        Time.timeScale = 1f;
        
        // Unpause level
        isLevelPaused = false;
        isLevelActive = true;
        
        // Reset level start time (now that gameplay actually begins)
        levelStartTime = Time.time;
        
        // Get current level data
        LevelData currentLevel = allLevels[currentLevelIndex];
        
        // Start leak activation system
        leakActivationCoroutine = StartCoroutine(LeakActivationLoop(currentLevel));
        
        // Start water rising system
        waterRisingCoroutine = StartCoroutine(WaterRisingLoop(currentLevel));
        
        // Initial UI update
        UpdateLeakUI();
        
        Debug.Log("[FloodLevelManager] Gameplay started!");
    }
    
    /// <summary>
    /// Clears/deactivates all leaks in the scene.
    /// </summary>
    private void ClearAllLeaks()
    {
        foreach (LeakSpot leak in leakSpots)
        {
            if (leak != null && leak.IsLeaking())
            {
                leak.StopLeak();
            }
        }
        UpdateLeakUI();
    }
    
    /// <summary>
    /// Coroutine that manages leak activation based on level difficulty.
    /// </summary>
    private IEnumerator LeakActivationLoop(LevelData levelData)
    {
        while (isLevelActive && !isLevelPaused)
        {
            // Wait a random interval before activating next leak
            float waitTime = Random.Range(3f, 8f);
            yield return new WaitForSeconds(waitTime);
            
            if (!isLevelActive || isLevelPaused) break;
            
            // Count currently active leaks
            int activeCount = GetActiveLeakCount();
            
            // Only activate if we haven't reached max active leaks
            if (activeCount < levelData.maxActiveLeaks)
            {
                // Find available (non-leaking) leaks
                List<LeakSpot> availableLeaks = new List<LeakSpot>();
                foreach (LeakSpot leak in leakSpots)
                {
                    if (leak != null && !leak.IsLeaking())
                    {
                        availableLeaks.Add(leak);
                    }
                }
                
                // Activate a random leak if available
                if (availableLeaks.Count > 0)
                {
                    LeakSpot leakToActivate = availableLeaks[Random.Range(0, availableLeaks.Count)];
                    leakToActivate.StartLeak();
                    UpdateLeakUI();
                    Debug.Log($"[FloodLevelManager] Activated leak: {leakToActivate.gameObject.name}");
                }
            }
        }
    }
    
    /// <summary>
    /// Coroutine that manages water rising based on level settings.
    /// </summary>
    private IEnumerator WaterRisingLoop(LevelData levelData)
    {
        int currentWaterLevel = 0;
        float timeBetweenLevels = 5f / levelData.waterRisingSpeed; // Adjust based on rising speed
        
        while (isLevelActive && !isLevelPaused)
        {
            yield return new WaitForSeconds(timeBetweenLevels);
            
            if (!isLevelActive || isLevelPaused) break;
            
            // Progressively increase water level
            currentWaterLevel++;
            if (floodController != null)
            {
                floodController.SetFloodLevel(currentWaterLevel);
                Debug.Log($"[FloodLevelManager] Water level increased to: {currentWaterLevel}");
            }
        }
    }
    
    /// <summary>
    /// Checks if the win condition for the current level has been met.
    /// </summary>
    private void CheckWinCondition()
    {
        LevelData currentLevel = allLevels[currentLevelIndex];
        
        bool winConditionMet = false;
        
        // Check win condition based on level data
        if (currentLevel.leaksToRepairTarget > 0)
        {
            // Win condition: Repair count
            if (leaksRepairedThisLevel >= currentLevel.leaksToRepairTarget)
            {
                winConditionMet = true;
                Debug.Log($"[FloodLevelManager] Win condition met: Repaired {leaksRepairedThisLevel}/{currentLevel.leaksToRepairTarget} leaks");
            }
        }
        else
        {
            // Win condition: Time duration
            float elapsedTime = Time.time - levelStartTime;
            if (elapsedTime >= currentLevel.levelDuration)
            {
                winConditionMet = true;
                Debug.Log($"[FloodLevelManager] Win condition met: Survived {elapsedTime:F1}/{currentLevel.levelDuration} seconds");
            }
        }
        
        // Trigger victory if condition is met
        if (winConditionMet && GameOverManager.Instance != null)
        {
            isLevelActive = false;
            GameOverManager.Instance.TriggerVictory();
        }
    }
    
    /// <summary>
    /// Called when a leak is repaired (subscribed to LeakSpot.OnLeakRepaired event).
    /// </summary>
    private void OnLeakRepaired()
    {
        if (isLevelActive)
        {
            leaksRepairedThisLevel++;
            UpdateLeakUI();
            Debug.Log($"[FloodLevelManager] Leak repaired! Total this level: {leaksRepairedThisLevel}");
        }
    }
    
    /// <summary>
    /// Gets the count of currently active leaks.
    /// </summary>
    private int GetActiveLeakCount()
    {
        int count = 0;
        foreach (LeakSpot leak in leakSpots)
        {
            if (leak != null && leak.IsLeaking())
            {
                count++;
            }
        }
        return count;
    }
    
    /// <summary>
    /// Updates the leak count UI by counting active leaks and managing visibility.
    /// </summary>
    public void UpdateLeakUI()
    {
        if (leakCountText == null)
        {
            return;
        }
        
        int activeLeakCount = GetActiveLeakCount();
        
        // Update visibility based on active leak count
        if (activeLeakCount > 0)
        {
            // Show UI when there are active leaks
            leakCountText.gameObject.SetActive(true);
            leakCountText.text = "Leaks Left: " + activeLeakCount;
        }
        else
        {
            // Hide UI when there are no active leaks
            leakCountText.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Called from UI button to proceed to the next level.
    /// Hides victory panel and starts next level (which will show level start message).
    /// </summary>
    public void NextLevel()
    {
        // Hide victory panel
        if (GameOverManager.Instance != null && GameOverManager.Instance.victoryPanel != null)
        {
            GameOverManager.Instance.victoryPanel.SetActive(false);
        }
        
        // Start next level (will show level start message and pause)
        StartLevel(currentLevelIndex + 1);
    }
}

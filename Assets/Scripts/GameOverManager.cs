using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton manager that handles game over, victory, and end game states and UI.
/// Can be triggered from various sources (drowning, too many leaks, victory, etc.)
/// </summary>
public class GameOverManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance - accessible from anywhere in the code.
    /// </summary>
    public static GameOverManager Instance { get; private set; }
    
    /// <summary>
    /// Static flag to track if the game is restarting (used to skip cutscene on restart).
    /// </summary>
    public static bool IsRestarting = false;
    
    [Header("UI References")]
    [Tooltip("The GameOverPanel GameObject that contains the game over UI.")]
    public GameObject gameOverPanel;
    
    [Tooltip("The TextMeshProUGUI component that displays why the player died.")]
    public TextMeshProUGUI reasonText;
    
    [Tooltip("The VictoryPanel GameObject that contains the victory UI (shown when level is completed).")]
    public GameObject victoryPanel;
    
    [Tooltip("The EndGamePanel GameObject that contains the end game UI (shown when all levels are completed).")]
    public GameObject endGamePanel;
    
    private void Awake()
    {
        // Singleton pattern - ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;
            // Optional: Uncomment to persist across scene loads
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If another instance already exists, destroy this one
            Destroy(gameObject);
            return;
        }
        
        // Ensure time scale is reset to normal (important for restart)
        Time.timeScale = 1f;
        
        // Ensure all panels are hidden at start
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
        
        if (endGamePanel != null)
        {
            endGamePanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Triggers the game over state with a specific reason.
    /// </summary>
    /// <param name="reason">The reason why the game ended (e.g., "Out of Oxygen!", "Too many leaks!")</param>
    public void TriggerGameOver(string reason)
    {
        // Show the game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[GameOverManager] GameOverPanel is not assigned!");
        }
        
        // Set the reason text
        if (reasonText != null)
        {
            reasonText.text = reason;
        }
        else
        {
            Debug.LogWarning("[GameOverManager] ReasonText is not assigned!");
        }
        
        // Freeze the game
        Time.timeScale = 0f;
        
        // Unlock cursor so player can click the restart button
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log($"[GameOverManager] Game Over triggered: {reason}");
    }
    
    /// <summary>
    /// Triggers the victory state (level completed).
    /// </summary>
    public void TriggerVictory()
    {
        // Show the victory panel
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[GameOverManager] VictoryPanel is not assigned!");
        }
        
        // Freeze the game
        Time.timeScale = 0f;
        
        // Unlock cursor so player can click the next level button
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log("[GameOverManager] Victory triggered!");
    }
    
    /// <summary>
    /// Triggers the end game state (all levels completed).
    /// </summary>
    public void TriggerEndGame()
    {
        // Show the end game panel
        if (endGamePanel != null)
        {
            endGamePanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[GameOverManager] EndGamePanel is not assigned!");
        }
        
        // Freeze the game
        Time.timeScale = 0f;
        
        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log("[GameOverManager] End Game triggered - All levels completed!");
    }
    
    /// <summary>
    /// Restarts the current scene.
    /// Should be called by the restart button in the UI.
    /// </summary>
    public void RestartGame()
    {
        // Set restarting flag before reloading scene
        IsRestarting = true;
        
        // Unfreeze time before reloading scene
        Time.timeScale = 1f;
        
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        
        Debug.Log("[GameOverManager] Restarting game...");
    }
}

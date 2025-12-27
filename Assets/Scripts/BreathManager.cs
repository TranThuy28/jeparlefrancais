using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the player's breath/oxygen system when swimming underwater.
/// Depletes breath while underwater and regenerates when surfaced.
/// </summary>
public class BreathManager : MonoBehaviour
{
    [Header("Breath Settings")]
    [Tooltip("Maximum breath duration in seconds (how long player can stay underwater).")]
    public float maxBreath = 10f;
    
    [Tooltip("How fast breath regenerates when surfaced (multiplier).")]
    public float breathRegenSpeed = 2f;
    
    [Header("UI References")]
    [Tooltip("The Image component that shows the breath bar fill (should be a child Image with Image Type = Filled).")]
    public Image breathFillImage;
    
    [Tooltip("The parent GameObject/Container that holds the breath UI (to hide/show the entire bar).")]
    public GameObject breathUIContainer;
    
    [Header("Character Controller Reference")]
    [Tooltip("Reference to the ManualCharacterController to check swimming state.")]
    public ManualCharacterController playerController;
    
    private float currentBreath;
    private bool isSwimming = false;
    
    void Start()
    {
        // Initialize breath to full
        currentBreath = maxBreath;
        
        // Auto-find player controller if not assigned
        if (playerController == null)
        {
            playerController = FindObjectOfType<ManualCharacterController>();
            if (playerController == null)
            {
                Debug.LogWarning("[BreathManager] ManualCharacterController not found. Please assign it manually.");
            }
        }
        
        // Validate UI references
        if (breathFillImage == null)
        {
            Debug.LogWarning("[BreathManager] Breath Fill Image not assigned. Breath bar will not update.");
        }
        else
        {
            // Ensure the image type is set to Filled (should be done in inspector, but we'll check)
            if (breathFillImage.type != Image.Type.Filled)
            {
                Debug.LogWarning("[BreathManager] Breath Fill Image Type should be set to 'Filled' in the Inspector for fillAmount to work.");
            }
        }
        
        // Hide UI container initially (player starts on surface)
        if (breathUIContainer != null)
        {
            breathUIContainer.SetActive(false);
        }
    }
    
    void Update()
    {
        // Get swimming state from player controller
        if (playerController != null)
        {
            isSwimming = playerController.IsSwimming();
        }
        
        // Update breath based on swimming state
        if (isSwimming)
        {
            // Player is underwater - deplete breath
            currentBreath -= Time.deltaTime;
            currentBreath = Mathf.Clamp(currentBreath, 0f, maxBreath);
            
            // Show UI when underwater
            if (breathUIContainer != null && !breathUIContainer.activeSelf)
            {
                breathUIContainer.SetActive(true);
            }
        }
        else
        {
            // Player is surfaced - regenerate breath
            currentBreath += Time.deltaTime * breathRegenSpeed;
            currentBreath = Mathf.Clamp(currentBreath, 0f, maxBreath);
            
            // Hide UI when breath is full and player is surfaced
            if (currentBreath >= maxBreath && breathUIContainer != null && breathUIContainer.activeSelf)
            {
                breathUIContainer.SetActive(false);
            }
        }
        
        // Update UI fill amount
        if (breathFillImage != null)
        {
            float fillAmount = currentBreath / maxBreath;
            breathFillImage.fillAmount = fillAmount;
        }
        
        // Check if player has drowned
        if (currentBreath <= 0f && isSwimming)
        {
            Drown();
        }
    }
    
    /// <summary>
    /// Called when the player runs out of breath underwater.
    /// </summary>
    private void Drown()
    {
        Debug.Log("Player Drowned!");
        
        // Trigger game over through GameOverManager
        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.TriggerGameOver("Out of Oxygen!");
        }
        else
        {
            Debug.LogError("[BreathManager] GameOverManager.Instance is null! Cannot trigger game over.");
        }
    }
    
    /// <summary>
    /// Public method to manually set swimming state (alternative to checking player controller).
    /// </summary>
    public void SetSwimmingState(bool swimming)
    {
        isSwimming = swimming;
    }
    
    /// <summary>
    /// Get current breath value (0 to maxBreath).
    /// </summary>
    public float GetCurrentBreath()
    {
        return currentBreath;
    }
    
    /// <summary>
    /// Get breath percentage (0 to 1).
    /// </summary>
    public float GetBreathPercentage()
    {
        return currentBreath / maxBreath;
    }
}


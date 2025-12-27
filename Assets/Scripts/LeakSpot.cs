using System;
using UnityEngine;

/// <summary>
/// Controls a leak spot with a particle system (water spray) that can be repaired.
/// Player must be in range (trigger zone) and press E to repair when leaking.
/// </summary>
public class LeakSpot : MonoBehaviour
{
    /// <summary>
    /// Static event that fires when any leak is repaired.
    /// Subscribe to this event in FloodLevelManager to update UI.
    /// </summary>
    public static Action OnLeakRepaired;
    
    [Header("Leak Settings")]
    [Tooltip("The particle system that represents the water spray. Should be a child of this GameObject.")]
    public ParticleSystem waterSpray;
    
    [Tooltip("Whether the leak is currently active.")]
    private bool isLeaking = false;
    
    [Tooltip("Whether the player is currently in range to repair.")]
    private bool playerInRange = false;
    
    [Header("Visual Indicator")]
    [Tooltip("The renderer component to change color when leaking. If not set, will try to find one automatically.")]
    public Renderer leakRenderer;
    
    [Tooltip("Color to show when leaking.")]
    public Color leakingColor = Color.red;
    
    private Color originalColor;
    private Material materialInstance;
    
    private void Awake()
    {
        // Check if there's a collider set as trigger
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            col = GetComponentInChildren<Collider>();
        }
        if (col == null)
        {
            Debug.LogError($"[LeakSpot] No Collider found on {gameObject.name} or its children. A Collider component set to 'Is Trigger' is required for trigger detection.");
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning($"[LeakSpot] Collider on {gameObject.name} is not set to 'Is Trigger'. Trigger detection will not work. Please enable 'Is Trigger' on the Collider.");
        }
        
        // Find the particle system if not assigned
        if (waterSpray == null)
        {
            waterSpray = GetComponentInChildren<ParticleSystem>();
            if (waterSpray == null)
            {
                Debug.LogWarning($"[LeakSpot] No ParticleSystem found in children of {gameObject.name}. Please assign one manually.");
            }
        }
        
        // Find the renderer if not assigned
        if (leakRenderer == null)
        {
            leakRenderer = GetComponent<Renderer>();
            if (leakRenderer == null)
            {
                leakRenderer = GetComponentInChildren<Renderer>();
            }
        }
        
        // Initialize material and store original color
        if (leakRenderer != null && leakRenderer.material != null)
        {
            // Create a material instance to avoid modifying the shared material
            materialInstance = new Material(leakRenderer.material);
            leakRenderer.material = materialInstance;
            originalColor = materialInstance.color;
        }
        
        // Make sure particle system starts off
        if (waterSpray != null)
        {
            waterSpray.Stop();
            var emission = waterSpray.emission;
            emission.enabled = false;
        }
    }
    
    /// <summary>
    /// Start the leak - turns on the particle system and changes visual indicator to red.
    /// </summary>
    [ContextMenu("⚡ Kích hoạt rò rỉ (Test)")]
    public void StartLeak()
    {
        isLeaking = true;
        
        // Enable and play particle system
        if (waterSpray != null)
        {
            var emission = waterSpray.emission;
            emission.enabled = true;
            waterSpray.Play();
        }
        
        // Change visual indicator to red
        UpdateVisualIndicator(true);
        
        Debug.Log($"[LeakSpot] Leak started on {gameObject.name}");
    }
    
    /// <summary>
    /// Repair the leak - turns off the particle system and restores original color.
    /// </summary>
    public void Repair()
    {
        isLeaking = false;
        
        // Stop particle system
        if (waterSpray != null)
        {
            waterSpray.Stop();
            var emission = waterSpray.emission;
            emission.enabled = false;
        }
        
        // Restore original visual appearance
        UpdateVisualIndicator(false);
        
        // Fire event to notify listeners (like FloodLevelManager) that a leak was repaired
        OnLeakRepaired?.Invoke();
        
        Debug.Log("Repaired!");
    }
    
    /// <summary>
    /// Stops the leak without triggering repair events (used for resetting between levels).
    /// </summary>
    public void StopLeak()
    {
        isLeaking = false;
        
        // Stop particle system
        if (waterSpray != null)
        {
            waterSpray.Stop();
            var emission = waterSpray.emission;
            emission.enabled = false;
        }
        
        // Restore original visual appearance
        UpdateVisualIndicator(false);
    }
    
    /// <summary>
    /// Public method to check if this leak is currently active.
    /// Used by FloodLevelManager to count active leaks.
    /// </summary>
    public bool IsLeaking()
    {
        return isLeaking;
    }
    
    /// <summary>
    /// Updates the visual indicator based on leak status.
    /// </summary>
    private void UpdateVisualIndicator(bool leaking)
    {
        if (materialInstance != null)
        {
            materialInstance.color = leaking ? leakingColor : originalColor;
        }
    }
    
    /// <summary>
    /// Update method to check for E key input when player is in range.
    /// </summary>
    private void Update()
    {
        // Check if player is in range, leak is active, and E key is pressed
        if (playerInRange && isLeaking && Input.GetKeyDown(KeyCode.E))
        {
            Repair();
        }
    }
    
    /// <summary>
    /// Called when a collider enters the trigger zone.
    /// Detects if the player is in range.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (isLeaking)
            {
                Debug.Log("[LeakSpot] Player entered range. Press E to repair.");
            }
        }
    }
    
    /// <summary>
    /// Called when a collider exits the trigger zone.
    /// Removes player from range.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
    
    /// <summary>
    /// Displays GUI message when player is in range and leak is active.
    /// </summary>
    private void OnGUI()
    {
        if (playerInRange && isLeaking)
        {
            // Display message at center of screen
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2, 200, 50), 
                     "Press E to Repair");
        }
    }
    
    /// <summary>
    /// Cleanup material instance on destroy.
    /// </summary>
    private void OnDestroy()
    {
        if (materialInstance != null)
        {
            Destroy(materialInstance);
        }
    }
}


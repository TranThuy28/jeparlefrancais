using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the level progression sequence, controlling flood levels and leak activation.
/// </summary>
public class FloodLevelManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the FloodController that manages water levels.")]
    public FloodController floodController;
    
    [Header("Leak Management")]
    [Tooltip("List of all LeakSpot components found in the scene.")]
    private List<LeakSpot> leakSpots = new List<LeakSpot>();
    
    /// <summary>
    /// Initializes the manager and starts the gameplay loop.
    /// Call this method from your Cutscene Timeline Signal or Game Manager when gameplay should begin.
    /// </summary>
    public void StartGameplayLoop()
    {
        // Find FloodController if not assigned
        if (floodController == null)
        {
            floodController = FindObjectOfType<FloodController>();
            if (floodController == null)
            {
                Debug.LogWarning("[FloodLevelManager] FloodController not found. Please assign it manually.");
            }
        }
        
        // Find all GameObjects with tag 'Leak' and store their LeakSpot components
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
        
        // Start the level sequence
        StartCoroutine(StartLevelSequence());
    }
    
    /// <summary>
    /// Coroutine that manages the level progression sequence.
    /// </summary>
    private IEnumerator StartLevelSequence()
    {
        Debug.Log("[FloodLevelManager] Starting level sequence...");
        
        // Wait 2 seconds
        yield return new WaitForSeconds(2f);
        Debug.Log("[FloodLevelManager] 2 seconds elapsed. Moving to Level 1...");
        
        // Tell FloodController to go to Level 1
        if (floodController != null)
        {
            floodController.SetFloodLevel(1);
            Debug.Log("[FloodLevelManager] Flood level set to Level 1.");
        }
        else
        {
            Debug.LogWarning("[FloodLevelManager] Cannot set flood level - FloodController is null.");
        }
        
        // Wait 5 seconds
        yield return new WaitForSeconds(5f);
        Debug.Log("[FloodLevelManager] 5 seconds elapsed. Activating first leak...");
        
        // Pick 1 random LeakSpot and call StartLeak()
        if (leakSpots.Count > 0)
        {
            LeakSpot randomLeak = leakSpots[Random.Range(0, leakSpots.Count)];
            randomLeak.StartLeak();
            Debug.Log($"[FloodLevelManager] Leak activated on '{randomLeak.gameObject.name}'.");
        }
        else
        {
            Debug.LogWarning("[FloodLevelManager] No LeakSpots available to activate.");
        }
        
        // Wait 10 seconds
        yield return new WaitForSeconds(10f);
        Debug.Log("[FloodLevelManager] 10 seconds elapsed. Moving to Level 2...");
        
        // Tell FloodController to go to Level 2
        if (floodController != null)
        {
            floodController.SetFloodLevel(2);
            Debug.Log("[FloodLevelManager] Flood level set to Level 2.");
        }
        else
        {
            Debug.LogWarning("[FloodLevelManager] Cannot set flood level - FloodController is null.");
        }
        
        // Pick 2 random LeakSpots (without duplicates) and leak them
        if (leakSpots.Count > 0)
        {
            List<LeakSpot> availableLeaks = new List<LeakSpot>(leakSpots);
            int leaksToActivate = Mathf.Min(2, availableLeaks.Count);
            
            for (int i = 0; i < leaksToActivate; i++)
            {
                int randomIndex = Random.Range(0, availableLeaks.Count);
                LeakSpot selectedLeak = availableLeaks[randomIndex];
                selectedLeak.StartLeak();
                availableLeaks.RemoveAt(randomIndex); // Remove to avoid duplicates
                Debug.Log($"[FloodLevelManager] Leak {i + 1} activated on '{selectedLeak.gameObject.name}'.");
            }
            
            Debug.Log($"[FloodLevelManager] Level 2 complete - {leaksToActivate} leak(s) activated.");
        }
        else
        {
            Debug.LogWarning("[FloodLevelManager] No LeakSpots available to activate for Level 2.");
        }
        
        Debug.Log("[FloodLevelManager] Level sequence completed.");
    }
}


using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineCameraManager : MonoBehaviour
{
    [Header("Camera References")]
    [Tooltip("Main gameplay camera")]
    public Camera mainCamera; // Main Camera GameObject
    
    [Tooltip("Normal Camera cho cutscene - sẽ được animate trong Timeline")]
    public Camera cutsceneCamera;
    
    [Header("Timeline Settings")]
    public PlayableDirector timelineDirector;
    
    [Header("Gameplay Camera Scripts")]
    [Tooltip("Các script điều khiển camera gameplay cần tắt trong cutscene")]
    public MonoBehaviour[] gameplayCameraScripts;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    private bool isInCutscene = false;
    
    void Start()
    {
        SetupCameraSystem();
        
        if (timelineDirector)
        {
            timelineDirector.played += OnCutsceneStart;
            timelineDirector.stopped += OnCutsceneEnd;
        }
    }
    
    void SetupCameraSystem()
    {
        // Auto-find main camera nếu chưa assign
        if (!mainCamera)
        {
            mainCamera = Camera.main;
        }
        
        // Setup cutscene camera
        if (cutsceneCamera)
        {
            cutsceneCamera.enabled = false; // Tắt cutscene camera ban đầu
            Debug.Log("Cutscene camera setup complete");
        }
        
        Debug.Log("Camera system setup complete (No Cinemachine version)");
    }
    
    void OnCutsceneStart(PlayableDirector director)
    {
        if (director == timelineDirector)
        {
            Debug.Log("=== CUTSCENE CAMERA START ===");
            SwitchToCutsceneCamera();
        }
    }
    
    void OnCutsceneEnd(PlayableDirector director)
    {
        if (director == timelineDirector)
        {
            Debug.Log("=== CUTSCENE CAMERA END ===");
            SwitchToGameplayCamera();
        }
    }
    
    public void SwitchToCutsceneCamera()
    {
        isInCutscene = true;
        
        Debug.Log("Switching to cutscene camera...");
        
        // Tắt gameplay camera scripts trước
        DisableGameplayCameraScripts();
        
        // Switch cameras
        if (mainCamera && cutsceneCamera)
        {
            mainCamera.enabled = false;
            cutsceneCamera.enabled = true;
            
            Debug.Log("Switched to cutscene camera");
        }
        else
        {
            Debug.LogWarning("Main camera or cutscene camera is not assigned!");
        }
    }
    
    public void SwitchToGameplayCamera()
    {
        isInCutscene = false;
        
        Debug.Log("Switching back to gameplay camera...");
        
        // Switch cameras
        if (mainCamera && cutsceneCamera)
        {
            cutsceneCamera.enabled = false;
            mainCamera.enabled = true;
            
            Debug.Log("Switched back to gameplay camera");
        }
        
        // Bật lại gameplay camera scripts
        EnableGameplayCameraScripts();
    }
    
    void DisableGameplayCameraScripts()
    {
        // Tắt các script điều khiển camera gameplay
        if (gameplayCameraScripts != null)
        {
            foreach (MonoBehaviour script in gameplayCameraScripts)
            {
                if (script != null)
                {
                    script.enabled = false;
                    Debug.Log($"Disabled gameplay camera script: {script.GetType().Name}");
                }
            }
        }
        
        Debug.Log("Disabled gameplay camera scripts");
    }
    
    void EnableGameplayCameraScripts()
    {
        // Bật lại các script điều khiển camera gameplay
        if (gameplayCameraScripts != null)
        {
            foreach (MonoBehaviour script in gameplayCameraScripts)
            {
                if (script != null)
                {
                    script.enabled = true;
                    Debug.Log($"Enabled gameplay camera script: {script.GetType().Name}");
                }
            }
        }
        
        Debug.Log("Enabled gameplay camera scripts");
    }
    
    // Method để bind cutscene camera vào Timeline
    public void BindCutsceneCameraToTimeline()
    {
        if (timelineDirector && cutsceneCamera)
        {
            foreach (PlayableBinding binding in timelineDirector.playableAsset.outputs)
            {
                Debug.Log($"Timeline binding: {binding.streamName}");
                
                if (binding.streamName.Contains("Animation") || 
                    binding.streamName.Contains("Camera") ||
                    binding.streamName == "Camera Track")
                {
                    // Bind cutscene camera GameObject vào Animation Track
                    timelineDirector.SetGenericBinding(binding.sourceObject, cutsceneCamera.gameObject);
                    Debug.Log($"Bound cutscene camera to Timeline track: {binding.streamName}");
                }
            }
        }
    }
    
    // Public methods để control thủ công
    public void ForceEnterCutsceneMode()
    {
        SwitchToCutsceneCamera();
    }
    
    public void ForceExitCutsceneMode()
    {
        SwitchToGameplayCamera();
    }
    
    // Method để set cutscene camera position manually
    public void SetCutsceneCameraPosition(Vector3 position, Vector3 rotation)
    {
        if (cutsceneCamera)
        {
            cutsceneCamera.transform.position = position;
            cutsceneCamera.transform.eulerAngles = rotation;
            Debug.Log($"Set cutscene camera to position: {position}, rotation: {rotation}");
        }
    }
    
    public void SetCutsceneCameraTarget(Transform target)
    {
        if (cutsceneCamera && target)
        {
            cutsceneCamera.transform.LookAt(target);
            Debug.Log($"Cutscene camera looking at: {target.name}");
        }
    }
    
    // Debug methods
    void Update()
    {
        // Debug controls
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (isInCutscene)
                ForceExitCutsceneMode();
            else
                ForceEnterCutsceneMode();
        }
        
        if (Input.GetKeyDown(KeyCode.B))
        {
            BindCutsceneCameraToTimeline();
        }
        
        if (showDebugInfo)
        {
            DebugCameraState();
        }
    }
    
    void DebugCameraState()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("=== CAMERA DEBUG INFO ===");
            Debug.Log($"Is in cutscene: {isInCutscene}");
            Debug.Log($"Main camera enabled: {mainCamera?.enabled}");
            Debug.Log($"Cutscene camera enabled: {cutsceneCamera?.enabled}");
            Debug.Log($"Timeline state: {timelineDirector?.state}");
            
            if (gameplayCameraScripts != null)
            {
                Debug.Log($"Gameplay camera scripts count: {gameplayCameraScripts.Length}");
                foreach (var script in gameplayCameraScripts)
                {
                    if (script != null)
                        Debug.Log($"  - {script.GetType().Name}: {(script.enabled ? "Enabled" : "Disabled")}");
                }
            }
        }
    }
    
    void OnDrawGizmos()
    {
        if (showDebugInfo)
        {
            // Vẽ main camera position
            if (mainCamera)
            {
                Gizmos.color = mainCamera.enabled ? Color.green : Color.gray;
                Gizmos.DrawWireCube(mainCamera.transform.position, Vector3.one * 0.5f);
                
                // Vẽ camera direction
                Gizmos.color = Color.green;
                Gizmos.DrawLine(mainCamera.transform.position, 
                               mainCamera.transform.position + mainCamera.transform.forward * 2f);
            }
            
            // Vẽ cutscene camera position
            if (cutsceneCamera)
            {
                Gizmos.color = isInCutscene ? Color.red : Color.yellow;
                Gizmos.DrawWireCube(cutsceneCamera.transform.position, Vector3.one * 0.5f);
                
                // Vẽ camera direction
                Gizmos.color = isInCutscene ? Color.red : Color.yellow;
                Gizmos.DrawLine(cutsceneCamera.transform.position, 
                               cutsceneCamera.transform.position + cutsceneCamera.transform.forward * 2f);
            }
        }
    }
    
    void OnGUI()
    {
        if (showDebugInfo && Application.isPlaying)
        {
            GUILayout.BeginArea(new Rect(10, 10, 250, 150));
            
            GUILayout.Label("=== Camera Manager Debug ===");
            GUILayout.Label($"Current Mode: {(isInCutscene ? "CUTSCENE" : "GAMEPLAY")}");
            
            if (GUILayout.Button("Toggle Camera (C)"))
            {
                if (isInCutscene)
                    ForceExitCutsceneMode();
                else
                    ForceEnterCutsceneMode();
            }
            
            if (GUILayout.Button("Bind Camera to Timeline (B)"))
            {
                BindCutsceneCameraToTimeline();
            }
            
            GUILayout.Label($"Main Camera: {(mainCamera?.enabled == true ? "ON" : "OFF")}");
            GUILayout.Label($"Cutscene Camera: {(cutsceneCamera?.enabled == true ? "ON" : "OFF")}");
            
            GUILayout.EndArea();
        }
    }
}
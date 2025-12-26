using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections;
using Unity.Cinemachine;

// Script chính quản lý chuyển tiếp với cutscene tự động bắt đầu
public class GameplayToCutsceneManager2 : MonoBehaviour
{
    [Header("Cutscene Settings")]
    public PlayableDirector timelineDirector; // Timeline Director
    public TimelineAsset cutsceneTimeline; // Timeline Asset
    
    [Header("Auto Start Settings")]
    [Tooltip("Tự động bắt đầu cutscene khi game khởi động")]
    public bool autoStartCutscene = true;
    [Tooltip("Thời gian delay trước khi bắt đầu cutscene (giây)")]
    public float startDelay = 0.5f;
    
    [Header("Trigger Settings")]
    public Transform triggerPoint; // Vị trí trigger (gần nhà)
    public float triggerDistance = 1f; // Khoảng cách kích hoạt
    public LayerMask playerLayer = 1; // Layer của player
    
    [Header("Character References")]
    public GameObject playerCharacter; // Nhân vật chính
    public CharacterController characterController; // Character Controller
    public MonoBehaviour[] gameplayScripts; // Các script gameplay cần tắt

    [Header("Position Management")]
    [Tooltip("Vị trí cố định mà nhân vật sẽ đứng sau cutscene")]
    public Transform gameplayStartPosition; // Vị trí bắt đầu gameplay
    [Tooltip("Có sử dụng vị trí cố định hay không")]
    public bool useFixedGameplayPosition = true;
    [Tooltip("Freeze character position during cutscene to prevent falling")]
    public bool freezeCharacterDuringCutscene = true;
    
    [Header("Camera Settings")]
    public CinemachineCamera gameplayCamera; // Camera gameplay
    public CinemachineCamera cutsceneCamera; // Camera cutscene (optional)
    
    [Header("UI References")]
    public Canvas gameplayUI; // UI gameplay
    public Canvas cutsceneUI; // UI cutscene (optional)
    
    [Header("Debug")]
    public bool showDebugGizmos = true;

    [Header("Audio")]
    public AudioSource mainAudio;
    [Header("Flood Manager")]
    public FloodLevelManager floodManager; 
    
    private bool cutsceneTriggered = false;
    private bool isInCutscene = false;
    private bool gameStarted = false; // Flag để track game đã bắt đầu chưa
    
    // Position management variables
    private Vector3 gameplayPosition; // Vị trí gameplay thực tế
    private Quaternion gameplayRotation; // Rotation gameplay thực tế
    private Vector3 frozenPosition; // Vị trí được freeze trong cutscene
    private Quaternion frozenRotation; // Rotation được freeze trong cutscene
    private bool isPositionFrozen = false;
    
    void Start()
    {
        Debug.Log("=== GAME STARTING ===");
        
        // Đảm bảo timeline director có timeline
        if (timelineDirector && cutsceneTimeline)
        {
            timelineDirector.playableAsset = cutsceneTimeline;
            timelineDirector.stopped += OnCutsceneEnded;
        }

        // Bind character vào timeline
        BindCharacterToTimeline();
        
        // Set up gameplay position ngay từ đầu
        SetupInitialGameplayPosition();
        
        // Tắt gameplay systems ngay từ đầu nếu sẽ auto start cutscene
        if (autoStartCutscene)
        {
            PrepareForCutscene();
            StartCoroutine(AutoStartCutscene());
        }
        else
        {
            // Nếu không auto start, lưu vị trí hiện tại làm gameplay position
            SaveGameplayPosition();
            gameStarted = true;
        }
    }
    
    void SetupInitialGameplayPosition()
    {
        // Luôn ưu tiên sử dụng gameplayStartPosition nếu có
        if (gameplayStartPosition)
        {
            gameplayPosition = gameplayStartPosition.position;
            gameplayRotation = gameplayStartPosition.rotation;
            useFixedGameplayPosition = true;
            Debug.Log($"Initial gameplay position set from gameplayStartPosition: {gameplayPosition}");
        }
        else if (playerCharacter)
        {
            // Fallback: sử dụng vị trí hiện tại của character
            gameplayPosition = playerCharacter.transform.position;
            gameplayRotation = playerCharacter.transform.rotation;
            Debug.Log($"Initial gameplay position set from current character position: {gameplayPosition}");
        }
    }
    
    void PrepareForCutscene()
    {
        Debug.Log("Preparing for cutscene - disabling gameplay systems...");
        
        // Ẩn gameplay UI ngay từ đầu
        if (gameplayUI)
        {
            gameplayUI.gameObject.SetActive(false);
        }

        // Disable gameplay camera
        if (gameplayCamera)
        {
            gameplayCamera.Priority = 50;
            mainAudio.enabled = false;
        }
        
        // Enable cutscene camera
        if (cutsceneCamera)
        {
            cutsceneCamera.Priority = 100;
        }
    }
    
    IEnumerator AutoStartCutscene()
    {
        Debug.Log($"Auto-starting cutscene in {startDelay} seconds...");
        yield return new WaitForSeconds(startDelay);
        
        if (!gameStarted) // Chỉ chạy nếu chưa bắt đầu game
        {
            TriggerCutscene();
        }
    }
    
    void SaveGameplayPosition()
    {
        if (useFixedGameplayPosition && gameplayStartPosition)
        {
            // Sử dụng vị trí cố định
            gameplayPosition = gameplayStartPosition.position;
            gameplayRotation = gameplayStartPosition.rotation;
            Debug.Log($"Using fixed gameplay position: {gameplayPosition}");
        }
        else if (playerCharacter)
        {
            // Sử dụng vị trí hiện tại của nhân vật
            gameplayPosition = playerCharacter.transform.position;
            gameplayRotation = playerCharacter.transform.rotation;
            Debug.Log($"Using current character position as gameplay position: {gameplayPosition}");
        }
    }
    
    void Update()
    {
        // Chỉ check trigger nếu game đã bắt đầu và không trong cutscene
        if (gameStarted && !cutsceneTriggered && !isInCutscene && playerCharacter && triggerPoint)
        {
            CheckTriggerDistance();
        }
        
        // Freeze character position during cutscene
        if (isInCutscene && isPositionFrozen && freezeCharacterDuringCutscene)
        {
            playerCharacter.transform.position = frozenPosition;
            playerCharacter.transform.rotation = frozenRotation;
        }
        
        // Debug input để test (chỉ hoạt động sau khi game đã bắt đầu)
        if (gameStarted)
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                TriggerCutscene();
            }
            
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetToGameplay();
            }
        }
    }
    
    void CheckTriggerDistance()
    {
        float distance = Vector3.Distance(playerCharacter.transform.position, triggerPoint.position);
        
        if (distance <= triggerDistance)
        {
            Debug.Log($"Player reached trigger distance: {distance}");
            TriggerCutscene();
        }
    }
    
    public void TriggerCutscene()
    {
        if (cutsceneTriggered) return;
        Debug.Log("=== STARTING CUTSCENE ===");
        cutsceneTriggered = true;
        isInCutscene = true;

        // Lưu vị trí gameplay hiện tại nếu chưa có hoặc không dùng fixed position
        if (!useFixedGameplayPosition || (!gameStarted && autoStartCutscene))
        {
            SaveGameplayPosition();
        }

        // Freeze vị trí nếu cần
        if (playerCharacter && freezeCharacterDuringCutscene)
        {
            frozenPosition = playerCharacter.transform.position;
            frozenRotation = playerCharacter.transform.rotation;
            isPositionFrozen = true;
            Debug.Log($"Frozen character at position: {frozenPosition}");
        }
        
        // SYNC ROTATION TRƯỚC KHI BẮT ĐẦU TIMELINE
        SyncCharacterRotationForCutscene();
        
        // Tắt gameplay (chỉ nếu game đã bắt đầu)
        if (gameStarted)
        {
            DisableGameplay();
        }
        else
        {
            // Nếu là lần đầu (auto start), chỉ disable các script cần thiết
            DisableGameplayScripts();
        }
        
        // Bắt đầu cutscene
        StartCoroutine(StartCutsceneDelayed());
    }
    
    void DisableGameplayScripts()
    {
        Debug.Log("Disabling gameplay scripts for cutscene...");
        
        // Tắt character controller và scripts
        foreach (MonoBehaviour script in gameplayScripts)
        {
            if (script != null && script is ManualCharacterController manualController)
            {
                Debug.Log("Disabling gravity on ManualCharacterController");
                manualController.DisableGravity();
            }
        }
        
        foreach (MonoBehaviour script in gameplayScripts)
        {
            if (script != null)
            {
                Debug.Log($"Disabling script: {script.GetType().Name}");
                script.enabled = false;
            }
        }
        
        if (characterController)
        {
            if (freezeCharacterDuringCutscene)
            {
                Debug.Log("Keeping CharacterController enabled but frozen");
            }
            else
            {
                Debug.Log("Disabling CharacterController");
                characterController.enabled = false;
            }
        }
    }

    void DisableGameplay()
    {
        Debug.Log("Disabling gameplay systems...");
        
        // Disable scripts
        DisableGameplayScripts();
        
        // Ẩn gameplay UI
        if (gameplayUI)
        {
            gameplayUI.gameObject.SetActive(false);
        }

        // Chuyển camera
        if (gameplayCamera)
        {
            gameplayCamera.Priority = 50;
        }

        if (cutsceneCamera)
        {
            cutsceneCamera.Priority = 100;
        }
        Debug.Log("Gameplay disabled successfully");
    }
    
    IEnumerator StartCutsceneDelayed()
    {
        yield return new WaitForEndOfFrame();
        StartCutscene();
    }
    
    void StartCutscene()
    {
        Debug.Log("Starting Timeline...");
        if (timelineDirector)
        {
            timelineDirector.Play();
            Debug.Log($"Timeline duration: {timelineDirector.duration}");
        }
        
        // Hiện cutscene UI nếu có
        if (cutsceneUI)
        {
            cutsceneUI.gameObject.SetActive(true);
        }
    }
    
    void BindCharacterToTimeline()
    {
        if (timelineDirector && playerCharacter)
        {
            // METHOD 1: Bind Animator thay vì GameObject
            Animator characterAnimator = playerCharacter.GetComponent<Animator>();
            
            if (characterAnimator != null)
            {
                foreach (PlayableBinding binding in timelineDirector.playableAsset.outputs)
                {
                    Debug.Log($"Found binding: {binding.streamName}");
                    
                    if (binding.streamName.Contains("RAPHAEL") || 
                        binding.streamName.Contains("Village_head") ||
                        binding.streamName.Contains("Character") || 
                        binding.streamName.Contains("Player"))
                    {
                        // BIND ANIMATOR thay vì GameObject
                        timelineDirector.SetGenericBinding(binding.sourceObject, characterAnimator);
                        Debug.Log($"Bound ANIMATOR to Timeline track: {binding.streamName}");
                    }
                }
            }
            else
            {
                // Fallback: Bind GameObject
                Debug.LogWarning("No Animator found, binding GameObject instead");
                foreach (PlayableBinding binding in timelineDirector.playableAsset.outputs)
                {
                    if (binding.streamName.Contains("RAPHAEL") || 
                        binding.streamName.Contains("Village_head") ||
                        binding.streamName.Contains("Character") || 
                        binding.streamName.Contains("Player"))
                    {
                        timelineDirector.SetGenericBinding(binding.sourceObject, playerCharacter);
                    }
                }
            }
        }
    }
    
    void SyncCharacterRotationForCutscene()
    {
        if (playerCharacter)
        {
            Vector3 currentRotation = playerCharacter.transform.eulerAngles;
            Debug.Log($"Character hierarchy rotation: {currentRotation}");
            
            TimelineRotationSync rotationSync = playerCharacter.GetComponent<TimelineRotationSync>();
            if (rotationSync == null)
            {
                rotationSync = playerCharacter.AddComponent<TimelineRotationSync>();
            }
            
            rotationSync.timelineDirector = timelineDirector;
            rotationSync.targetCharacter = playerCharacter;
            rotationSync.syncRotationWithHierarchy = true;
        }
    }
    
    void OnCutsceneEnded(PlayableDirector director)
    {
        if (director == timelineDirector)
        {
            Debug.Log("=== CUTSCENE ENDED ===");

            EndCutscene();
            if (floodManager != null) 
            {
                Debug.Log("Gọi FloodManager bắt đầu dâng nước!");
                floodManager.StartGameplayLoop();
            }
            else
            {
                Debug.LogError("QUÊN KÉO FLOOD MANAGER VÀO RỒI BẠN ƠI!");
            }
//
        }
    }
    
    void EndCutscene()
    {
        Debug.Log("Ending cutscene, returning to gameplay...");
        isInCutscene = false;
        isPositionFrozen = false;
        
        // Đánh dấu game đã bắt đầu
        gameStarted = true;
        
        // Bật lại gameplay
        StartCoroutine(EnableGameplayDelayed());
    }
    
    IEnumerator EnableGameplayDelayed()
    {
        yield return new WaitForSeconds(0.1f);
        EnableGameplay();
    }
    
    void EnableGameplay()
    {
        Debug.Log("=== ENABLING GAMEPLAY ===");
        
        // QUAN TRỌNG: Đặt nhân vật về vị trí gameplay chính xác
        RestoreGameplayPosition();
        
        // Bật lại character controller
        if (characterController)
        {
            Debug.Log("Enabling CharacterController");
            characterController.enabled = true;
        }
        
        // Bật lại Rigidbody settings
        Rigidbody rb = playerCharacter.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.Log("Enabling Rigidbody gravity");
            rb.useGravity = true;
            rb.isKinematic = false;
        }
        
        // Bật lại các script gameplay
        foreach (MonoBehaviour script in gameplayScripts)
        {
            if (script != null)
            {
                Debug.Log($"Enabling script: {script.GetType().Name}");
                script.enabled = true;
            }
        }
        
        // Hiện lại gameplay UI
        if (gameplayUI)
        {
            gameplayUI.gameObject.SetActive(true);
        }

        // Chuyển lại camera
        if (cutsceneCamera)
        {
            cutsceneCamera.Priority = 50;
        }

        if (gameplayCamera)
        {
            gameplayCamera.Priority = 100;
            mainAudio.enabled = true;
        }

        // Ẩn cutscene UI
        if (cutsceneUI)
        {
            cutsceneUI.gameObject.SetActive(false);
        }
        
        Debug.Log($"Character positioned at gameplay location: {playerCharacter.transform.position}");
        Debug.Log("Gameplay enabled successfully!");
    }
    
    void RestoreGameplayPosition()
    {
        if (playerCharacter)
        {
            // Tắt character controller tạm thời để có thể set position
            bool wasControllerEnabled = characterController && characterController.enabled;
            if (wasControllerEnabled)
            {
                characterController.enabled = false;
            }
            
            // Đặt nhân vật về vị trí gameplay
            playerCharacter.transform.position = gameplayPosition;
            playerCharacter.transform.rotation = gameplayRotation;
            
            Debug.Log($"Restored character to gameplay position: {gameplayPosition}");
            
            // Kiểm tra xem có cần điều chỉnh Y position không (để tránh rơi vào đất)
            if (Physics.Raycast(gameplayPosition + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 5f))
            {
                Vector3 adjustedPosition = new Vector3(gameplayPosition.x, hit.point.y + 0.1f, gameplayPosition.z);
                playerCharacter.transform.position = adjustedPosition;
                Debug.Log($"Adjusted Y position to ground: {adjustedPosition}");
            }
            
            // Bật lại character controller nếu cần
            if (wasControllerEnabled)
            {
                characterController.enabled = true;
            }
        }
    }
    
    public void ResetToGameplay()
    {
        Debug.Log("=== MANUAL RESET ===");
        
        if (timelineDirector.state == PlayState.Playing)
        {
            timelineDirector.Stop();
        }
        
        cutsceneTriggered = false;
        isInCutscene = false;
        gameStarted = true; // Đảm bảo game được đánh dấu là đã bắt đầu
        
        EnableGameplay();
    }
    
    // Method để set vị trí gameplay mới (hữu ích khi cần thay đổi spawn point)
    public void SetGameplayPosition(Vector3 position, Quaternion rotation)
    {
        gameplayPosition = position;
        gameplayRotation = rotation;
        Debug.Log($"New gameplay position set: {position}");
    }
    
    public void SetGameplayPosition(Transform transform)
    {
        if (transform)
        {
            SetGameplayPosition(transform.position, transform.rotation);
        }
    }
    
    // Hàm public để trigger manual
    public void ManualTriggerCutscene()
    {
        TriggerCutscene();
    }
    
    // Hàm để skip cutscene (hữu ích cho testing)
    public void SkipCutscene()
    {
        if (isInCutscene && timelineDirector.state == PlayState.Playing)
        {
            timelineDirector.Stop();
        }
    }
    
    void OnDrawGizmos()
    {
        if (showDebugGizmos)
        {
            // Vẽ trigger point
            if (triggerPoint)
            {
                Gizmos.color = cutsceneTriggered ? Color.red : Color.yellow;
                Gizmos.DrawWireSphere(triggerPoint.position, triggerDistance);
                
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(triggerPoint.position, Vector3.one * 0.5f);
            }
            
            // Vẽ gameplay position
            if (gameplayStartPosition)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(gameplayStartPosition.position, Vector3.one * 0.8f);
                Gizmos.DrawLine(gameplayStartPosition.position, gameplayStartPosition.position + gameplayStartPosition.forward * 2f);
            }
            else if (playerCharacter && Application.isPlaying)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(gameplayPosition, Vector3.one * 0.6f);
            }
            
            // Vẽ text để hiển thị trạng thái
            if (Application.isPlaying)
            {
#if UNITY_EDITOR
                UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
                    $"Game Started: {gameStarted}\nIn Cutscene: {isInCutscene}\nTriggered: {cutsceneTriggered}");
#endif
            }
        }
    }
}
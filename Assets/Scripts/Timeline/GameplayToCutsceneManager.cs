using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections;

// Script chính quản lý chuyển tiếp
public class GameplayToCutsceneManager : MonoBehaviour
{
    [Header("Cutscene Settings")]
    public PlayableDirector timelineDirector; // Timeline Director
    public TimelineAsset cutsceneTimeline; // Timeline Asset
    
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
    [Header("Character Position Fix")]
    [Tooltip("Freeze character position during cutscene to prevent falling")]
    public bool freezeCharacterDuringCutscene = true;
    
    [Header("Camera Settings")]
    public Camera gameplayCamera; // Camera gameplay
    public Camera cutsceneCamera; // Camera cutscene (optional)
    
    [Header("UI References")]
    public Canvas gameplayUI; // UI gameplay
    public Canvas cutsceneUI; // UI cutscene (optional)
    
    [Header("Debug")]
    public bool showDebugGizmos = true;
    
    private bool cutsceneTriggered = false;
    private bool isInCutscene = false;
    // Để freeze position
    private Vector3 frozenPosition;
    private Quaternion frozenRotation;
    private bool isPositionFrozen = false;
    private Vector3 characterOriginalPosition;
    private Quaternion characterOriginalRotation;
    void Start()
    {
        // Đảm bảo timeline director có timeline
        if (timelineDirector && cutsceneTimeline)
        {
            timelineDirector.playableAsset = cutsceneTimeline;
            timelineDirector.stopped += OnCutsceneEnded;
        }

        // Bind character vào timeline
        BindCharacterToTimeline();
        // Lưu vị trí ban đầu
        if (playerCharacter)
        {
            characterOriginalPosition = playerCharacter.transform.position;
            characterOriginalRotation = playerCharacter.transform.rotation;
        }
    }

    
    
    void Update()
    {
        if (!cutsceneTriggered && !isInCutscene && playerCharacter && triggerPoint)
        {
            CheckTriggerDistance();
        }
        // FIX: Freeze character position during cutscene
        if (isInCutscene && isPositionFrozen && freezeCharacterDuringCutscene)
        {
            playerCharacter.transform.position = frozenPosition;
            playerCharacter.transform.rotation = frozenRotation;
        }
        // Debug input để test
        if (Input.GetKeyDown(KeyCode.T))
        {
            TriggerCutscene();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetToGameplay();
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

        if (playerCharacter && freezeCharacterDuringCutscene)
        {
            frozenPosition = playerCharacter.transform.position;
            frozenRotation = playerCharacter.transform.rotation;
            isPositionFrozen = true;
            Debug.Log($"Frozen character at position: {frozenPosition}");
        }
        // SYNC ROTATION TRƯỚC KHI BẮT ĐẦU TIMELINE
        SyncCharacterRotationForCutscene();
        // Tắt gameplay
        DisableGameplay();
        
        // Bắt đầu cutscene
        StartCoroutine(StartCutsceneDelayed());
    }

    void DisableGameplay()
    {
        Debug.Log("Disabling gameplay systems...");
        // 1. TẮT TẤT CẢ SCRIPT MOVEMENT TRƯỚC
        /* foreach (MonoBehaviour script in gameplayScripts)
        {
            if (script != null)
            {
                Debug.Log($"Disabling script: {script.GetType().Name}");
                script.enabled = false;
            }
        } */
        // Tắt character controller
        foreach (MonoBehaviour script in gameplayScripts)
        {
            if (script != null && script is ManualCharacterController manualController)
            {
                Debug.Log("Disabling gravity on ManualCharacterController");
                manualController.DisableGravity();
            }
        }
        
        // 2. Sau đó mới tắt scripts
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
                // Không tắt controller, thay vào đó freeze position trong Update
            }
            else
            {
                Debug.Log("Disabling CharacterController");
                characterController.enabled = false;
            }
        }
        // Ẩn gameplay UI
        if (gameplayUI)
        {
            gameplayUI.gameObject.SetActive(false);
        }

        // Chuyển camera
        if (gameplayCamera)
        {
            gameplayCamera.enabled = false;
        }

        if (cutsceneCamera)
        {
            cutsceneCamera.enabled = true;
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
    // Thêm method mới để sync rotation
    void SyncCharacterRotationForCutscene()
    {
        if (playerCharacter)
        {
            // Lưu rotation hiện tại từ hierarchy
            Vector3 currentRotation = playerCharacter.transform.eulerAngles;
            Debug.Log($"Character hierarchy rotation: {currentRotation}");
            
            // Thêm component để sync rotation
            TimelineRotationSync rotationSync = playerCharacter.GetComponent<TimelineRotationSync>();
            if (rotationSync == null)
            {
                rotationSync = playerCharacter.AddComponent<TimelineRotationSync>();
            }
            
            rotationSync.timelineDirector = timelineDirector;
            rotationSync.targetCharacter = playerCharacter;
            rotationSync.syncRotationWithHierarchy = true;
            
            // Hoặc set rotation cụ thể
            // rotationSync.useCustomRotation = true;
            // rotationSync.customRotationEuler = new Vector3(0, 180, 0); // Ví dụ: quay mặt về phía camera
        }
    }
    
    void OnCutsceneEnded(PlayableDirector director)
    {
        if (director == timelineDirector)
        {
            Debug.Log("=== CUTSCENE ENDED ===");
            EndCutscene();
        }
    }
    
    void EndCutscene()
    {
        Debug.Log("Ending cutscene, returning to gameplay...");
        isInCutscene = false;
        isPositionFrozen = false;
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
        // Bật lại character controller
        if (playerCharacter && characterController)
        {
            // Tắt controller để set position
            characterController.enabled = false;
            
            // Set về vị trí cuối cùng được freeze hoặc vị trí gần mặt đất
            if (freezeCharacterDuringCutscene)
            {
                playerCharacter.transform.position = frozenPosition;
                playerCharacter.transform.rotation = frozenRotation;
            }
            else
            {
                // Nếu không freeze, tìm mặt đất gần nhất
                Vector3 currentPos = playerCharacter.transform.position;
                if (Physics.Raycast(currentPos, Vector3.down, out RaycastHit hit, 10f))
                {
                    playerCharacter.transform.position = hit.point + Vector3.up * 0.1f;
                    Debug.Log($"Repositioned character to ground: {hit.point}");
                }
            }
            
            //yield return null; // Wait một frame
            Debug.Log("Enabling CharacterController");
            characterController.enabled = true;
        }
        // FIX: Bật lại Rigidbody settings
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
            cutsceneCamera.enabled = false;
        }

        if (gameplayCamera)
        {
            gameplayCamera.enabled = true;
        }

        // Ẩn cutscene UI
        if (cutsceneUI)
        {
            cutsceneUI.gameObject.SetActive(false);
        }
        Debug.Log("Gameplay enabled successfully!");
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
        
        EnableGameplay();
        
        // Reset vị trí character nếu cần
        if (playerCharacter)
        {
            characterController.enabled = false;
            characterController.enabled = true;
        }
    }
    
    
    // Hàm public để trigger manual
    public void ManualTriggerCutscene()
    {
        TriggerCutscene();
    }
    
    void OnDrawGizmos()
    {
        if (showDebugGizmos && triggerPoint)
        {
            Gizmos.color = cutsceneTriggered ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(triggerPoint.position, triggerDistance);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(triggerPoint.position, Vector3.one * 0.5f);
        }
    }
}
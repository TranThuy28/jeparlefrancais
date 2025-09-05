// NPCAnimationController.cs - Hệ thống animation hoàn chỉnh cho NPC
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Class định nghĩa animation cho từng dòng dialogue
/// </summary>
[System.Serializable]
public class DialogueAnimation
{
    [Header("Animation Settings")]
    [Tooltip("Dòng thoại thứ mấy sẽ trigger animation này (bắt đầu từ 0)")]
    public int dialogueLineIndex;
    
    [Tooltip("Tên animation state trong Animator")]
    public string animationName;
    
    [Tooltip("Delay trước khi play animation (giây)")]
    public float delay = 0f;
    
    [Tooltip("Thời gian animation kéo dài (-1 = vô hạn)")]
    public float duration = -1f;
    
    [Tooltip("Có quay về idle sau khi animation kết thúc không")]
    public bool returnToIdle = true;
    
    [Tooltip("Có play sound effect không")]
    public bool playSound = false;
    
    [Tooltip("Sound effect để play cùng animation")]
    public AudioClip soundEffect;
}

/// <summary>
/// Component quản lý animation cho NPC trong dialogue system
/// </summary>
public class NPCAnimationController : MonoBehaviour
{
    [Header("Core Components")]
    [Tooltip("Animator component của NPC")]
    public Animator animator;
    
    [Tooltip("AudioSource để play sound effects")]
    public AudioSource audioSource;
    
    [Header("Basic Animation States")]
    [Tooltip("Animation mặc định khi không làm gì")]
    public string idleAnimationName = "Idle";
    
    [Tooltip("Animation khi đang nói chuyện")]
    public string talkingAnimationName = "Talking";
    
    [Tooltip("Animation khi player đến gần")]
    public string greetingAnimationName = "Greeting";
    
    [Header("Animation Settings")]
    [Tooltip("Thời gian transition giữa các animation")]
    public float transitionTime = 0.2f;
    
    [Tooltip("Tự động play talking animation khi dialogue bắt đầu")]
    public bool autoPlayTalkingAnimation = true;
    
    [Tooltip("Quay về idle khi dialogue kết thúc")]
    public bool returnToIdleAfterDialogue = true;
    
    [Header("Dialogue-Specific Animations")]
    [Tooltip("Animation đặc biệt cho từng dòng thoại")]
    public DialogueAnimation[] dialogueAnimations;
    
    [Header("Debug")]
    [Tooltip("Hiển thị debug info trong console")]
    public bool enableDebugLog = true;
    
    // Private variables
    private DialogueManager dialogueManager;
    private DialogueTrigger dialogueTrigger;
    private string currentAnimationState;
    private string previousAnimationState;
    private Coroutine currentAnimationCoroutine;
    
    // Dictionary để tìm animation nhanh theo line index
    private Dictionary<int, DialogueAnimation> animationMap;
    
    // Trạng thái hiện tại
    private bool isInDialogue = false;
    private bool playerNearby = false;
    
    void Start()
    {
        SetupComponents();
        SetupAnimationMap();
        SubscribeToEvents();
        
        // Bắt đầu với idle animation
        currentAnimationState = idleAnimationName;
        previousAnimationState = idleAnimationName;
        
        DebugLog("NPCAnimationController đã khởi tạo.");
    }
    
    /// <summary>
    /// Setup các component cần thiết
    /// </summary>
    void SetupComponents()
    {
        // Auto-find Animator nếu chưa gán
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError($"NPCAnimationController trên {gameObject.name}: Không tìm thấy Animator component!");
            }
        }
        
        // Auto-find AudioSource nếu chưa gán
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                // Tạo AudioSource mới nếu không có
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1f; // 3D sound
            }
        }
        
        // Tìm DialogueManager
        dialogueManager = DialogueManager.Instance;
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<DialogueManager>();
        }
        
        // Tìm DialogueTrigger trên cùng GameObject
        dialogueTrigger = GetComponent<DialogueTrigger>();
        
        DebugLog("Đã setup components.");
    }
    
    /// <summary>
    /// Tạo dictionary mapping từ dialogue line index sang animation
    /// </summary>
    void SetupAnimationMap()
    {
        animationMap = new Dictionary<int, DialogueAnimation>();
        
        if (dialogueAnimations != null)
        {
            foreach (DialogueAnimation anim in dialogueAnimations)
            {
                if (!animationMap.ContainsKey(anim.dialogueLineIndex))
                {
                    animationMap[anim.dialogueLineIndex] = anim;
                    DebugLog($"Đã map animation '{anim.animationName}' cho dòng thoại {anim.dialogueLineIndex}");
                }
                else
                {
                    Debug.LogWarning($"Duplicate dialogue line index {anim.dialogueLineIndex} trong animation settings!");
                }
            }
        }
        
        DebugLog($"Đã setup {animationMap.Count} dialogue animations.");
    }
    
    /// <summary>
    /// Subscribe vào các events từ DialogueManager và DialogueTrigger
    /// </summary>
    void SubscribeToEvents()
    {
        // Subscribe to DialogueManager events
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueStarted += OnDialogueStarted;
            dialogueManager.OnDialogueLineChanged += OnDialogueLineChanged;
            dialogueManager.OnDialogueEnded += OnDialogueEnded;
            DebugLog("Đã subscribe vào DialogueManager events.");
        }
        
        // Subscribe to DialogueTrigger events
        if (dialogueTrigger != null)
        {
            dialogueTrigger.OnPlayerEnterRange += OnPlayerEnterRange;
            dialogueTrigger.OnPlayerExitRange += OnPlayerExitRange;
            DebugLog("Đã subscribe vào DialogueTrigger events.");
        }
    }
    
    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    /// <summary>
    /// Unsubscribe khỏi các events để tránh memory leak
    /// </summary>
    void UnsubscribeFromEvents()
    {
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueStarted -= OnDialogueStarted;
            dialogueManager.OnDialogueLineChanged -= OnDialogueLineChanged;
            dialogueManager.OnDialogueEnded -= OnDialogueEnded;
        }
        
        if (dialogueTrigger != null)
        {
            dialogueTrigger.OnPlayerEnterRange -= OnPlayerEnterRange;
            dialogueTrigger.OnPlayerExitRange -= OnPlayerExitRange;
        }
    }
    
    // ========== EVENT HANDLERS ==========
    
    /// <summary>
    /// Khi player vào vùng tương tác
    /// </summary>
    void OnPlayerEnterRange()
    {
        playerNearby = true;
        
        /* if (!isInDialogue && !string.IsNullOrEmpty(greetingAnimationName))
        {
            // Play greeting animation ngắn
            PlayTemporaryAnimation(greetingAnimationName, 2f, true);
        } */
        
        DebugLog("Player đến gần NPC.");
    }
    
    /// <summary>
    /// Khi player rời vùng tương tác
    /// </summary>
    void OnPlayerExitRange()
    {
        playerNearby = false;
        
        if (!isInDialogue)
        {
            PlayAnimation(idleAnimationName);
        }
        
        DebugLog("Player rời xa NPC.");
    }
    
    /// <summary>
    /// Khi dialogue bắt đầu
    /// </summary>
    void OnDialogueStarted()
    {
        isInDialogue = true;
        
        // Stop any current animation coroutine
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
            currentAnimationCoroutine = null;
        }
        
        /* if (autoPlayTalkingAnimation)
        {
            PlayAnimation(talkingAnimationName);
        } */
        
        DebugLog("Dialogue bắt đầu");
    }
    
    /// <summary>
    /// Khi dòng thoại thay đổi
    /// </summary>
    void OnDialogueLineChanged(int lineIndex)
    {
        DebugLog($"Dòng thoại thay đổi: {lineIndex}");
        
        // Kiểm tra có animation đặc biệt cho dòng này không
        if (animationMap.ContainsKey(lineIndex))
        {
            DialogueAnimation dialogueAnim = animationMap[lineIndex];
            PlayDialogueAnimation(dialogueAnim);
            DebugLog($"Playing dialogue animation '{dialogueAnim.animationName}' cho dòng {lineIndex}");
        }
        else if (autoPlayTalkingAnimation)
        {
            DebugLog("Không có animation đặc biệt, play idle animation");
            // Không có animation đặc biệt, quay về talking animation
            PlayAnimation(idleAnimationName);
        }
    }
    
    /// <summary>
    /// Khi dialogue kết thúc
    /// </summary>
    void OnDialogueEnded()
    {
        isInDialogue = false;
        
        // Stop animation coroutine
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
            currentAnimationCoroutine = null;
        }
        
        // Quay về trạng thái phù hợp
        if (returnToIdleAfterDialogue)
        {
            PlayAnimation(idleAnimationName);
        
        }
        
        DebugLog("Dialogue kết thúc - quay về idle state.");
    }
    
    // ========== ANIMATION METHODS ==========
    
    /// <summary>
    /// Play animation đặc biệt cho dialogue với delay và duration
    /// </summary>
    void PlayDialogueAnimation(DialogueAnimation dialogueAnim)
    {
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
        }
        
        currentAnimationCoroutine = StartCoroutine(PlayAnimationWithSettings(dialogueAnim));
    }
    
    /// <summary>
    /// Coroutine play animation với các settings từ DialogueAnimation
    /// </summary>
    IEnumerator PlayAnimationWithSettings(DialogueAnimation dialogueAnim)
    {
        // Delay trước khi play animation
        if (dialogueAnim.delay > 0)
        {
            DebugLog($"Waiting {dialogueAnim.delay}s trước khi play animation '{dialogueAnim.animationName}'");
            yield return new WaitForSeconds(dialogueAnim.delay);
        }
        
        // Play sound effect nếu có
        if (dialogueAnim.playSound && dialogueAnim.soundEffect != null && audioSource != null)
        {
            audioSource.PlayOneShot(dialogueAnim.soundEffect);
            DebugLog($"Playing sound effect cho animation '{dialogueAnim.animationName}'");
        }
        
        // Play animation
        PlayAnimation(dialogueAnim.animationName);
        
        // Nếu có duration, đợi rồi quay về trạng thái phù hợp
        if (dialogueAnim.duration > 0)
        {
            yield return new WaitForSeconds(dialogueAnim.duration);
            
            if (dialogueAnim.returnToIdle && isInDialogue)
            {
                string returnAnimation = idleAnimationName;
                PlayAnimation(returnAnimation);
                DebugLog($"Duration hết, quay về '{returnAnimation}' animation");
            }
        }
        
        currentAnimationCoroutine = null;
    }
    
    /// <summary>
    /// Play animation tạm thời với duration cố định
    /// </summary>
    public void PlayTemporaryAnimation(string animationName, float duration, bool returnToIdle)
    {
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
        }
        
        currentAnimationCoroutine = StartCoroutine(PlayTempAnimationCoroutine(animationName, duration, returnToIdle));
    }
    
    IEnumerator PlayTempAnimationCoroutine(string animationName, float duration, bool returnToIdle)
    {
        PlayAnimation(animationName);
        yield return new WaitForSeconds(duration);
        
        if (returnToIdle)
        {
            if (isInDialogue && autoPlayTalkingAnimation)
            {
                PlayAnimation(talkingAnimationName);
            }
            else
            {
                PlayAnimation(idleAnimationName);
            }
        }
        
        currentAnimationCoroutine = null;
    }
    
    /// <summary>
    /// Play animation cơ bản với CrossFade mượt mà
    /// </summary>
    public void PlayAnimation(string animationName)
    {
        if (animator == null || string.IsNullOrEmpty(animationName)) 
        {
            DebugLog($"Không thể play animation: animator null hoặc animation name rỗng");
            return;
        }
        
        
        // Chỉ chuyển animation nếu khác với hiện tại
        if (currentAnimationState != animationName)
        {
            previousAnimationState = currentAnimationState;
            currentAnimationState = animationName;
            
            // Đảm bảo animator enabled và không bị stuck
            animator.enabled = true;
            
            // Sử dụng CrossFade để chuyển mượt mà
            animator.CrossFade(animationName, transitionTime);
            
            DebugLog($"Animation chuyển từ '{previousAnimationState}' sang '{currentAnimationState}'");
        }
    }
    
    /// <summary>
    /// Play animation ngay lập tức không có transition
    /// </summary>
    public void PlayAnimationImmediate(string animationName)
    {
        if (animator == null || string.IsNullOrEmpty(animationName))
        {
            DebugLog($"Không thể play immediate animation: animator null hoặc animation name rỗng");
            return;
        }
        if (!HasAnimation(animationName))
        {
            Debug.LogWarning($"Animation '{animationName}' không tồn tại!");
            return;
        }
        
        previousAnimationState = currentAnimationState;
        currentAnimationState = animationName;
        
        animator.enabled = true;
        animator.Play(animationName);
        
        DebugLog($"Play immediate animation: {animationName}");
    }
    
    /// <summary>
    /// Kiểm tra animation có tồn tại trong Animator Controller không
    /// </summary>
    public bool HasAnimation(string animationName)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return false;
        
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == animationName)
                return true;
        }
        return false;
    }
    
    /// <summary>
    /// Fix animation bị stuck/đơ
    /// </summary>
    /* public void FixStuckAnimation()
    {
        if (animator == null) return;
        
        DebugLog("Fixing stuck animation...");
        
        // Re-enable animator
        animator.enabled = false;
        yield return null; // Wait một frame
        animator.enabled = true;
        
        // Force update
        animator.Update(0f);
        
        // Play lại animation hiện tại
        animator.CrossFade(currentAnimationState.Length > 0 ? currentAnimationState : idleAnimationName, 0.1f);
        
        DebugLog("Animation fix completed.");
    } */
    
    // ========== UTILITY METHODS ==========
    
    /// <summary>
    /// Set tốc độ animation
    /// </summary>
    public void SetAnimatorSpeed(float speed)
    {
        if (animator != null)
        {
            animator.speed = speed;
            DebugLog($"Animator speed set to: {speed}");
        }
    }
    
    /// <summary>
    /// Pause animation
    /// </summary>
    public void PauseAnimation()
    {
        SetAnimatorSpeed(0f);
        DebugLog("Animation paused.");
    }
    
    /// <summary>
    /// Resume animation
    /// </summary>
    public void ResumeAnimation()
    {
        SetAnimatorSpeed(1f);
        DebugLog("Animation resumed.");
    }
    
    /// <summary>
    /// Force về idle animation
    /// </summary>
    public void ForceIdle()
    {
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
            currentAnimationCoroutine = null;
        }
        
        PlayAnimation(idleAnimationName);
        DebugLog("Forced to idle animation.");
    }
    
    // ========== PUBLIC GETTERS ==========
    
    public string GetCurrentAnimation() => currentAnimationState;
    public string GetPreviousAnimation() => previousAnimationState;
    public bool IsInDialogue() => isInDialogue;
    public bool IsPlayerNearby() => playerNearby;
    public bool HasPendingAnimation() => currentAnimationCoroutine != null;
    
    // ========== DEBUG ==========
    
    void DebugLog(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log($"[NPCAnimController - {gameObject.name}] {message}");
        }
    }
    
    // ========== EDITOR HELPERS ==========
    
    [ContextMenu("Test Idle Animation")]
    void TestIdleAnimation()
    {
        PlayAnimation(idleAnimationName);
    }
    
    [ContextMenu("Test Talking Animation")]  
    void TestTalkingAnimation()
    {
        PlayAnimation(talkingAnimationName);
    }
    
    [ContextMenu("Test Greeting Animation")]
    void TestGreetingAnimation()
    {
        if (!string.IsNullOrEmpty(greetingAnimationName))
        {
            PlayAnimation(greetingAnimationName);
        }
    }
    
    /* [ContextMenu("Fix Stuck Animation")]
    void TestFixStuckAnimation()
    {
        StartCoroutine(FixStuckAnimationCoroutine());
    }
    
    IEnumerator FixStuckAnimationCoroutine()
    {
        yield return StartCoroutine(nameof(FixStuckAnimation));
    } */
    
    [ContextMenu("List All Animations")]
    void ListAllAnimations()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            Debug.Log("Không có Animator Controller!");
            return;
        }
        
        Debug.Log($"=== Animations trong {animator.runtimeAnimatorController.name} ===");
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            Debug.Log($"- {clip.name} (Length: {clip.length}s)");
        }
    }
}
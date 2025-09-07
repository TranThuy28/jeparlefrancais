using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Cinemachine;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    public GameObject dialoguePanel;
    public GameObject dialogueUI;
    public TaskManager taskManager;

    public CinemachineCamera dialogueCamera;
    public CinemachineCamera playerCamera;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI speakerNameText;
    
    [Header("Camera Transition Settings")]
    public float cameraTransitionDuration = 1.5f;
    public AnimationCurve cameraTransitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("Player Control Settings")]
    public string playerTag = "Player";
    public GameObject player;

    [Header("Animation & Effects")]
    public bool useTypingEffect = false;
    public float typingSpeed = 0.03f;
    public AudioSource dialogueAudioSource;
    public AudioClip[] typingSounds;
    private string[] lines;
    private int index;
    private bool isTalking;
    private bool isTransitioning;
    private bool isTyping;
    private Transform npc;
    private NPCAnimationController currentNPCAnimController;

    public System.Action<int> OnDialogueLineChanged;
    public System.Action OnDialogueStarted;
    public System.Action OnDialogueEnded;
    void Awake()
    {
        Instance = this;
        dialogueUI.SetActive(false);
        SetupInitialState();
        Debug.Log("DialogueManager đã được khởi tạo.");
    }
    void SetupInitialState()
    {
        if (dialogueUI != null)
            dialogueUI.SetActive(false);
            
        // Tự động tìm player nếu chưa gán
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag(playerTag);
        }
        
        // Setup audio
        if (dialogueAudioSource == null)
            dialogueAudioSource = GetComponent<AudioSource>();
    }
    void Update()
    {
        if (isTalking && !isTransitioning && Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("Đã nhấn K để chuyển dòng thoại tiếp theo.");
            ShowLine();
        }
    }
    
    public void StartDialogue(string[] dialogueLines, Transform npcTransform)
    {
        if (isTransitioning) return; // Tránh bắt đầu dialogue khi đang chuyển camera
        
        npc = npcTransform;
        lines = dialogueLines;
        index = 0;
        isTalking = true;
        
        // Tìm NPC Animation Controller
        currentNPCAnimController = npc?.GetComponent<NPCAnimationController>();
            
        Debug.Log($"Bắt đầu hội thoại. Tổng số dòng: {lines.Length}");
        OnDialogueStarted?.Invoke();
        // Bắt đầu chuyển camera mượt mà
        StartCoroutine(SwitchToDialogueCameraSmooth());
    }
    
    void ShowLine()
    {
        if (index < lines.Length)
        {
            // Trigger animation event cho dòng hiện tại
            OnDialogueLineChanged?.Invoke(index);

            dialogueText.text = lines[index];
            Debug.Log("Hiển thị lời thoại: " + lines[index]);
            index++;
            Debug.Log("Chỉ số dòng thoại hiện tại: " + index);
        }
        else
        {
            EndDialogue();
        }
    }
    
    public void EndDialogue()
    {
        if (isTransitioning) return; // Tránh kết thúc dialogue khi đang chuyển camera
        
        isTalking = false;
        Debug.Log("Kết thúc hội thoại.");
        OnDialogueEnded?.Invoke();
        // Bắt đầu chuyển về camera người chơi mượt mà
        taskManager.CompleteTask(0); // Giả sử ID nhiệm vụ là 0, thay đổi theo nhu cầu

        StartCoroutine(SwitchToPlayerCameraSmooth());
    }
    
    IEnumerator SwitchToDialogueCameraSmooth()
    {
        isTransitioning = true;
        Debug.Log("Bắt đầu chuyển sang camera hội thoại mượt mà.");
        
        // Thiết lập camera dialogue để nhìn về phía NPC
        if (npc != null && dialogueCamera != null)
        {
            // Đặt vị trí camera để nhìn NPC từ góc đẹp
            Vector3 npcPosition = npc.position;
            Vector3 cameraOffset = new Vector3(2.0f, 1.5f, 3f); // Có thể điều chỉnh offset này
            
            dialogueCamera.transform.position = npcPosition + cameraOffset;
            dialogueCamera.transform.LookAt(npcPosition + Vector3.up * 1.5f); // Nhìn về mặt NPC
        }
        
        // Chuyển độ ưu tiên camera với hiệu ứng mượt
        float elapsedTime = 0f;
        int startPriority = dialogueCamera.Priority;
        int targetPriority = 200;
        
        playerCamera.Priority = 50;
        
        while (elapsedTime < cameraTransitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / cameraTransitionDuration;
            float curveValue = cameraTransitionCurve.Evaluate(t);
            
            // Smooth transition của priority
            dialogueCamera.Priority = Mathf.RoundToInt(Mathf.Lerp(startPriority, targetPriority, curveValue));
            
            yield return null;
        }
        
        dialogueCamera.Priority = targetPriority;
        
        // Hiển thị UI dialogue sau khi camera đã chuyển xong
        dialogueUI.SetActive(true);
        
        // Hiển thị dòng đầu tiên
        ShowLine();
        
        isTransitioning = false;
        Debug.Log("Hoàn thành chuyển camera hội thoại.");
    }
    
    IEnumerator SwitchToPlayerCameraSmooth()
    {
        isTransitioning = true;
        Debug.Log("Bắt đầu chuyển về camera người chơi mượt mà.");
        
        // Ẩn UI dialogue trước
        dialogueUI.SetActive(false);
        
        // Chuyển độ ưu tiên camera với hiệu ứng mượt
        float elapsedTime = 0f;
        int startPriority = playerCamera.Priority;
        int targetPriority = 200;
        
        dialogueCamera.Priority = 50;
        
        while (elapsedTime < cameraTransitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / cameraTransitionDuration;
            float curveValue = cameraTransitionCurve.Evaluate(t);
            
            // Smooth transition của priority
            playerCamera.Priority = Mathf.RoundToInt(Mathf.Lerp(startPriority, targetPriority, curveValue));
            
            yield return null;
        }
        
        playerCamera.Priority = targetPriority;
        
        // Kích hoạt lại điều khiển người chơi sau khi camera đã chuyển xong
        //EnablePlayerController();
            
        isTransitioning = false;
        Debug.Log("Hoàn thành chuyển về camera người chơi.");
    }
    
    // Phương thức để vô hiệu hóa PlayerController một cách an toàn
    void DisablePlayerController()
    {
        if (player != null)
        {
            MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                string scriptName = script.GetType().Name.ToLower();
                if (scriptName.Contains("player") || scriptName.Contains("controller") || scriptName.Contains("movement"))
                {
                    script.enabled = false;
                    Debug.Log("Đã vô hiệu hóa script: " + script.GetType().Name);
                }
            }
        }
        else
        {
            Debug.LogWarning("Không tìm thấy GameObject với tag: " + playerTag);
        }
    }
    
    // Phương thức để kích hoạt lại PlayerController một cách an toàn
    void EnablePlayerController()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                string scriptName = script.GetType().Name.ToLower();
                if (scriptName.Contains("player") || scriptName.Contains("controller") || scriptName.Contains("movement"))
                {
                    script.enabled = true;
                    Debug.Log("Đã kích hoạt lại script: " + script.GetType().Name);
                }
            }
        }
        else
        {
            Debug.LogWarning("Không tìm thấy GameObject với tag: " + playerTag);
        }
    }
    
    // Phương thức legacy để tương thích ngược (nếu cần)
    void SwitchToDialogueCamera()
    {
        dialogueCamera.Priority = 200;
        playerCamera.Priority = 50;
    }
    
    void SwitchToPlayerCamera()
    {
        dialogueCamera.Priority = 50;
        playerCamera.Priority = 100;
    }
}
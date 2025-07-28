using UnityEngine;
using UnityEngine.UI;

public class DialogueTrigger : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject interactUI;
    public Canvas worldCanvas; // Canvas để hiển thị UI trên đầu NPC
    public float uiHeightOffset = 2.5f; // Độ cao UI so với NPC
    
    [Header("Dialogue Settings")]
    public DialogueManager dialogueManager;
    public string npcName = "NPC";
    [TextArea(3, 10)]
    public string[] dialogueLines;
    
    [Header("Interaction Settings")]
    public float interactionRange = 2f; // Phạm vi tương tác
    public LayerMask playerLayer = 1; // Layer của player
    
    [Header("Debug")]
    public bool showDebugGizmos = true;
    
    private bool playerInRange;
    private bool isDialogueActive;
    private Transform playerTransform;
    private Camera mainCamera;
    
    void Start()
    {
        SetupComponents();
        SetupInteractUI();
    }
    
    void SetupComponents()
    {
        // Tìm DialogueManager nếu chưa được gán
        if (dialogueManager == null)
        {
            dialogueManager = DialogueManager.Instance;
            if (dialogueManager == null)
            {
                dialogueManager = FindObjectOfType<DialogueManager>();
            }
        }
        
        // Tìm main camera
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }
        }
        
        // Tìm player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }
    
    void SetupInteractUI()
{
    if (interactUI == null) return;

    // Tạo World Canvas nếu chưa có
    if (worldCanvas == null)
    {
        GameObject canvasGO = new GameObject(npcName + "_InteractCanvas");
        canvasGO.transform.SetParent(transform);
        canvasGO.transform.localPosition = Vector3.up * uiHeightOffset;

        worldCanvas = canvasGO.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;
        worldCanvas.worldCamera = mainCamera;
        worldCanvas.sortingOrder = 100;

        // Điều chỉnh kích thước canvas
        RectTransform canvasRect = worldCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(2, 1);
        canvasRect.localScale = Vector3.one * 0.01f;

        // Đảm bảo interactUI là con của canvas và đặt lại vị trí cho đúng
        interactUI.transform.SetParent(worldCanvas.transform, false);

        // 👉 Sửa lỗi bay xa: đặt lại vị trí và scale của interactUI
        RectTransform uiRect = interactUI.GetComponent<RectTransform>();
        if (uiRect != null)
        {
            uiRect.anchoredPosition = Vector2.zero;
            uiRect.localPosition = Vector3.zero;
            uiRect.localScale = Vector3.one;
        }
        else
        {
            // fallback nếu không có RectTransform (dù hiếm)
            interactUI.transform.localPosition = Vector3.zero;
            interactUI.transform.localScale = Vector3.one;
        }

        // Đảm bảo UI luôn nhìn về camera
        StartCoroutine(LookAtCamera());
    }

    // Ẩn UI ban đầu
    interactUI.SetActive(false);
}

    System.Collections.IEnumerator LookAtCamera()
    {
        while (worldCanvas != null && mainCamera != null)
        {
            // Xoay canvas để luôn nhìn về camera
            worldCanvas.transform.LookAt(worldCanvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
                                        mainCamera.transform.rotation * Vector3.up);
            yield return new WaitForSeconds(0.1f); // Update mỗi 0.1s để tiết kiệm performance
        }
    }
    
    void Update()
    {
        CheckPlayerDistance();
        HandleInput();
    }
    
    void CheckPlayerDistance()
    {
        if (playerTransform == null) return;
        
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool inRange = distance <= interactionRange;
        
        // Nếu trạng thái thay đổi
        if (inRange != playerInRange)
        {
            playerInRange = inRange;
            
            if (playerInRange)
            {
                OnPlayerEnterRange();
            }
            else
            {
                OnPlayerExitRange();
            }
        }
    }
    
    void HandleInput()
    {
        // Chỉ cho phép bắt đầu dialogue khi player trong vùng và không có dialogue nào đang diễn ra
        if (playerInRange && !isDialogueActive && Input.GetKeyDown(KeyCode.E))
        {
            StartDialogue();
        }
    }
    
    void OnPlayerEnterRange()
    {
        Debug.Log("Player đã vào vùng trò chuyện với " + npcName + " (khoảng cách: " + interactionRange + "m)");

        // Chỉ hiển thị interact UI nếu không có dialogue nào đang diễn ra
        if (!isDialogueActive && interactUI != null)
        {
            interactUI.SetActive(true);
        }
        
    }
    
    void OnPlayerExitRange()
    {
        Debug.Log("Player đã rời xa " + npcName);
        
        if (interactUI != null)
        {
            interactUI.SetActive(false);
        }
        
        // Tùy chọn: tự động kết thúc dialogue khi player đi xa
        // if (isDialogueActive && dialogueManager != null)
        //     dialogueManager.EndDialogue();
    }
    
    void StartDialogue()
    {
        if (interactUI != null)
        {
            interactUI.SetActive(false);
        }
        Debug.Log("Bắt đầu hội thoại với " + npcName);
        
        // Kiểm tra các điều kiện cần thiết
        if (dialogueManager == null)
        {
            Debug.LogError("DialogueManager không tìm thấy!");
            return;
        }
        
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogError("Không có dialogue lines cho " + npcName);
            return;
        }
        
        // Bắt đầu dialogue
        isDialogueActive = true;
        
        // Ẩn interact UI
        
        // Bắt đầu dialogue với DialogueManager
        dialogueManager.StartDialogue(dialogueLines, transform);
        
        // Theo dõi khi dialogue kết thúc
        StartCoroutine(WaitForDialogueEnd());
    }
    
    System.Collections.IEnumerator WaitForDialogueEnd()
    {
        // Chờ cho đến khi dialogue kết thúc
        while (dialogueManager != null && IsDialogueActive())
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        // Dialogue đã kết thúc
        isDialogueActive = false;
        
        // Hiển thị lại interact UI nếu player vẫn trong vùng
        if (playerInRange && interactUI != null && isDialogueActive)
        {
            interactUI.SetActive(true);
        }
        
        Debug.Log("Đã kết thúc hội thoại với " + npcName);
    }
    
    bool IsDialogueActive()
    {
        return dialogueManager != null && 
               dialogueManager.dialogueUI != null && 
               dialogueManager.dialogueUI.activeInHierarchy;
    }
    
    // Phương thức public để thay đổi dialogue từ script khác
    public void SetDialogue(string[] newDialogueLines)
    {
        dialogueLines = newDialogueLines;
    }
    
    public void SetNPCName(string newName)
    {
        npcName = newName;
        if (worldCanvas != null)
        {
            worldCanvas.gameObject.name = newName + "_InteractCanvas";
        }
    }
    
    // Gizmos để debug trong Scene view
    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;
        
        // Hiển thị vùng tương tác
        Gizmos.color = playerInRange ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        // Hiển thị vị trí UI
        Gizmos.color = Color.blue;
        Vector3 uiPosition = transform.position + Vector3.up * uiHeightOffset;
        Gizmos.DrawWireCube(uiPosition, Vector3.one * 0.5f);
        
        // Hiển thị hướng nhìn
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * 1f);
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        // Hiển thị tên NPC
        Vector3 labelPosition = transform.position + Vector3.up * (uiHeightOffset + 0.5f);
        
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(labelPosition, npcName);
        #endif
    }
}
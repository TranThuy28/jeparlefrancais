using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public GameObject interactUI;
    public DialogueManager dialogueManager;
    public string[] dialogueLines;
    
    [Header("NPC Settings")]
    public string npcName = "NPC";
    
    private bool playerInRange;
    private bool isDialogueActive;
    
    void Start()
    {
        // Đảm bảo dialogueManager được tham chiếu
        if (dialogueManager == null)
            dialogueManager = DialogueManager.Instance;
    }
    
    void Update()
    {
        // Chỉ cho phép bắt đầu dialogue khi player trong vùng và không có dialogue nào đang diễn ra
        if (playerInRange && !isDialogueActive && Input.GetKeyDown(KeyCode.E))
        {
            StartDialogue();
        }
    }
    
    void StartDialogue()
    {
        Debug.Log("Đã nhấn E để bắt đầu hội thoại với " + npcName);
        
        if (dialogueManager != null && dialogueLines.Length > 0)
        {
            isDialogueActive = true;
            interactUI.SetActive(false);
            
            // Truyền transform của NPC này để camera có thể nhìn về phía NPC
            dialogueManager.StartDialogue(dialogueLines, transform);
            
            // Đăng ký sự kiện kết thúc dialogue (nếu cần)
            StartCoroutine(WaitForDialogueEnd());
        }
        else
        {
            Debug.LogWarning("DialogueManager hoặc dialogueLines chưa được thiết lập!");
        }
    }
    
    System.Collections.IEnumerator WaitForDialogueEnd()
    {
        // Chờ cho đến khi dialogue kết thúc
        while (dialogueManager != null && dialogueManager.GetComponent<DialogueManager>() != null)
        {
            // Kiểm tra xem dialogue có còn đang diễn ra không
            if (!IsDialogueActive())
            {
                isDialogueActive = false;
                
                // Hiển thị lại interact UI nếu player vẫn trong vùng
                if (playerInRange)
                {
                    interactUI.SetActive(true);
                }
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    bool IsDialogueActive()
    {
        // Kiểm tra xem dialogue UI có đang active không
        return dialogueManager != null && 
               dialogueManager.dialogueUI != null && 
               dialogueManager.dialogueUI.activeInHierarchy;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player đã vào vùng trò chuyện với " + npcName);
            playerInRange = true;
            
            // Chỉ hiển thị interact UI nếu không có dialogue nào đang diễn ra
            if (!isDialogueActive)
            {
                interactUI.SetActive(true);
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player đã rời khỏi vùng trò chuyện với " + npcName);
            playerInRange = false;
            interactUI.SetActive(false);
            
            // Nếu có dialogue đang diễn ra, có thể tự động kết thúc (tùy chọn)
            // Bỏ comment dòng dưới nếu muốn tự động kết thúc dialogue khi rời khỏi vùng
            // if (isDialogueActive && dialogueManager != null)
            //     dialogueManager.EndDialogue();
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Hiển thị vùng trigger trong Scene view
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
    }
}
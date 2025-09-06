using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CircularBoundary : MonoBehaviour
{
    [Header("Boundary Settings")]
    [SerializeField] private Transform centerPoint;
    [SerializeField] private float radius = 10f;
    
    [Header("UI Settings")]
    [SerializeField] private Text boundaryMessage; // Dùng Text thông thường
    [SerializeField] private TextMeshProUGUI boundaryMessageTMP; // Hoặc dùng TextMeshPro
    [SerializeField] private float messageDisplayTime = 2f;
    [SerializeField] private string warningText = "Bạn không thể đi xa hơn nữa";
    
    [Header("Dynamic Distance Settings")]
    [SerializeField] private bool showDynamicDistance = false;
    [SerializeField] private string dynamicTextFormat = "Khoảng cách: {0:F1}m";
    
    private bool isShowingMessage = false;
    private Coroutine hideMessageCoroutine;
    private GameObject uiCanvas;
    
    void Start()
    {
        // Kiểm tra centerPoint
        if (centerPoint == null)
        {
            Debug.LogWarning("Center Point chưa được gán! Sử dụng vị trí (0,0,0) làm tâm.");
            GameObject center = new GameObject("BoundaryCenter");
            centerPoint = center.transform;
            centerPoint.position = Vector3.zero;
        }
        
        // Ẩn thông báo khi bắt đầu
        HideMessage();
    }
    
    void Update()
    {
        CheckAndConstrainPosition();
    }
    
    private void CheckAndConstrainPosition()
    {
        // Tính khoảng cách từ nhân vật đến tâm vùng
        Vector3 centerPos = centerPoint.position;
        Vector3 currentPos = transform.position;
        
        // Chỉ tính khoảng cách trên mặt phẳng X-Z (bỏ qua Y để tránh ảnh hưởng của nhảy)
        Vector3 centerPosFlat = new Vector3(centerPos.x, currentPos.y, centerPos.z);
        float distanceFromCenter = Vector3.Distance(currentPos, centerPosFlat);
        
        // Nếu vượt quá bán kính
        if (distanceFromCenter > radius)
        {
            // Tính vector hướng từ tâm đến nhân vật
            Vector3 directionFromCenter = (currentPos - centerPosFlat).normalized;
            
            // Đặt nhân vật ở vị trí sát biên
            Vector3 newPosition = centerPosFlat + directionFromCenter * radius;
            transform.position = newPosition;
            Debug.Log("Đã giới hạn vị trí nhân vật trong vòng tròn.");
            
            // Hiển thị thông báo nếu chưa hiển thị
            if (!isShowingMessage)
            {
                ShowBoundaryMessage();
            }
        }
    }
    
    private void ShowBoundaryMessage()
    {
        isShowingMessage = true;
        
        // Hiển thị thông báo (Text hoặc TextMeshPro)
        if (boundaryMessage != null)
        {
            boundaryMessage.text = warningText;
            boundaryMessage.gameObject.SetActive(true);
        }
        
        if (boundaryMessageTMP != null)
        {
            boundaryMessageTMP.text = warningText;
            boundaryMessageTMP.gameObject.SetActive(true);
        }
        
        // Hủy coroutine cũ nếu có
        if (hideMessageCoroutine != null)
        {
            StopCoroutine(hideMessageCoroutine);
        }
        
        // Bắt đầu đếm ngược để ẩn thông báo
        hideMessageCoroutine = StartCoroutine(HideMessageAfterDelay());
    }
    
    private IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDisplayTime);
        HideMessage();
    }
    
    private void HideMessage()
    {
        isShowingMessage = false;
        
        if (boundaryMessage != null)
        {
            boundaryMessage.gameObject.SetActive(false);
        }
        
        if (boundaryMessageTMP != null)
        {
            boundaryMessageTMP.gameObject.SetActive(false);
        }
    }
    
    // Vẽ vòng tròn ranh giới trong Scene view để dễ debug
    void OnDrawGizmosSelected()
    {
        if (centerPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(centerPoint.position, radius);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(centerPoint.position, 0.5f);
        }
    }
    
    // Phương thức công khai để thay đổi bán kính từ code khác
    public void SetRadius(float newRadius)
    {
        radius = Mathf.Max(0.1f, newRadius);
    }
    
    public void SetCenterPoint(Transform newCenter)
    {
        centerPoint = newCenter;
    }
    
    public void SetWarningMessage(string newMessage)
    {
        warningText = newMessage;
    }
    
    public void ToggleDynamicDistance(bool enabled)
    {
        showDynamicDistance = enabled;
        
        // Nếu tắt dynamic text, ẩn UI
        if (!enabled)
        {
            HideMessage();
        }
    }
    
    public void SetDynamicTextFormat(string newFormat)
    {
        dynamicTextFormat = newFormat;
    }
    
    void OnDestroy()
    {
        // Dọn dẹp UI khi script bị hủy
        if (uiCanvas != null)
        {
            DestroyImmediate(uiCanvas);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

// Script chính quản lý minigame
public class MouseCatchingMinigame : MonoBehaviour
{
    [Header("UI References")]
    public GameObject minigamePanel;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI gameOverText;
    public Button startButton;
    public Button closeButton;
    
    [Header("Mouse Settings")]
    public Sprite mouseSprite;
    public Vector2 mouseSize = new Vector2(40, 40);
    
    [Header("Game Settings")]
    public float gameTime = 60f;
    public float mouseSpawnRate = 0.8f;
    public int scorePerMouse = 10;
    public int maxMice = 8;
    public float difficultyIncrease = 0.1f;
    
    private int currentScore = 0;
    private float currentTime;
    private bool isGameActive = false;
    private List<MouseTarget> activeMice = new List<MouseTarget>();
    private Coroutine spawnCoroutine;
    
    void Start()
    {
        minigamePanel.SetActive(false);
        gameOverText.gameObject.SetActive(false);
        startButton.onClick.AddListener(StartGame);
        closeButton.onClick.AddListener(CloseMinigame);
    }
    
    public void OpenMinigame()
    {
        minigamePanel.SetActive(true);
        Time.timeScale = 0f; // Tạm dừng game 3D
        ResetGame();
    }
    
    public void CloseMinigame()
    {
        minigamePanel.SetActive(false);
        Time.timeScale = 1f; // Tiếp tục game 3D
        StopGame();
    }
    
    void StartGame()
    {
        isGameActive = true;
        currentScore = 0;
        currentTime = gameTime;
        
        startButton.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(false);
        
        spawnCoroutine = StartCoroutine(SpawnMice());
        StartCoroutine(GameTimer());
        
        UpdateUI();
    }
    
    void StopGame()
    {
        isGameActive = false;
        
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);
        
        // Xóa tất cả chuột
        foreach (MouseTarget mouse in activeMice)
        {
            if (mouse != null)
                Destroy(mouse.gameObject);
        }
        activeMice.Clear();
    }
    
    void ResetGame()
    {
        StopGame();
        startButton.gameObject.SetActive(true);
        gameOverText.gameObject.SetActive(false);
        currentScore = 0;
        currentTime = gameTime;
        UpdateUI();
    }
    
    IEnumerator SpawnMice()
    {
        float currentSpawnRate = mouseSpawnRate;
        
        while (isGameActive)
        {
            // Chỉ spawn nếu chưa đạt giới hạn
            // if (activeMice.Count < maxMice)
            // {
            SpawnMouse();
            // }
            
            yield return new WaitForSecondsRealtime(currentSpawnRate);
            
            // Tăng độ khó theo thời gian
            currentSpawnRate = Mathf.Max(0.3f, currentSpawnRate - difficultyIncrease * Time.unscaledTime * 0.01f);
        }
    }
    
    void SpawnMouse()
    {
        // Tạo GameObject cho chuột
        GameObject mouseObj = new GameObject("Mouse");
        mouseObj.transform.SetParent(minigamePanel.transform, false);
        
        // Thêm RectTransform
        RectTransform rectTransform = mouseObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = mouseSize;
        
        // Thêm Image component
        Image mouseImage = mouseObj.AddComponent<Image>();
        mouseImage.sprite = mouseSprite;
        mouseImage.preserveAspect = true;
        
        // Thêm Button component để click được
        Button mouseButton = mouseObj.AddComponent<Button>();
        
        // Thêm MouseTarget script
        MouseTarget mouseTarget = mouseObj.AddComponent<MouseTarget>();
        mouseTarget.Initialize(this);
        activeMice.Add(mouseTarget);
        
        // Thiết lập sự kiện click
        mouseButton.onClick.AddListener(() => mouseTarget.OnMouseClick());
        
        // Random vị trí spawn ở rìa màn hình
        RectTransform panelRect = minigamePanel.GetComponent<RectTransform>();
        Vector2 spawnPos = GetRandomEdgePosition(panelRect);
        rectTransform.anchoredPosition = spawnPos;
        
        // Thêm hiệu ứng spawn
        StartCoroutine(SpawnEffect(mouseObj));
    }
    
    Vector2 GetRandomEdgePosition(RectTransform panelRect)
    {
        int edge = Random.Range(0, 4); // 0=top, 1=right, 2=bottom, 3=left
        float x, y;
        
        switch(edge)
        {
            case 0: // Top
                x = Random.Range(-panelRect.rect.width/2, panelRect.rect.width/2);
                y = panelRect.rect.height/2 - 30;
                break;
            case 1: // Right
                x = panelRect.rect.width/2 - 30;
                y = Random.Range(-panelRect.rect.height/2, panelRect.rect.height/2);
                break;
            case 2: // Bottom
                x = Random.Range(-panelRect.rect.width/2, panelRect.rect.width/2);
                y = -panelRect.rect.height/2 + 30;
                break;
            default: // Left
                x = -panelRect.rect.width/2 + 30;
                y = Random.Range(-panelRect.rect.height/2, panelRect.rect.height/2);
                break;
        }
        
        return new Vector2(x, y);
    }
    
    IEnumerator SpawnEffect(GameObject mouseObj)
    {
        Vector3 originalScale = mouseObj.transform.localScale;
        mouseObj.transform.localScale = Vector3.zero;
        
        float elapsedTime = 0f;
        float duration = 0.3f;
        
        while (elapsedTime < duration)
        {
            float scale = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            mouseObj.transform.localScale = originalScale * scale;
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        
        mouseObj.transform.localScale = originalScale;
    }
    
    IEnumerator GameTimer()
    {
        while (currentTime > 0 && isGameActive)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            currentTime -= 0.1f;
            UpdateUI();
        }
        
        if (isGameActive)
            EndGame();
    }
    
    void EndGame()
    {
        isGameActive = false;
        gameOverText.text = $"Game Over!\nScore: {currentScore}";
        gameOverText.gameObject.SetActive(true);
        startButton.gameObject.SetActive(true);
        StopGame();
    }
    
    public void OnMouseCaught(MouseTarget mouse)
    {
        if (!isGameActive) return;
        
        currentScore += scorePerMouse;
        activeMice.Remove(mouse);
        Destroy(mouse.gameObject);
        UpdateUI();
    }
    
    void UpdateUI()
    {
        scoreText.text = "Score: " + currentScore;
        timeText.text = "Time: " + Mathf.Ceil(currentTime).ToString();
    }
}

// Script cho từng con chuột - chỉ là UI Image
public class MouseTarget : MonoBehaviour
{
    private MouseCatchingMinigame gameManager;
    private float lifetime = 4f;
    private bool isCaught = false;
    
    [Header("Mouse Settings")]
    public float moveSpeed = 400f;
    public float changeDirectionTime = 1f;
    public float panicSpeed = 250f;
    public float detectionRadius = 100f;
    
    private RectTransform rectTransform;
    private Image mouseImage;
    private Button mouseButton;
    private Vector2 moveDirection;
    private Vector2 targetPosition;
    private float directionTimer;
    private bool isPanicking = false;
    private Vector2 lastMousePosition;
    private float pauseTimer = 0f;
    private bool isPaused = false;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        mouseImage = GetComponent<Image>();
        mouseButton = GetComponent<Button>();
        
        SetRandomTarget();
        
        // Tự hủy sau một thời gian
        Destroy(gameObject, lifetime);
        
        // Lưu vị trí chuột máy tính ban đầu
        lastMousePosition =  Mouse.current.position.ReadValue();
        
        // Đảm bảo có thể click được
        mouseButton.targetGraphic = mouseImage;
    }
    
    void Update()
    {
        if (isCaught) return;
        
        // Kiểm tra chuột máy tính gần
        CheckForMouseCursor();
        
        // Di chuyển chuột
        MoveMouse();
        
        // Đổi hướng/mục tiêu định kỳ
        directionTimer += Time.unscaledDeltaTime;
        if (directionTimer >= changeDirectionTime)
        {
            if (!isPanicking)
            {
                SetRandomTarget();
            }
            directionTimer = 0f;
        }
        
        // Hiệu ứng hover khi chuột máy tính gần
        UpdateVisualEffects();
    }
    
    void UpdateVisualEffects()
    {
        if (isPanicking)
        {
            // Nhấp nháy khi sợ
            float alpha = 0.7f + 0.3f * Mathf.Sin(Time.unscaledTime * 10f);
            Color color = mouseImage.color;
            color.a = alpha;
            mouseImage.color = color;
        }
        else
        {
            // Màu bình thường
            Color color = mouseImage.color;
            color.a = 1f;
            mouseImage.color = color;
        }
    }
    
    void CheckForMouseCursor()
    {
        // Chuyển đổi vị trí chuột máy tính sang UI space
        Vector2 mouseScreenPos =  Mouse.current.position.ReadValue();
        Vector2 mouseUIPos;
        
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform, 
                mouseScreenPos, 
                canvas.worldCamera, 
                out mouseUIPos);
            
            // Tính khoảng cách đến con chuột
            float distance = Vector2.Distance(rectTransform.anchoredPosition, mouseUIPos);
            
            if (distance < detectionRadius)
            {
                // Chuột sợ và chạy trốn
                isPanicking = true;
                Vector2 fleeDirection = (rectTransform.anchoredPosition - mouseUIPos).normalized;
                moveDirection = fleeDirection;
                changeDirectionTime = 0.3f; // Phản ứng nhanh hơn khi sợ
                
                // Tăng kích thước một chút khi sợ
                rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, Vector3.one * 1.1f, Time.unscaledDeltaTime * 5f);
            }
            else if (isPanicking && distance > detectionRadius * 1.5f)
            {
                // Hết sợ, về trạng thái bình thường
                isPanicking = false;
                changeDirectionTime = 1f;
                SetRandomTarget();
                rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, Vector3.one, Time.unscaledDeltaTime * 3f);
            }
        }
        
        lastMousePosition = mouseScreenPos;
    }
    
    void MoveMouse()
    {
        float currentSpeed = isPanicking ? panicSpeed : moveSpeed;
        
        // Thỉnh thoảng dừng lại như chuột thật
        if (!isPanicking && Random.Range(0f, 1f) < 0.002f && !isPaused)
        {
            isPaused = true;
            pauseTimer = Random.Range(0.5f, 1.5f);
        }
        
        if (isPaused)
        {
            pauseTimer -= Time.unscaledDeltaTime;
            if (pauseTimer <= 0f)
            {
                isPaused = false;
                SetRandomTarget(); // Đổi hướng sau khi dừng
            }
            return; // Không di chuyển khi đang dừng
        }
        
        Vector2 newPos;
        
        if (isPanicking)
        {
            // Di chuyển thẳng khi hoảng sợ
            newPos = rectTransform.anchoredPosition + moveDirection * currentSpeed * Time.unscaledDeltaTime;
        }
        else
        {
            // Di chuyển về phía target
            Vector2 directionToTarget = (targetPosition - rectTransform.anchoredPosition).normalized;
            newPos = rectTransform.anchoredPosition + directionToTarget * currentSpeed * Time.unscaledDeltaTime;
            
            // Nếu gần target thì chọn target mới
            if (Vector2.Distance(rectTransform.anchoredPosition, targetPosition) < 20f)
            {
                SetRandomTarget();
            }
        }
        
        // Giới hạn trong panel và xử lý va chạm với tường
        RectTransform parentRect = transform.parent.GetComponent<RectTransform>();
        float maxX = parentRect.rect.width/2 - rectTransform.rect.width/2;
        float maxY = parentRect.rect.height/2 - rectTransform.rect.height/2;
        
        // Kiểm tra va chạm với tường
        bool hitWall = false;
        if (newPos.x > maxX || newPos.x < -maxX)
        {
            moveDirection.x = -moveDirection.x; // Nảy theo trục X
            hitWall = true;
        }
        if (newPos.y > maxY || newPos.y < -maxY)
        {
            moveDirection.y = -moveDirection.y; // Nảy theo trục Y
            hitWall = true;
        }
        
        newPos.x = Mathf.Clamp(newPos.x, -maxX, maxX);
        newPos.y = Mathf.Clamp(newPos.y, -maxY, maxY);
        
        rectTransform.anchoredPosition = newPos;
        
        // Nếu va chạm tường và không hoảng sợ, chọn target mới
        if (hitWall && !isPanicking)
        {
            SetRandomTarget();
        }
        
        // Xoay chuột theo hướng di chuyển
        if (moveDirection.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            rectTransform.rotation = Quaternion.Lerp(rectTransform.rotation, Quaternion.Euler(0, 0, angle), Time.unscaledDeltaTime * 5f);
        }
    }
    
    void SetRandomTarget()
    {
        RectTransform parentRect = transform.parent.GetComponent<RectTransform>();
        float maxX = parentRect.rect.width/2 - 50;
        float maxY = parentRect.rect.height/2 - 50;
        
        targetPosition = new Vector2(
            Random.Range(-maxX, maxX),
            Random.Range(-maxY, maxY)
        );
        
        // Tính toán hướng di chuyển mới
        moveDirection = (targetPosition - rectTransform.anchoredPosition).normalized;
        
        // Thêm một chút ngẫu nhiên vào hướng di chuyển
        float randomAngle = Random.Range(-30f, 30f) * Mathf.Deg2Rad;
        float cos = Mathf.Cos(randomAngle);
        float sin = Mathf.Sin(randomAngle);
        
        Vector2 rotatedDirection = new Vector2(
            moveDirection.x * cos - moveDirection.y * sin,
            moveDirection.x * sin + moveDirection.y * cos
        );
        
        moveDirection = rotatedDirection.normalized;
    }
    
    public void Initialize(MouseCatchingMinigame manager)
    {
        gameManager = manager;
    }
    
    public void OnMouseClick()
    {
        if (isCaught || gameManager == null) return;
        
        isCaught = true;
        gameManager.OnMouseCaught(this);
        
        // Hiệu ứng khi bắt được chuột
        StartCoroutine(CatchEffect());
    }
    
    IEnumerator CatchEffect()
    {
        // Phóng to rồi thu nhỏ
        Vector3 originalScale = transform.localScale;
        
        float elapsedTime = 0f;
        float duration = 0.2f;
        
        while (elapsedTime < duration)
        {
            float scale = Mathf.Lerp(1f, 1.2f, elapsedTime / duration);
            transform.localScale = originalScale * scale;
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        
        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float scale = Mathf.Lerp(1.2f, 0f, elapsedTime / duration);
            transform.localScale = originalScale * scale;
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
    }
}

// Script để gắn vào Button của chuột - KHÔNG CẦN THIẾT NỮA
// Vì đã tích hợp trực tiếp vào MouseTarget

// Script để mở minigame từ game 3D chính
public class MinigameManager : MonoBehaviour
{
    public MouseCatchingMinigame mouseCatchingGame;
    public KeyCode openMinigameKey = KeyCode.M;
    
    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            mouseCatchingGame.OpenMinigame();
        }
    }
    
    // Có thể gọi từ script khác hoặc UI button
    public void OpenMouseCatchingGame()
    {
        mouseCatchingGame.OpenMinigame();
    }
}
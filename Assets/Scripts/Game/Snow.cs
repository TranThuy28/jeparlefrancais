using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ScratchCardGame : MonoBehaviour
{
    [Header("Canvas Setup")]
    public Canvas gameCanvas;
    public Texture2D scratchCard;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI gameOverText;
    public Button resetButton;
    
    [Header("Scratch Settings")]
    public int brushSize = 50;
    public float scratchThreshold = 0.8f; // 80% cào xong thì thắng
    
    public RawImage backgroundImage;
    public RawImage scratchImage;
    private Texture2D scratchTexture;
    private Texture2D backgroundTexture;
    private Color[] originalPixels;
    private bool gameStarted = false;
    private bool gameEnded = false;
    private float scratchedPercentage = 0f;
    private Camera playerCamera;
    
    void Start()
    {
        playerCamera = Camera.main;
        InitializeScratchCard();
        SetupUI();
    }
    
    void InitializeScratchCard()
    {
        // Tạo texture cho lớp tuyết (màu trắng)
        int width = 512;
        int height = 512;
        
        scratchTexture = new Texture2D(width, height);
        backgroundTexture = new Texture2D(width, height);
        
        // Tạo background (hình ảnh ẩn bên dưới)
        Color[] backgroundPixels = new Color[width * height];
        for (int i = 0; i < backgroundPixels.Length; i++)
        {
            // Tạo pattern colorful cho background
            int x = i % width;
            int y = i / width;
            float r = Mathf.Sin(x * 0.01f) * 0.5f + 0.5f;
            float g = Mathf.Sin(y * 0.01f) * 0.5f + 0.5f;
            float b = Mathf.Sin((x + y) * 0.01f) * 0.5f + 0.5f;
            backgroundPixels[i] = new Color(r, g, b, 1f);
        }
        backgroundTexture.SetPixels(backgroundPixels);
        backgroundTexture.Apply();
        
        // Tạo lớp tuyết (màu trắng, alpha = 1)
        Color[] snowPixels = new Color[width * height];
        for (int i = 0; i < snowPixels.Length; i++)
        {
            snowPixels[i] = Color.white; // Lớp tuyết trắng
        }
        
        scratchTexture.SetPixels(snowPixels);
        scratchTexture.Apply();
        
        // Lưu trữ pixels gốc
        originalPixels = scratchTexture.GetPixels();
        
        // Gán texture trực tiếp
        scratchCard = scratchTexture;
        //backgroundImage.texture = backgroundTexture;
        scratchImage.texture = scratchTexture;
        gameStarted = true;
        Debug.Log("Scratch card initialized - Game started!");
    }
    
    void SetupUI()
    {
        if (progressText != null)
            progressText.text = "Tiến độ: 0%";
            
        if (gameOverText != null)
        {
            gameOverText.text = "";
            gameOverText.gameObject.SetActive(false);
        }
        
        if (resetButton != null)
        {
            resetButton.gameObject.SetActive(false);
            resetButton.onClick.AddListener(ResetGame);
        }
        
    }
    
    void Update()
    {
        if (!gameStarted || gameEnded) return;
        
        // Kiểm tra input chuột hoặc touch
        if (Input.GetMouseButton(0))
        {
            HandleScratch();
        }
    }
    
    void HandleScratch()
    {
        Vector2 mousePos = Input.mousePosition;
        
        // Chuyển đổi tọa độ chuột sang tọa độ texture
        // Giả sử screen tương ứng với texture
        Vector2 textureCoord = new Vector2(
            mousePos.x / Screen.width,
            mousePos.y / Screen.height
        );
        
        // Đảm bảo tọa độ trong phạm vi [0,1]
        textureCoord.x = Mathf.Clamp01(textureCoord.x);
        textureCoord.y = Mathf.Clamp01(textureCoord.y);
        
        ScratchAtPosition(textureCoord);
    }
    
    void ScratchAtPosition(Vector2 textureCoord)
    {
        int textureX = (int)(textureCoord.x * scratchTexture.width);
        int textureY = (int)(textureCoord.y * scratchTexture.height);
        
        // Cào trong vùng brush size
        for (int x = -brushSize/2; x < brushSize/2; x++)
        {
            for (int y = -brushSize/2; y < brushSize/2; y++)
            {
                int pixelX = textureX + x;
                int pixelY = textureY + y;
                
                // Kiểm tra bounds
                if (pixelX >= 0 && pixelX < scratchTexture.width && 
                    pixelY >= 0 && pixelY < scratchTexture.height)
                {
                    // Tạo hiệu ứng brush tròn
                    float distance = Vector2.Distance(Vector2.zero, new Vector2(x, y));
                    if (distance <= brushSize/2)
                    {
                        // Làm trong suốt pixel (cào bỏ tuyết)
                        scratchTexture.SetPixel(pixelX, pixelY, Color.clear);
                    }
                }
            }
        }
        
        scratchTexture.Apply();
        scratchImage.texture = scratchTexture;
        CalculateProgress();
    }
    
    void CalculateProgress()
    {
        Color[] currentPixels = scratchTexture.GetPixels();
        int scratchedPixels = 0;
        int totalPixels = currentPixels.Length;
        
        for (int i = 0; i < currentPixels.Length; i++)
        {
            // Pixel được cào nếu alpha < 0.1
            if (currentPixels[i].a < 0.1f)
            {
                scratchedPixels++;
            }
        }
        
        scratchedPercentage = (float)scratchedPixels / totalPixels;
        
        // Cập nhật UI
        if (progressText != null)
            progressText.text = $"Tiến độ: {(scratchedPercentage * 100):F1}%";
        
        // Kiểm tra điều kiện thắng
        if (scratchedPercentage >= scratchThreshold)
        {
            EndGame();
        }
    }
    
    void EndGame()
    {
        gameEnded = true;
        Debug.Log("Game Over! Đã cào xong!");
        
        if (gameOverText != null)
        {
            gameOverText.text = "Félicitations ! Le passage est terminé.";
            gameOverText.gameObject.SetActive(true);
        }
        
        if (resetButton != null)
        {
            resetButton.gameObject.SetActive(true);
        }
        
        // Có thể thêm hiệu ứng, âm thanh, etc.
        StartCoroutine(ShowBackgroundImage());
    }
    
    IEnumerator ShowBackgroundImage()
    {
        yield return new WaitForSeconds(1f);

        // Hiển thị hoàn toàn background image
        if (scratchImage != null)
            scratchImage.enabled = false;
        scratchCard = backgroundTexture;
    }
    
    public void ResetGame()
    {
        gameEnded = false;
        gameStarted = false;
        scratchedPercentage = 0f;
        
        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);
            
        if (resetButton != null)
            resetButton.gameObject.SetActive(false);
        
        // Khởi tạo lại scratch card
        InitializeScratchCard();
    }
    
    void OnDestroy()
    {
        // Giải phóng memory
        if (scratchTexture != null)
            Destroy(scratchTexture);
        if (backgroundTexture != null)
            Destroy(backgroundTexture);
    }
}

// Script để quản lý 3D environment
public class GameManager3D : MonoBehaviour
{
    [Header("3D Environment")]
    public Transform player;
    public Transform scratchScreen; // 3D object chứa canvas
    public float interactionDistance = 3f;
    
    [Header("UI References")]
    public GameObject scratchGameUI;
    public ScratchCardGame scratchGame;
    
    private bool isNearScreen = false;
    private bool gameActive = false;
    
    void Update()
    {
        CheckPlayerDistance();
        HandleInteraction();
    }
    
    void CheckPlayerDistance()
    {
        if (player != null && scratchScreen != null)
        {
            float distance = Vector3.Distance(player.position, scratchScreen.position);
            isNearScreen = distance <= interactionDistance;
        }
    }
    
    void HandleInteraction()
    {
        if (isNearScreen && Input.GetKeyDown(KeyCode.E) && !gameActive)
        {
            StartScratchGame();
        }
        else if (gameActive && Input.GetKeyDown(KeyCode.Escape))
        {
            EndScratchGame();
        }
    }
    
    void StartScratchGame()
    {
        gameActive = true;
        
        if (scratchGameUI != null)
            scratchGameUI.SetActive(true);
            
        // Khóa movement của player
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log("Minigame bắt đầu!");
    }
    
    void EndScratchGame()
    {
        gameActive = false;
        
        if (scratchGameUI != null)
            scratchGameUI.SetActive(false);
            
        // Mở khóa movement của player
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Debug.Log("Minigame kết thúc!");
    }
    
    void OnGUI()
    {
        if (isNearScreen && !gameActive)
        {
            GUI.Label(new Rect(Screen.width/2 - 100, Screen.height/2, 200, 50), 
                     "Nhấn E để chơi minigame cào tuyết");
        }
    }
}
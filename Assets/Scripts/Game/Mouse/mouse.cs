using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WhackAMoleGame : MonoBehaviour
{
    [Header("Game Settings")]
    public int totalHoles = 9;
    public float gameTime = 60f;
    public float minMoleTime = 0.5f;
    public float maxMoleTime = 2f;
    public float moleShowDuration = 3f;
    
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI gameOverText;
    public Button startButton;
    public Button restartButton;
    public GameObject gamePanel; // Panel chứa toàn bộ game UI
    
    [Header("Game Objects")]
    public GameObject holePrefab;
    public GameObject molePrefab;
    public Transform gameArea;
    public AudioClip hitSound;
    public AudioClip missSound;
    
    [Header("Visual Effects")]
    public ParticleSystem hitEffect;
    public Color[] holeColors = { Color.green, Color.blue, Color.yellow, Color.magenta, Color.cyan };
    
    [Header("Sprites for Images")]
    public Sprite holeSprite;
    public Sprite moleSprite;
    
    private List<MoleHole> holes;
    private int score = 0;
    private float currentTime;
    private bool gameActive = false;
    private AudioSource audioSource;
    private bool gameVisible = false; // Trạng thái hiển thị game

    void Awake()
    {
        //Debug.LogError("aksbjascksajbkcbakjb");
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        holes = new List<MoleHole>();
        SetupGame();
        UpdateUI();

        startButton.onClick.AddListener(StartGame);
        restartButton.onClick.AddListener(RestartGame);
        restartButton.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(false);

        // Ẩn game khi bắt đầu
        if (gamePanel != null)
            gamePanel.SetActive(false);
        gameVisible = false;
        //Debug.LogError("WhackAMole Script đã khởi động!");
    }
    
void Update()
{
    //Debug.LogError("WhackAMole Script đã khởi động!");
    if (Input.GetKeyDown(KeyCode.M))
        {
            //Debug.Log("Phím M được nhấn qua Update!");
            ToggleGame();
        }
}

void ToggleGame()
{
    gameVisible = !gameVisible;
    
    if (gamePanel != null)
    {
        gamePanel.SetActive(gameVisible);
    }
    
    // Nếu đang tắt game và game đang chạy, dừng game
    if (!gameVisible && gameActive)
    {
        EndGame();
    }

    // Thêm bật/tắt chuột
    if (gameVisible)
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;   // thả tự do
    }
    else
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked; // khóa vào giữa màn hình
    }
    
    //Debug.Log("Whack-a-Mole Game: " + (gameVisible ? "ON" : "OFF"));
}

    void SetupGame()
    {
        // Clear existing holes
        foreach (MoleHole hole in holes)
        {
            if (hole != null)
                DestroyImmediate(hole.gameObject);
        }
        holes.Clear();
        
        // Create holes with improved positioning - GIỮ NGUYÊN VỊ TRÍ CŨ
        Vector2 areaSize = new Vector2(10f, 6f);
        List<Vector2> positions = GenerateImprovedPositions(totalHoles, areaSize);
        
        for (int i = 0; i < totalHoles; i++)
        {
            GameObject holeObj = Instantiate(holePrefab, gameArea);

            holeObj.transform.localPosition = new Vector2(positions[i].x * 200, positions[i].y * 200 - 100);

            MoleHole hole = holeObj.GetComponent<MoleHole>();
            if (hole == null)
                hole = holeObj.AddComponent<MoleHole>();
                
            hole.Initialize(this, i, holeColors[i % holeColors.Length]);
            holes.Add(hole);
        }
    }
    
    // GIỮ NGUYÊN CÁC HÀM POSITIONING CŨ
    List<Vector2> GenerateImprovedPositions(int count, Vector2 areaSize)
    {
        List<Vector2> positions = new List<Vector2>();
        
        if (count == 9) // 3x3 grid for 9 holes
        {
            return GenerateGridPositions(3, 3, areaSize);
        }
        else if (count == 12) // 4x3 grid for 12 holes
        {
            return GenerateGridPositions(4, 3, areaSize);
        }
        else if (count == 16) // 4x4 grid for 16 holes
        {
            return GenerateGridPositions(4, 4, areaSize);
        }
        else
        {
            return GenerateRandomPositionsImproved(count, areaSize);
        }
    }
    
    List<Vector2> GenerateGridPositions(int cols, int rows, Vector2 areaSize)
    {
        List<Vector2> positions = new List<Vector2>();
        
        float spacingX = areaSize.x / (cols + 1);
        float spacingY = areaSize.y / (rows + 1);
        
        float startX = -areaSize.x / 2 + spacingX;
        float startY = areaSize.y / 2 - spacingY;
        
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                float x = startX + col * spacingX;
                float y = startY - row * spacingY;
                
                Vector2 randomOffset = new Vector2(
                    Random.Range(-0.3f, 0.3f),
                    Random.Range(-0.3f, 0.3f)
                );
                
                positions.Add(new Vector2(x, y) + randomOffset);
                
                if (positions.Count >= totalHoles) break;
            }
            if (positions.Count >= totalHoles) break;
        }
        
        return positions;
    }
    
    List<Vector2> GenerateRandomPositionsImproved(int count, Vector2 areaSize)
    {
        List<Vector2> positions = new List<Vector2>();
        float minDistance = 2f;
        int maxAttempts = 200;
        
        for (int i = 0; i < count; i++)
        {
            Vector2 newPos = Vector2.zero;
            bool validPosition = false;
            int attempts = 0;
            
            while (!validPosition && attempts < maxAttempts)
            {
                float angle = Random.Range(0f, 2f * Mathf.PI);
                float radius = Random.Range(0.5f, Mathf.Min(areaSize.x, areaSize.y) * 0.4f);
                
                newPos = new Vector2(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius
                );
                
                newPos.x = Mathf.Clamp(newPos.x, -areaSize.x / 2 + 1f, areaSize.x / 2 - 1f);
                newPos.y = Mathf.Clamp(newPos.y, -areaSize.y / 2 + 1f, areaSize.y / 2 - 1f);
                
                validPosition = true;
                foreach (Vector2 existingPos in positions)
                {
                    if (Vector2.Distance(newPos, existingPos) < minDistance)
                    {
                        validPosition = false;
                        break;
                    }
                }
                attempts++;
            }
            
            if (!validPosition)
            {
                newPos = new Vector2(
                    Random.Range(-areaSize.x / 2 + 1f, areaSize.x / 2 - 1f),
                    Random.Range(-areaSize.y / 2 + 1f, areaSize.y / 2 - 1f)
                );
            }
            
            positions.Add(newPos);
        }
        
        return positions;
    }
    
    public void StartGame()
    {
        gameActive = true;
        score = 0;
        currentTime = gameTime;
        startButton.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        
        StartCoroutine(GameLoop());
        StartCoroutine(SpawnMoles());
    }
    
    public void RestartGame()
    {
        gameActive = false;
        StopAllCoroutines();
        
        foreach (MoleHole hole in holes)
        {
            hole.HideMole();
        }
        
        SetupGame();
        
        startButton.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(false);
        
        UpdateUI();
    }
    
    IEnumerator GameLoop()
    {
        while (gameActive && currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateUI();
            yield return null;
        }
        
        EndGame();
    }
    
    IEnumerator SpawnMoles()
    {
        while (gameActive)
        {
            yield return new WaitForSeconds(Random.Range(minMoleTime, maxMoleTime));
            
            if (gameActive)
            {
                List<MoleHole> availableHoles = new List<MoleHole>();
                foreach (MoleHole hole in holes)
                {
                    if (!hole.HasMole())
                        availableHoles.Add(hole);
                }
                
                if (availableHoles.Count > 0)
                {
                    MoleHole selectedHole = availableHoles[Random.Range(0, availableHoles.Count)];
                    selectedHole.ShowMole(moleShowDuration, molePrefab);
                }
            }
        }
    }
    
    public void OnMoleHit(MoleHole hole)
    {
        score += 10;
        UpdateUI();
        
        if (hitSound != null)
            audioSource.PlayOneShot(hitSound);
        
        if (hitEffect != null)
        {
            ParticleSystem effect = Instantiate(hitEffect, hole.transform.position, Quaternion.identity);
            Destroy(effect.gameObject, 2f);
        }
        
        StartCoroutine(ScreenShake());
    }
    
    public void OnMoleMiss()
    {
        if (missSound != null)
            audioSource.PlayOneShot(missSound);
    }
    
    IEnumerator ScreenShake()
    {
        Vector3 originalPos = Camera.main.transform.localPosition;
        float shakeDuration = 0.2f;
        float shakeAmount = 0.1f;
        
        float timer = 0;
        while (timer < shakeDuration)
        {
            Vector2 shake2D = Random.insideUnitCircle * shakeAmount;
            Camera.main.transform.localPosition = originalPos + new Vector3(shake2D.x, shake2D.y, originalPos.z);
            timer += Time.deltaTime;
            yield return null;
        }
        
        Camera.main.transform.localPosition = originalPos;
    }
    
    void EndGame()
    {
        gameActive = false;
        gameOverText.text = $"Game Over!\nScore: {score}";
        gameOverText.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);
        
        foreach (MoleHole hole in holes)
        {
            hole.HideMole();
        }
    }
    
    void UpdateUI()
    {
        scoreText.text = "Score: " + score;
        timeText.text = "Time: " + Mathf.Ceil(currentTime).ToString();
    }
}

public class MoleHole : MonoBehaviour, IPointerClickHandler
{
    private WhackAMoleGame gameManager;
    private GameObject mole;
    private bool hasMole = false;
    private int holeIndex;
    private Image holeImage;
    
    public void Initialize(WhackAMoleGame manager, int index, Color holeColor)
    {
        gameManager = manager;
        holeIndex = index;
        
        // Setup hole visual - CHỈ THAY ĐỔI CÁCH SỬ DỤNG IMAGE THAY VÌ SPRITERENDERER
        holeImage = GetComponent<Image>();

        if (holeImage == null)
            holeImage = gameObject.AddComponent<Image>();
            
        if (manager.holeSprite != null)
            holeImage.sprite = manager.holeSprite;
        // else
        //     holeImage.sprite = CreateHoleSprite();
            
        //holeImage.color = holeColor;
        
        // Đảm bảo có thể click được
        holeImage.raycastTarget = true;
    }
    
    Sprite CreateHoleSprite()
    {
        Texture2D texture = new Texture2D(64, 64);
        Color[] pixels = new Color[64 * 64];
        
        Vector2 center = new Vector2(32, 32);
        float radius = 30f;
        
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);
                
                if (distance <= radius)
                {
                    float alpha = 1f - (distance / radius) * 0.5f;
                    pixels[y * 64 + x] = new Color(0.3f, 0.2f, 0.1f, alpha);
                }
                else
                {
                    pixels[y * 64 + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
    }
    
    public void ShowMole(float duration, GameObject molePrefab = null)
    {
        if (hasMole) return;
        
        hasMole = true;
        
        if (molePrefab != null)
        {
            mole = Instantiate(molePrefab, transform);
            mole.transform.localPosition = Vector3.zero;
        }
        else
        {
            mole = new GameObject("Mole");
            mole.transform.SetParent(transform);
            mole.transform.localPosition = Vector3.zero;
            
            // CHỈ THAY ĐỔI: SỬ DỤNG IMAGE THAY VÌ SPRITERENDERER
            Image moleImage = mole.AddComponent<Image>();
            if (gameManager.moleSprite != null)
                moleImage.sprite = gameManager.moleSprite;
            else
                moleImage.sprite = CreateMoleSprite();
            
            moleImage.color = GetRandomMoleColor();
            moleImage.raycastTarget = true;
        }
        
        // Add click handler
        MoleClickHandler clickHandler = mole.GetComponent<MoleClickHandler>();
        if (clickHandler == null)
            clickHandler = mole.AddComponent<MoleClickHandler>();
        clickHandler.Initialize(this);
        
        // Animate mole appearance
        StartCoroutine(AnimateMole(duration));
    }
    
    Sprite CreateMoleSprite()
    {
        Texture2D texture = new Texture2D(48, 48);
        Color[] pixels = new Color[48 * 48];
        
        Vector2 center = new Vector2(24, 24);
        float radius = 20f;
        
        for (int y = 0; y < 48; y++)
        {
            for (int x = 0; x < 48; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);
                
                if (distance <= radius)
                {
                    float normalizedDistance = distance / radius;
                    Color baseColor = new Color(0.6f, 0.4f, 0.2f, 1f);
                    
                    if (normalizedDistance > 0.7f)
                        baseColor = Color.Lerp(baseColor, Color.black, 0.3f);
                        
                    pixels[y * 48 + x] = baseColor;
                    
                    // Add simple eyes
                    if ((Vector2.Distance(pos, center + new Vector2(-6, 6)) < 2f) ||
                        (Vector2.Distance(pos, center + new Vector2(6, 6)) < 2f))
                    {
                        pixels[y * 48 + x] = Color.black;
                    }
                    
                    // Add nose
                    if (Vector2.Distance(pos, center + new Vector2(0, -2)) < 1.5f)
                    {
                        pixels[y * 48 + x] = new Color(0.8f, 0.3f, 0.3f, 1f);
                    }
                }
                else
                {
                    pixels[y * 48 + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 48, 48), new Vector2(0.5f, 0.5f));
    }
    
    Color GetRandomMoleColor()
    {
        Color[] moleColors = {
            new Color(0.6f, 0.4f, 0.2f), // Brown
            new Color(0.4f, 0.3f, 0.2f), // Dark brown
            new Color(0.7f, 0.5f, 0.3f), // Light brown
            new Color(0.5f, 0.5f, 0.5f)  // Gray
        };
        
        return moleColors[Random.Range(0, moleColors.Length)];
    }
    
    IEnumerator AnimateMole(float duration)
    {
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;
        float animTime = 0.2f;
        float timer = 0;
        
        while (timer < animTime)
        {
            mole.transform.localScale = Vector3.Lerp(startScale, endScale, timer / animTime);
            timer += Time.deltaTime;
            yield return null;
        }
        
        mole.transform.localScale = endScale;
        
        yield return new WaitForSeconds(duration - animTime);
        
        if (hasMole)
        {
            HideMole();
        }
    }
    
    public void OnMoleClicked()
    {
        if (hasMole)
        {
            gameManager.OnMoleHit(this);
            HideMole();
        }
    }
    
    public void HideMole()
    {
        if (mole != null)
        {
            StartCoroutine(AnimateMoleHide());
        }
        hasMole = false;
    }
    
    IEnumerator AnimateMoleHide()
    {
        Vector3 startScale = mole.transform.localScale;
        Vector3 endScale = Vector3.zero;
        float animTime = 0.15f;
        float timer = 0;
        
        while (timer < animTime && mole != null)
        {
            mole.transform.localScale = Vector3.Lerp(startScale, endScale, timer / animTime);
            timer += Time.deltaTime;
            yield return null;
        }
        
        if (mole != null)
        {
            Destroy(mole);
            mole = null;
        }
    }
    
    public bool HasMole()
    {
        return hasMole;
    }
    
    // CHỈ THAY ĐỔI: SỬ DỤNG IPOINTERCLICKHANDLER THAY VÌ ONMOUSEDOWN
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!hasMole)
        {
            gameManager.OnMoleMiss();
        }
    }
}

public class MoleClickHandler : MonoBehaviour, IPointerClickHandler
{
    private MoleHole parentHole;
    
    public void Initialize(MoleHole hole)
    {
        parentHole = hole;
        
        // Đảm bảo có Image component để nhận click
        Image image = GetComponent<Image>();
        if (image != null)
            image.raycastTarget = true;
    }
    
    // CHỈ THAY ĐỔI: SỬ DỤNG IPOINTERCLICKHANDLER THAY VÌ ONMOUSEDOWN
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Mole UI clicked!");
        if (parentHole != null)
        {
            parentHole.OnMoleClicked();
        }
    }
}
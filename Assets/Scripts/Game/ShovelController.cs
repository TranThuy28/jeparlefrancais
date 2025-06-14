using UnityEngine;
using UnityEngine.EventSystems;

public class ShovelController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Shovel Settings")]
    public float moveSpeed = 500f;
    public float collectionRadius = 50f;
    
    private RectTransform rectTransform;
    private SnowGameManager gameManager;
    private bool isDragging = false;
    private Vector2 dragOffset;
    private Canvas canvas;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        
        // Set initial position
        rectTransform.anchoredPosition = Vector2.zero;
    }
    
    void Update()
    {
        if (!gameManager.IsGameActive()) return;
        
        // Keyboard movement
        Vector2 movement = Vector2.zero;
        
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            movement.x -= 1;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            movement.x += 1;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            movement.y += 1;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            movement.y -= 1;
            
        if (movement != Vector2.zero)
        {
            MoveShovel(movement * moveSpeed * Time.deltaTime);
        }
        
        // Check for snow collection
        CollectNearbySnow();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out dragOffset);
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                gameManager.GetGameArea(), eventData.position, eventData.pressEventCamera, out localPoint);
            
            rectTransform.anchoredPosition = localPoint - dragOffset;
            ClampToGameArea();
        }
    }
    
    void MoveShovel(Vector2 movement)
    {
        rectTransform.anchoredPosition += movement;
        ClampToGameArea();
    }
    
    void ClampToGameArea()
    {
        if (gameManager.GetGameArea() == null) return;
        
        Vector2 pos = rectTransform.anchoredPosition;
        RectTransform gameArea = gameManager.GetGameArea();
        
        float halfWidth = rectTransform.rect.width / 2;
        float halfHeight = rectTransform.rect.height / 2;
        
        pos.x = Mathf.Clamp(pos.x, -gameArea.rect.width / 2 + halfWidth, gameArea.rect.width / 2 - halfWidth);
        pos.y = Mathf.Clamp(pos.y, -gameArea.rect.height / 2 + halfHeight, gameArea.rect.height / 2 - halfHeight);
        
        rectTransform.anchoredPosition = pos;
    }
    
    void CollectNearbySnow()
    {
        Snowflake[] allSnow = FindObjectsOfType<Snowflake>();
        
        foreach (Snowflake snow in allSnow)
        {
            if (snow.IsOnGround())
            {
                float distance = Vector2.Distance(rectTransform.anchoredPosition, 
                    snow.GetComponent<RectTransform>().anchoredPosition);
                    
                if (distance <= collectionRadius)
                {
                    gameManager.AddScore(10);
                    gameManager.RemoveSnowFromGround(snow.gameObject);
                    Destroy(snow.gameObject);
                }
            }
        }
    }
    
    public void SetGameManager(SnowGameManager manager)
    {
        gameManager = manager;
    }
}


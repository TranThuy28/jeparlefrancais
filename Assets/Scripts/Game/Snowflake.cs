// Snowflake.cs - Individual snowflake behavior
using UnityEngine;

public class Snowflake : MonoBehaviour
{
    private RectTransform rectTransform;
    private SnowGameManager gameManager;
    private float fallSpeed = 50f;
    private bool onGround = false;
    private float groundTimer = 0f;
    private float maxGroundTime = 10f;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    
    void Update()
    {
        if (!gameManager.IsGameActive()) return;
        
        if (!onGround)
        {
            // Fall down
            rectTransform.anchoredPosition += Vector2.down * fallSpeed * Time.deltaTime;
            
            // Check if hit ground
            if (rectTransform.anchoredPosition.y <= -gameManager.GetGameArea().rect.height / 2)
            {
                LandOnGround();
            }
        }
        else
        {
            // Timer for snow on ground
            groundTimer += Time.deltaTime;
            if (groundTimer >= maxGroundTime)
            {
                // Snow melts after being on ground too long
                gameManager.RemoveSnowFromGround(gameObject);
                Destroy(gameObject);
            }
        }
    }
    
    void LandOnGround()
    {
        onGround = true;
        gameManager.AddSnowToGround(gameObject);
        
        // Clamp to ground level
        Vector2 pos = rectTransform.anchoredPosition;
        pos.y = -gameManager.GetGameArea().rect.height / 2 + rectTransform.rect.height / 2;
        rectTransform.anchoredPosition = pos;
    }
    
    public void SetGameManager(SnowGameManager manager)
    {
        gameManager = manager;
    }
    
    public void SetFallSpeed(float speed)
    {
        fallSpeed = speed;
    }
    
    public bool IsOnGround()
    {
        return onGround;
    }
}
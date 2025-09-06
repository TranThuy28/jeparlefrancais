using System;
using UnityEngine;
using UnityEngine.UI;

namespace InventoryPlus
{
    public class ItemCollectable : MonoBehaviour
    {
        [Header("Item Data")]
        public Item itemData;
        public int amount = 1;
        
        [Header("UI Settings")]
        public Font textFont; // Assign font trong Inspector (optional)
        public int fontSize = 18;
        public Color textColor = Color.white;
        public Color backgroundColor = new Color(0, 0, 0, 0.8f); // Màu nền semi-transparent
        public Vector3 uiOffset = new Vector3(0, 2f, 0); // Offset từ item position
        
        [Header("Settings")]
        public float detectionRadius = 3f;
        public KeyCode collectKey = KeyCode.C;
        
        private Transform player;
        private Inventory playerInventory;
        private bool isPlayerNear = false;
        private bool isMouseOver = false;
        
        // UI Components
        private Canvas collectUI;
        private Text collectText;
        private Image backgroundImage;
        private GameObject uiObject;
        
        // Events
        public static event Action<Item, int> OnItemCollected;
        
        void Start()
        {
            // Tìm player
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

            playerInventory = FindAnyObjectByType<Inventory>(FindObjectsInactive.Include);
            
            Debug.Log(player == null ? "Player not found. Please ensure the player has the 'Player' tag." : "Player found.");
            
            // Tạo UI động cho item này
            CreateCollectUI();
        }
        
        void CreateCollectUI()
        {
            // Tạo GameObject chính cho UI
            uiObject = new GameObject($"CollectUI_{gameObject.name}");
            uiObject.transform.position = transform.position + uiOffset;
            uiObject.transform.SetParent(transform); // Make it child của item để follow item
            
            // Thêm Canvas (World Space)
            collectUI = uiObject.AddComponent<Canvas>();
            collectUI.renderMode = RenderMode.WorldSpace;
            collectUI.worldCamera = Camera.main;
            collectUI.sortingOrder = 100; // Đảm bảo hiện trên các UI khác
            
            // Thêm CanvasScaler để scale đúng
            CanvasScaler scaler = uiObject.AddComponent<CanvasScaler>();
            //scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantWorldSize;
            scaler.referenceResolution = new Vector2(800, 600);
            
            // Thêm GraphicRaycaster để detect mouse events
            uiObject.AddComponent<GraphicRaycaster>();
            
            // Set kích thước Canvas
            RectTransform canvasRect = collectUI.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(3, 0.8f);
            
            // Tạo background panel
            CreateBackgroundPanel();
            
            // Tạo text
            CreateCollectText();
            
            // Ẩn UI ban đầu
            uiObject.SetActive(false);
        }
        
        void CreateBackgroundPanel()
        {
            // Tạo GameObject cho background
            GameObject backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(uiObject.transform, false);
            
            // Thêm Image component
            backgroundImage = backgroundObject.AddComponent<Image>();
            backgroundImage.color = backgroundColor;
            backgroundImage.sprite = CreateBackgroundSprite();
            
            // Set RectTransform
            RectTransform bgRect = backgroundObject.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            bgRect.anchoredPosition = Vector2.zero;
        }

        void CreateCollectText()
        {
            // Tạo GameObject cho text
            GameObject textObject = new GameObject("CollectText");
            textObject.transform.SetParent(uiObject.transform, false);

            // Thêm Text component
            collectText = textObject.AddComponent<Text>();
            textObject.transform.localScale = Vector3.one * 0.01f; // Scale nhỏ để phù hợp với World Space Canvas

            // Setup font
            if (textFont != null)
            {
                collectText.font = textFont;
            }
            else
            {
                collectText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            // Setup text properties
            collectText.fontSize = fontSize;
            collectText.color = textColor;
            collectText.alignment = TextAnchor.MiddleCenter;
            collectText.horizontalOverflow = HorizontalWrapMode.Overflow;
            collectText.verticalOverflow = VerticalWrapMode.Overflow;

            // Set text content
            UpdateTextContent();

            // Set RectTransform
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            // Add padding
            //textRect.offsetMin = new Vector2(10, 10); // Padding left, bottom
            //textRect.offsetMax = new Vector2(-10, -10); // Padding right, top
            
        }
        
        Sprite CreateBackgroundSprite()
        {
            // Tạo texture 1x1 pixel
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            
            // Tạo sprite từ texture
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }
        
        void UpdateTextContent()
        {
            if (collectText != null && itemData != null)
            {
                collectText.text = $"Appuyez sur [{collectKey}] pour ramasser";
            }
        }
        
        void Update()
        {
            CheckPlayerDistance();
            HandleInput();
            UpdateUIRotation();
        }
        
        void CheckPlayerDistance()
        {
            if (player == null) return;
            
            float distance = Vector3.Distance(transform.position, player.position);
            isPlayerNear = distance <= detectionRadius;
            
            UpdateUIVisibility();
        }
        
        void UpdateUIVisibility()
        {
            // Bỏ điều kiện isMouseOver vì chuột có thể bị tắt
            bool shouldShowUI = isPlayerNear;
            
            if (uiObject != null)
            {
                uiObject.SetActive(shouldShowUI);
            }
        }
        
        void UpdateUIRotation()
        {
            // Làm cho UI luôn quay về phía camera
            if (uiObject != null && uiObject.activeInHierarchy && Camera.main != null)
            {
                uiObject.transform.LookAt(Camera.main.transform);
                uiObject.transform.Rotate(0, 180, 0); // Flip để text không bị ngược
            }
        }
        
        void HandleInput()
        {
            if (isPlayerNear && Input.GetKeyDown(collectKey))
            {
                CollectItem();
            }
        }
        
        void CollectItem()
        {
            // Trigger collection event
            OnItemCollected?.Invoke(itemData, amount);
            
            // Play audio if available
            if (itemData != null && itemData.useAudio != null)
            {
                AudioSource.PlayClipAtPoint(itemData.useAudio, transform.position);
            }
            
            playerInventory.AddInventory(itemData, amount, 0f, false);
            // Destroy UI object
            if (uiObject != null)
            {
                Destroy(uiObject);
            }
            
            // Destroy item object
            Destroy(gameObject);
        }
        
        // Mouse events (optional - có thể bỏ nếu chuột bị tắt)
        void OnMouseEnter()
        {
            isMouseOver = true;
        }
        
        void OnMouseExit()
        {
            isMouseOver = false;
        }
        
        // Cleanup khi destroy
        void OnDestroy()
        {
            if (uiObject != null)
            {
                Destroy(uiObject);
            }
        }
        
        // Gizmos để debug
        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            
            // Draw UI position
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position + uiOffset, new Vector3(3f, 0.8f, 0.1f));
        }
        
        // Public methods để customize từ bên ngoài
        public void SetTextColor(Color color)
        {
            textColor = color;
            if (collectText != null)
            {
                collectText.color = color;
            }
        }
        
        public void SetBackgroundColor(Color color)
        {
            backgroundColor = color;
            if (backgroundImage != null)
            {
                backgroundImage.color = color;
            }
        }
        
        public void SetUIOffset(Vector3 offset)
        {
            uiOffset = offset;
            if (uiObject != null)
            {
                uiObject.transform.localPosition = offset;
            }
        }
    }
}
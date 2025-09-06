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
        
        [Header("Auto-Generated UI Settings")]
        public bool autoCreateUI = true;
        public Vector3 uiOffset = new Vector3(0, 2f, 0);
        public Color textColor = Color.white;
        public Color backgroundColor = new Color(0, 0, 0, 0.7f);
        public int fontSize = 14;
        
        [Header("Manual UI References (Optional)")]
        public Canvas collectUI;
        public Text collectText;
        
        [Header("Settings")]
        public float detectionRadius = 10f;
        public KeyCode collectKey = KeyCode.C;
        
        private Transform player;
        private bool isPlayerNear = false;
        private bool isMouseOver = false;
        private Camera mainCamera;
        
        // Events
        public static event Action<Item, int> OnItemCollected;
        
        void Start()
        {
            // Tìm player và camera
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            mainCamera = Camera.main;
            
            Debug.Log(player == null ? "Player not found. Please ensure the player has the 'Player' tag." : "Player found.");
            
            // Tự động tạo UI nếu được bật và chưa có UI manual
            if (autoCreateUI && (collectUI == null || collectText == null))
            {
                CreateAutoUI();
            }
            else
            {
                SetupManualUI();
            }
        }
        
        void CreateAutoUI()
        {
            // Tạo Canvas object
            GameObject canvasObj = new GameObject($"CollectUI_{itemData?.itemName ?? "Item"}");
            canvasObj.transform.SetParent(transform);
            canvasObj.transform.localPosition = uiOffset;
            
            // Setup Canvas component
            collectUI = canvasObj.AddComponent<Canvas>();
            collectUI.renderMode = RenderMode.WorldSpace;
            collectUI.worldCamera = mainCamera;
            
            // Thiết lập kích thước canvas
            RectTransform canvasRect = collectUI.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(200, 50);
            canvasRect.localScale = Vector3.one * 0.01f; // Scale nhỏ lại cho phù hợp
            
            // Tạo background panel
            GameObject panelObj = new GameObject("Background");
            panelObj.transform.SetParent(canvasObj.transform, false);
            
            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = backgroundColor;
            
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            panelRect.anchoredPosition = Vector2.zero;
            
            // Tạo Text object
            GameObject textObj = new GameObject("CollectText");
            textObj.transform.SetParent(panelObj.transform, false);
            
            collectText = textObj.AddComponent<Text>();
            collectText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            collectText.text = $"Press {collectKey} to collect {itemData?.itemName ?? "Item"}";
            collectText.fontSize = fontSize;
            collectText.color = textColor;
            collectText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform textRect = collectText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            // Thêm Graphic Raycaster để canvas có thể nhận input
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Ẩn UI ban đầu
            collectUI.gameObject.SetActive(false);
            
            Debug.Log($"Auto-generated UI created for {itemData?.itemName ?? "Item"}");
        }
        
        void SetupManualUI()
        {
            if (collectUI != null)
            {
                collectUI.gameObject.SetActive(false);
                collectUI.worldCamera = mainCamera;
            }
            
            if (collectText != null)
            {
                collectText.text = $"Press {collectKey} to collect {itemData?.itemName ?? "Item"}";
            }
        }
        
        void Update()
        {
            CheckPlayerDistance();
            HandleInput();
            UpdateUIRotation();
        }
        
        void UpdateUIRotation()
        {
            // Làm cho UI luôn hướng về phía camera
            if (collectUI != null && mainCamera != null && collectUI.gameObject.activeInHierarchy)
            {
                collectUI.transform.LookAt(collectUI.transform.position + mainCamera.transform.rotation * Vector3.forward,
                                         mainCamera.transform.rotation * Vector3.up);
            }
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
            bool shouldShowUI = isPlayerNear && isMouseOver;
            
            if (collectUI != null)
            {
                collectUI.gameObject.SetActive(shouldShowUI);
            }
        }
        
        void HandleInput()
        {
            if (isPlayerNear && isMouseOver && Input.GetKeyDown(collectKey))
            {
                CollectItem();
            }
        }
        
        void CollectItem()
        {
            // Trigger collection event
            OnItemCollected?.Invoke(itemData, amount);
            
            // Play audio if available
            if (itemData?.useAudio != null)
            {
                AudioSource.PlayClipAtPoint(itemData.useAudio, transform.position);
            }
            
            // Hide UI and destroy object
            if (collectUI != null)
            {
                collectUI.gameObject.SetActive(false);
            }
            
            Debug.Log($"Collected {amount}x {itemData?.itemName ?? "Item"}");
            Destroy(gameObject);
        }
        
        // Mouse events
        void OnMouseEnter()
        {
            isMouseOver = true;
            UpdateUIVisibility();
        }
        
        void OnMouseExit()
        {
            isMouseOver = false;
            UpdateUIVisibility();
        }
        
        // Gizmos để debug
        public void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            
            // Vẽ vị trí UI offset
            if (autoCreateUI)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(transform.position + uiOffset, Vector3.one * 0.5f);
            }
        }
        
        // Method để cập nhật text runtime
        public void UpdateCollectText(string newText)
        {
            if (collectText != null)
            {
                collectText.text = newText;
            }
        }
        
        // Method để thay đổi màu UI runtime
        public void SetUIColors(Color textCol, Color bgCol)
        {
            textColor = textCol;
            backgroundColor = bgCol;
            
            if (collectText != null)
                collectText.color = textColor;
                
            Image bgImage = collectUI?.GetComponentInChildren<Image>();
            if (bgImage != null)
                bgImage.color = backgroundColor;
        }
    }
}
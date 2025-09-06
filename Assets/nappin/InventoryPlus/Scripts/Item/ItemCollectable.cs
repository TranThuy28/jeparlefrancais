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
        
        [Header("UI References")]
        public Canvas collectUI;
        public Text collectText;
        
        [Header("Settings")]
        public float detectionRadius = 10f;
        public KeyCode collectKey = KeyCode.C;
        
        private Transform player;
        private bool isPlayerNear = false;
        private bool isMouseOver = false;
        
        // Events
        public static event Action<Item, int> OnItemCollected;
        
        void Start()
        {
            // Tìm player (có thể thay đổi tag theo project của bạn)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            Debug.Log(player == null ? "Player not found. Please ensure the player has the 'Player' tag." : "Player found.");
            
            // Setup UI
            if (collectUI != null)
            {
                collectUI.gameObject.SetActive(false);
                collectUI.worldCamera = Camera.main;
            }
            
            if (collectText != null)
            {
                collectText.text = $"Press {collectKey} to collect {itemData.itemName}";
            }
        }
        
        void Update()
        {
            CheckPlayerDistance();
            HandleInput();
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
            if (itemData.useAudio != null)
            {
                AudioSource.PlayClipAtPoint(itemData.useAudio, transform.position);
            }
            
            // Hide UI and destroy object
            if (collectUI != null)
            {
                collectUI.gameObject.SetActive(false);
            }
            
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
        }
    }
}
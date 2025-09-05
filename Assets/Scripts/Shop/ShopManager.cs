using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace InventoryPlus
{
    // Enum for currency type
    public enum CurrencyType
    {
        Gems,
        Coins
    }

    // Manage gems and shop
    public class ShopManager : MonoBehaviour
    {
        [Header("Shop Settings")]
        public List<Item> allAvailableItems = new List<Item>(); // Pool of all possible items
        public List<Item> shopItems = new List<Item>(); // Currently displayed items
        public Transform shopItemContainer;
        public GameObject shopItemPrefab;

        [Header("Daily Shop Settings")]
        public int dailyItemCount = 6; // Number of items to spawn daily
        public bool enableDailyRefresh = true;
        public int refreshHour = 0; // Hour to refresh (0 = midnight)
        
        [Header("Purchase Dialog")]
        public GameObject purchaseDialog;
        public TextMeshProUGUI itemNameText;
        public Image itemIconImage;
        public TextMeshProUGUI gemsPriceText;
        public TextMeshProUGUI coinsPriceText;
        public Slider quantitySlider;
        public TextMeshProUGUI quantityDisplayText;
        public Button minusButton;
        public Button plusButton;
        public TextMeshProUGUI totalGemsText;
        public TextMeshProUGUI totalCoinsText;
        public Button buyWithCoinsButton;
        public Button cancelButton;

        [Header("Quantity Settings")]
        public int minQuantity = 1;
        public int maxQuantity = 100;

        [Header("Shop UI")]
        public TextMeshProUGUI nextRefreshText; // Optional: Display next refresh time

        private Item currentItem;
        private int currentQuantity = 1;
        private GameManager gameManager;
        public Inventory inventory;

        // Daily refresh system
        private DateTime lastRefreshDate;
        private float checkInterval = 60f; // Check every minute
        private float timeSinceLastCheck = 0f;

        void Start()
        {
            gameManager = FindFirstObjectByType<GameManager>();
            LoadLastRefreshDate();
            CheckForDailyRefresh();
            SetupPurchaseDialog();
            RefreshDailyItems();
        }

        void Update()
        {
            if (enableDailyRefresh)
            {
                timeSinceLastCheck += Time.deltaTime;
                
                if (timeSinceLastCheck >= checkInterval)
                {
                    timeSinceLastCheck = 0f;
                    CheckForDailyRefresh();
                    UpdateNextRefreshDisplay();
                }
            }
        }

        void LoadLastRefreshDate()
        {
            string lastRefreshString = PlayerPrefs.GetString("ShopLastRefresh", "");
            
            if (string.IsNullOrEmpty(lastRefreshString))
            {
                // First time - set to yesterday to trigger immediate refresh
                lastRefreshDate = DateTime.Now.AddDays(-1);
            }
            else
            {
                if (DateTime.TryParse(lastRefreshString, out DateTime savedDate))
                {
                    lastRefreshDate = savedDate;
                }
                else
                {
                    lastRefreshDate = DateTime.Now.AddDays(-1);
                }
            }
        }

        void SaveLastRefreshDate()
        {
            PlayerPrefs.SetString("ShopLastRefresh", DateTime.Now.ToString());
            PlayerPrefs.Save();
        }

        void CheckForDailyRefresh()
        {
            DateTime now = DateTime.Now;
            DateTime todayRefreshTime = new DateTime(now.Year, now.Month, now.Day, refreshHour, 0, 0);
            
            // If we haven't refreshed today and it's past refresh time
            if (lastRefreshDate.Date < now.Date && now >= todayRefreshTime)
            {
                RefreshDailyItems();
                lastRefreshDate = now;
                SaveLastRefreshDate();
            }
            // If it's past refresh time and we refreshed before refresh time today
            else if (lastRefreshDate.Date == now.Date && lastRefreshDate < todayRefreshTime && now >= todayRefreshTime)
            {
                RefreshDailyItems();
                lastRefreshDate = now;
                SaveLastRefreshDate();
            }
        }

        void RefreshDailyItems()
        {
            Debug.Log("🔄 Refreshing daily shop items!");
            
            // Clear current shop items
            shopItems.Clear();
            
            // Get random items from available pool
            if (allAvailableItems.Count > 0)
            {
                List<Item> availableCopy = new List<Item>(allAvailableItems);
                int itemsToAdd = Mathf.Min(dailyItemCount, availableCopy.Count);
                
                for (int i = 0; i < itemsToAdd; i++)
                {
                    int randomIndex = UnityEngine.Random.Range(0, availableCopy.Count);
                    shopItems.Add(availableCopy[randomIndex]);
                    availableCopy.RemoveAt(randomIndex); // Prevent duplicates
                }
            }
            
            // Reinitialize the shop UI
            InitializeShop();
        }

        void InitializeShop()
        {
            foreach (Transform child in shopItemContainer)
            {
                Destroy(child.gameObject); // Clear existing items
            }
            
            foreach (Item item in shopItems)
            {
                CreateShopItemUI(item);
            }
        }

        void CreateShopItemUI(Item item)
        {
            GameObject itemUI = Instantiate(shopItemPrefab, shopItemContainer);
            ShopItemUI shopItemUI = itemUI.GetComponent<ShopItemUI>();
            shopItemUI.SetupItem(item, this);
        }

        void SetupPurchaseDialog()
        {
            purchaseDialog.SetActive(false);

            // Setup slider
            quantitySlider.minValue = minQuantity;
            quantitySlider.maxValue = maxQuantity;
            quantitySlider.value = minQuantity;
            quantitySlider.wholeNumbers = true;

            // Add listeners
            minusButton.onClick.AddListener(() => ChangeQuantity(-1));
            plusButton.onClick.AddListener(() => ChangeQuantity(1));
            buyWithCoinsButton.onClick.AddListener(() => ConfirmPurchase(CurrencyType.Coins));
            cancelButton.onClick.AddListener(ClosePurchaseDialog);

            quantitySlider.onValueChanged.AddListener(OnQuantitySliderChanged);
        }

        void UpdateNextRefreshDisplay()
        {
            if (nextRefreshText != null)
            {
                DateTime now = DateTime.Now;
                DateTime nextRefresh = new DateTime(now.Year, now.Month, now.Day, refreshHour, 0, 0);
                
                if (now >= nextRefresh)
                {
                    nextRefresh = nextRefresh.AddDays(1);
                }
                
                TimeSpan timeUntilRefresh = nextRefresh - now;
                nextRefreshText.text = $"Next Refresh: {timeUntilRefresh.Hours:00}:{timeUntilRefresh.Minutes:00}:{timeUntilRefresh.Seconds:00}";
            }
        }

        public void OpenPurchaseDialog(Item item)
        {
//            Debug.Log($"Opening purchase dialog for {item.itemName}");
            currentItem = item;
            currentQuantity = minQuantity;

            purchaseDialog.SetActive(true);
//            Debug.Log($"{purchaseDialog.activeSelf} - {item.itemName}");
            itemNameText.text = item.itemName;
            itemIconImage.sprite = item.itemSprite;
            coinsPriceText.text = $"{item.sellingPrice} coins/item";

            // Reset slider
            quantitySlider.value = minQuantity;
            
            UpdatePurchaseDialog();
        }

        void ChangeQuantity(int change)
        {
            int newQuantity = currentQuantity + change;
            newQuantity = Mathf.Clamp(newQuantity, minQuantity, maxQuantity);
            
            quantitySlider.value = newQuantity;
            // OnQuantitySliderChanged will be called automatically
        }

        void OnQuantitySliderChanged(float value)
        {
            currentQuantity = Mathf.RoundToInt(value);
            UpdatePurchaseDialog();
        }

        void UpdatePurchaseDialog()
        {
            // Update quantity display text
            quantityDisplayText.text = $"Quantity: {currentQuantity}";

            int totalCoinsPrice = currentItem.sellingPrice * currentQuantity;
            totalCoinsText.text = $"Total: {totalCoinsPrice} coins";

            // Check if player has enough money
            buyWithCoinsButton.interactable = gameManager.currencyManager.coins >= totalCoinsPrice;

            // Update +/- button states
            minusButton.interactable = currentQuantity > minQuantity;
            plusButton.interactable = currentQuantity < maxQuantity;

            // Update button colors to show state
            UpdateButtonColors();
        }

        void UpdateButtonColors()
        {
            int totalCoinsPrice = currentItem.sellingPrice * currentQuantity;

            // Green color if enough money, gray if not enough
            buyWithCoinsButton.GetComponent<Image>().color = gameManager.currencyManager.coins >= totalCoinsPrice ? new Color(1f, 0.8f, 0f) : Color.gray;
        }

        void ConfirmPurchase(CurrencyType currencyType)
        {
            int totalPrice = currentItem.sellingPrice * currentQuantity;
            string currencyName = "coins";

            if (gameManager.currencyManager.coins >= totalPrice)
            {
                gameManager.currencyManager.AddCoins(-totalPrice);
                inventory.AddInventory(currentItem, currentQuantity, 0, false);
                Debug.Log($"✅ Purchased {currentQuantity}x {currentItem.itemName} for {totalPrice}");
                ClosePurchaseDialog();
            }
            else
            {
                Debug.Log($"❌ Not enough {currencyName}...");
            }
        }

        void ClosePurchaseDialog()
        {
            purchaseDialog.SetActive(false);
            currentItem = null;
        }

        // Manual refresh for testing
        [ContextMenu("Force Refresh Shop")]
        public void ForceRefreshShop()
        {
            RefreshDailyItems();
            lastRefreshDate = DateTime.Now;
            SaveLastRefreshDate();
        }

        // Cheat functions for testing
        [ContextMenu("Add 500 Gems")]
        public void AddGems()
        {
            gameManager.currencyManager.AddGems(500);
        }

        [ContextMenu("Add 300 Coins")]
        public void AddCoins()
        {
            gameManager.currencyManager.AddCoins(300);
        }

        // Debug function to check refresh status
        [ContextMenu("Check Refresh Status")]
        public void CheckRefreshStatus()
        {
            Debug.Log($"Last Refresh: {lastRefreshDate}");
            Debug.Log($"Current Time: {DateTime.Now}");
            Debug.Log($"Current Shop Items: {shopItems.Count}");
        }
    }
}
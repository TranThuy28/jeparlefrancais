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

    // Gérer les gemmes et la boutique
    public class ShopManager : MonoBehaviour
    {
        [Header("Paramètres de la Boutique")]
        public List<Item> allAvailableItems = new List<Item>(); // Pool de tous les objets possibles
        public List<Item> shopItems = new List<Item>(); // Objets actuellement affichés
        public Transform shopItemContainer;
        public GameObject shopItemPrefab;

        [Header("Paramètres de la Boutique Quotidienne")]
        public int dailyItemCount = 6; // Nombre d'objets à générer quotidiennement
        public bool enableDailyRefresh = true;
        public int refreshHour = 0; // Heure de rafraîchissement (0 = minuit)
        
        [Header("Dialogue d'Achat")]
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

        [Header("Paramètres de Quantité")]
        public int minQuantity = 1;
        public int maxQuantity = 100;

        [Header("Interface de la Boutique")]
        public TextMeshProUGUI nextRefreshText; // Optionnel : Afficher le temps jusqu'au prochain rafraîchissement

        private Item currentItem;
        private int currentQuantity = 1;
        private GameManager gameManager;
        public Inventory inventory;

        // Système de rafraîchissement quotidien
        private DateTime lastRefreshDate;
        private float checkInterval = 60f; // Vérifier toutes les minutes
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
                // Première fois - définir à hier pour déclencher un rafraîchissement immédiat
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
            
            // Si on n'a pas rafraîchi aujourd'hui et c'est passé l'heure de rafraîchissement
            if (lastRefreshDate.Date < now.Date && now >= todayRefreshTime)
            {
                RefreshDailyItems();
                lastRefreshDate = now;
                SaveLastRefreshDate();
            }
            // Si c'est passé l'heure de rafraîchissement et on a rafraîchi avant l'heure de rafraîchissement aujourd'hui
            else if (lastRefreshDate.Date == now.Date && lastRefreshDate < todayRefreshTime && now >= todayRefreshTime)
            {
                RefreshDailyItems();
                lastRefreshDate = now;
                SaveLastRefreshDate();
            }
        }

        void RefreshDailyItems()
        {
            Debug.Log("🔄 Rafraîchissement des objets de la boutique quotidienne !");
            
            // Vider les objets actuels de la boutique
            shopItems.Clear();
            
            // Obtenir des objets aléatoires du pool disponible
            if (allAvailableItems.Count > 0)
            {
                List<Item> availableCopy = new List<Item>(allAvailableItems);
                int itemsToAdd = Mathf.Min(dailyItemCount, availableCopy.Count);
                
                for (int i = 0; i < itemsToAdd; i++)
                {
                    int randomIndex = UnityEngine.Random.Range(0, availableCopy.Count);
                    shopItems.Add(availableCopy[randomIndex]);
                    availableCopy.RemoveAt(randomIndex); // Éviter les doublons
                }
            }
            
            // Réinitialiser l'interface de la boutique
            InitializeShop();
        }

        void InitializeShop()
        {
            foreach (Transform child in shopItemContainer)
            {
                Destroy(child.gameObject); // Effacer les objets existants
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

            // Configurer le curseur
            quantitySlider.minValue = minQuantity;
            quantitySlider.maxValue = maxQuantity;
            quantitySlider.value = minQuantity;
            quantitySlider.wholeNumbers = true;

            // Ajouter les écouteurs
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
                nextRefreshText.text = $"Prochain Rafraîchissement : {timeUntilRefresh.Hours:00}:{timeUntilRefresh.Minutes:00}:{timeUntilRefresh.Seconds:00}";
            }
        }

        public void OpenPurchaseDialog(Item item)
        {
//            Debug.Log($"Ouverture du dialogue d'achat pour {item.itemName}");
            currentItem = item;
            currentQuantity = minQuantity;

            purchaseDialog.SetActive(true);
//            Debug.Log($"{purchaseDialog.activeSelf} - {item.itemName}");
            itemNameText.text = item.itemName;
            itemIconImage.sprite = item.itemSprite;
            coinsPriceText.text = $"{item.sellingPrice} pièces/objet";

            // Réinitialiser le curseur
            quantitySlider.value = minQuantity;
            
            UpdatePurchaseDialog();
        }

        void ChangeQuantity(int change)
        {
            int newQuantity = currentQuantity + change;
            newQuantity = Mathf.Clamp(newQuantity, minQuantity, maxQuantity);
            
            quantitySlider.value = newQuantity;
            // OnQuantitySliderChanged sera appelé automatiquement
        }

        void OnQuantitySliderChanged(float value)
        {
            currentQuantity = Mathf.RoundToInt(value);
            UpdatePurchaseDialog();
        }

        void UpdatePurchaseDialog()
        {
            // Mettre à jour le texte d'affichage de la quantité
            quantityDisplayText.text = $"Quantité : {currentQuantity}";

            int totalCoinsPrice = currentItem.sellingPrice * currentQuantity;
            totalCoinsText.text = $"Total : {totalCoinsPrice} pièces";

            // Vérifier si le joueur a assez d'argent
            buyWithCoinsButton.interactable = gameManager.currencyManager.coins >= totalCoinsPrice;

            // Mettre à jour l'état des boutons +/-
            minusButton.interactable = currentQuantity > minQuantity;
            plusButton.interactable = currentQuantity < maxQuantity;

            // Mettre à jour les couleurs des boutons pour montrer l'état
            UpdateButtonColors();
        }

        void UpdateButtonColors()
        {
            int totalCoinsPrice = currentItem.sellingPrice * currentQuantity;

            // Couleur verte si assez d'argent, grise sinon
            buyWithCoinsButton.GetComponent<Image>().color = gameManager.currencyManager.coins >= totalCoinsPrice ? new Color(1f, 0.8f, 0f) : Color.gray;
        }

        void ConfirmPurchase(CurrencyType currencyType)
        {
            int totalPrice = currentItem.sellingPrice * currentQuantity;
            string currencyName = "pièces";

            if (gameManager.currencyManager.coins >= totalPrice)
            {
                gameManager.currencyManager.AddCoins(-totalPrice);
                inventory.AddInventory(currentItem, currentQuantity, 0, false);
                Debug.Log($"✅ Acheté {currentQuantity}x {currentItem.itemName} pour {totalPrice} pièces");
                ClosePurchaseDialog();
            }
            else
            {
                Debug.Log($"❌ Pas assez de {currencyName}...");
            }
        }

        void ClosePurchaseDialog()
        {
            purchaseDialog.SetActive(false);
            currentItem = null;
        }

        // Rafraîchissement manuel pour les tests
        [ContextMenu("Forcer le Rafraîchissement de la Boutique")]
        public void ForceRefreshShop()
        {
            RefreshDailyItems();
            lastRefreshDate = DateTime.Now;
            SaveLastRefreshDate();
        }

        // Fonctions de triche pour les tests
        [ContextMenu("Ajouter 500 Gemmes")]
        public void AddGems()
        {
            gameManager.currencyManager.AddGems(500);
        }

        [ContextMenu("Ajouter 300 Pièces")]
        public void AddCoins()
        {
            gameManager.currencyManager.AddCoins(300);
        }

        // Fonction de débogage pour vérifier le statut de rafraîchissement
        [ContextMenu("Vérifier le Statut de Rafraîchissement")]
        public void CheckRefreshStatus()
        {
            Debug.Log($"Dernier Rafraîchissement : {lastRefreshDate}");
            Debug.Log($"Heure Actuelle : {DateTime.Now}");
            Debug.Log($"Objets Actuels de la Boutique : {shopItems.Count}");
        }
    }
}
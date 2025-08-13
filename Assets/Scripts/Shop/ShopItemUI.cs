using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace InventoryPlus
{
    // UI component cho từng item trong shop
    public class ShopItemUI : MonoBehaviour
    {
        [Header("UI References")]
        public Image itemIcon;
        public TextMeshProUGUI itemNameText;
        public TextMeshProUGUI gemsPriceText;
        public TextMeshProUGUI coinsPriceText;
        public Button buyButton;

        private Item item;
        private ShopManager shopManager;

        public void SetupItem(Item shopItem, ShopManager manager)
        {
            item = shopItem;
            shopManager = manager;

            itemIcon.sprite = item.itemSprite;
            itemNameText.text = item.itemName;
//            gemsPriceText.text = $"{item.sellingPrice} 💎";
            coinsPriceText.text = $"{item.sellingPrice}";

            buyButton.onClick.AddListener(OnBuyButtonClicked);
        }

        void OnBuyButtonClicked()
        {
            Debug.Log($"Buying item: {item.itemName} for {item.sellingPrice} gems/coins");
            shopManager.OpenPurchaseDialog(item);
        }
    }
}
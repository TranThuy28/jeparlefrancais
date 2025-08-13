using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace InventoryPlus
{
    
// Utility class để tạo shop items trong editor
[System.Serializable]
    public class ShopItemData
    {
        public Item item;
        public int priceOverride = -1; // -1 means use item's selling price
        
        public int GetPrice()
        {
            return priceOverride > 0 ? priceOverride : item.sellingPrice;
        }
    }
}

// Extension để dễ dàng setup shop
namespace InventoryPlus.Editor
{
    #if UNITY_EDITOR
    using UnityEditor;
    
    [CustomEditor(typeof(ShopManager))]
    public class ShopManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            ShopManager shopManager = (ShopManager)target;
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Add 100 Gems (Runtime Only)"))
            {
                if (Application.isPlaying)
                {
                    shopManager.AddGems();
                }
            }
            
            if (GUILayout.Button("Add 50 Coins (Runtime Only)"))
            {
                if (Application.isPlaying)
                {
                    shopManager.AddCoins();
                }
            }
            
            if (GUILayout.Button("Refresh Shop UI"))
            {
                if (Application.isPlaying)
                {
                    // Clear existing items
                    for (int i = shopManager.shopItemContainer.childCount - 1; i >= 0; i--)
                    {
                        DestroyImmediate(shopManager.shopItemContainer.GetChild(i).gameObject);
                    }
                    
                    // Recreate items
                    foreach (Item item in shopManager.shopItems)
                    {
                        GameObject itemUI = Instantiate(shopManager.shopItemPrefab, shopManager.shopItemContainer);
                        ShopItemUI shopItemUI = itemUI.GetComponent<ShopItemUI>();
                        shopItemUI.SetupItem(item, shopManager);
                    }
                }
            }
        }
    }
    #endif
}
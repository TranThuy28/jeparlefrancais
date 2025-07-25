using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using TMPro;

namespace InventoryPlus
{
    [RequireComponent(typeof(UISlot))]
    public class RightClickMenu : MonoBehaviour, IPointerClickHandler
    {
        [Header("Menu Settings")]
        public GameObject contextMenuPrefab; // Prefab của context menu
        
        [Header("Menu Options")]
        public List<MenuOption> menuOptions = new List<MenuOption>();
        
        public UISlot UISlot;
        public Inventory inventory;
        public GameObject currentMenu;

        public GameManager gameManager;
        [System.Serializable]
        public class MenuOption
        {
            public string optionName;
            public Sprite optionIcon;
            public System.Action<UISlot> onClickAction;
            
            public MenuOption(string name, Sprite icon, System.Action<UISlot> action)
            {
                optionName = name;
                optionIcon = icon;
                onClickAction = action;
            }
        }

        #region Setup
        
        void Start()
        {
            UISlot = GetComponent<UISlot>();
            gameManager = FindFirstObjectByType<GameManager>();
            // Setup default menu options
            SetupDefaultMenuOptions();
        }
        
        public void SetInventory(UISlot _UISlot, Inventory _inventory)
        {
            UISlot = _UISlot;
            inventory = _inventory;
        }
        
        private void SetupDefaultMenuOptions()
        {
            // Clear existing options
            menuOptions.Clear();
            
            // Add default options
            menuOptions.Add(new MenuOption("Sell All", null, SellAllItem));
            menuOptions.Add(new MenuOption("Drop", null, DropItem));
            menuOptions.Add(new MenuOption("Sell", null, SellItem));
            menuOptions.Add(new MenuOption("Sort", null, SortItem));
        }
        
        #endregion

        #region Click Handler
        
        public void OnPointerClick(PointerEventData eventData)
        {
            // Check if it's right click
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                // Only show menu if slot has an item
                if (UISlot != null && UISlot.GetIsShown())
                {
                    ShowContextMenu(eventData.position);
                }
            }
            else if (eventData.button == PointerEventData.InputButton.Left)
            {
                // Hide menu on left click
                HideContextMenu();
            }
        }
        
        #endregion

        #region Menu Management
        
        private void ShowContextMenu(Vector2 position)
        {
            // Hide any existing menu first
            HideContextMenu();
            // contextMenuPrefab = contextMenuPrefab ?? GameObject.Find("context_menu");
            // Debug.Log("Showing context menu at position: " + FindAnyObjectByType<RightClickMenu>().contextMenuPrefab.transform.position);
            // Create menu from prefab or dynamically
            if (contextMenuPrefab != null)
            {
                CreateMenuFromPrefab(position);
            }
            else
            {
                CreateMenuDynamically(position);
            }
            
            // Close menu when clicking elsewhere
            StartCoroutine(WaitForClickOutside());
        }
        
        private void CreateMenuFromPrefab(Vector2 position)
        {
            currentMenu = Instantiate(contextMenuPrefab, inventory.transform);
            currentMenu.transform.position = position;
            
            // Setup menu buttons with options
            Button[] buttons = currentMenu.GetComponentsInChildren<Button>();
            for (int i = 0; i < buttons.Length && i < menuOptions.Count; i++)
            {
                int index = i; // Capture for closure
                buttons[i].onClick.AddListener(() => ExecuteMenuOption(index));
                
                // Set button text
                Text buttonText = buttons[i].GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = menuOptions[index].optionName;
                }
                
                // Set button icon if available
                Image buttonIcon = buttons[i].transform.Find("Icon")?.GetComponent<Image>();
                if (buttonIcon != null && menuOptions[index].optionIcon != null)
                {
                    buttonIcon.sprite = menuOptions[index].optionIcon;
                }
            }
        }
        
        private void CreateMenuDynamically(Vector2 position)
        {
            Debug.Log("Creating dynamic context menu at position: " + position);
            // Create menu container
            currentMenu = new GameObject("ContextMenu");
            currentMenu.transform.SetParent(inventory.transform);
            
            // Add background
            Image bg = currentMenu.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            
            // Add vertical layout
            VerticalLayoutGroup layout = currentMenu.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(5, 5, 5, 5);
            layout.spacing = 60;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = false;
            
            // Add content size fitter
            ContentSizeFitter fitter = currentMenu.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // Set position
            RectTransform rectTransform = currentMenu.GetComponent<RectTransform>();
            rectTransform.position = position;
            rectTransform.sizeDelta = new Vector2(10000, 1000);

            LayoutElement layoutElement = currentMenu.AddComponent<LayoutElement>();
            layoutElement.minWidth = 300;
            //layoutElement.preferredWidth = 300;

            for (int i = 0; i < menuOptions.Count; i++)
            {
                CreateMenuButton(i);
            }
        }
        
        private void CreateMenuButton(int optionIndex)
        {
            GameObject buttonObj = new GameObject(menuOptions[optionIndex].optionName);
            buttonObj.transform.SetParent(currentMenu.transform);
            
            // Add button component
            Button button = buttonObj.AddComponent<Button>();
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            
            // Add text
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = menuOptions[optionIndex].optionName;
            text.font = Resources.Load<TMP_FontAsset>("AntonFontAsset");
            text.fontSize = 40;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;

            // Set text rect transform
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            // Set button rect transform
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.localScale = Vector3.one;
            buttonRect.sizeDelta = new Vector2(30, 200);
            
            // Add click listener
            int index = optionIndex;
            button.onClick.AddListener(() => ExecuteMenuOption(index));
            
            Debug.Log("sizeDelta: " + buttonRect.sizeDelta);
            // Add hover effects
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            colors.pressedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            button.colors = colors;
        }
        
        private void ExecuteMenuOption(int index)
        {
            if (index >= 0 && index < menuOptions.Count)
            {
                menuOptions[index].onClickAction?.Invoke(UISlot);
            }
            HideContextMenu();
        }
        
        private void HideContextMenu()
        {
            if (currentMenu != null)
            {
                Destroy(currentMenu);
                currentMenu = null;
            }
        }
        
        private System.Collections.IEnumerator WaitForClickOutside()
        {
            yield return new WaitForEndOfFrame();
            
            while (currentMenu != null)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                {
                    Vector2 mousePos = Input.mousePosition;
                    RectTransform menuRect = currentMenu.GetComponent<RectTransform>();
                    
                    if (!RectTransformUtility.RectangleContainsScreenPoint(menuRect, mousePos))
                    {
                        HideContextMenu();
                        break;
                    }
                }
                yield return null;
            }
        }
        
        #endregion

        #region Menu Actions
        
        private void SellAllItem(UISlot slot)
        {
            ItemSlot inventorySlot = inventory.GetInventorySlot(slot);
            Debug.Log("Selling item: " + inventorySlot.GetItemNum());
            gameManager.currencyManager.AddCoins(inventorySlot.GetItemType().sellingPrice * inventorySlot.GetItemNum());
            int itemnum = inventorySlot.GetItemNum();
            for (int i = 0; i < itemnum; i++)
            {
                inventory.UseItem(slot); // Remove item from inventory
            }
        }
        
        private void DropItem(UISlot slot)
        {
            Debug.Log("Dropping item: " + slot.name);
            // Implement drop item logic here
            // Example: inventory.DropItem(slot);
        }
        
        private void SellItem(UISlot slot)
        {
            Debug.Log("Selling item: " + slot.name);
            gameManager.currencyManager.AddCoins(inventory.GetInventorySlot(slot).GetItemType().sellingPrice);
            inventory.UseItem(slot);
        }
        
        private void SortItem(UISlot slot)
        {
            Debug.Log("Sorting item: " + slot.name);
            inventory.Sort();
        }
        
        #endregion

        #region Public Methods
        
        public void AddMenuOption(string name, Sprite icon, System.Action<UISlot> action)
        {
            menuOptions.Add(new MenuOption(name, icon, action));
        }
        
        public void RemoveMenuOption(string name)
        {
            menuOptions.RemoveAll(option => option.optionName == name);
        }
        
        public void ClearMenuOptions()
        {
            menuOptions.Clear();
        }
        
        #endregion

        void Update()
        {
            // Close menu on Escape key
            if (Input.GetKeyDown(KeyCode.Escape) && currentMenu != null)
            {
                HideContextMenu();
            }
        }
    }
}
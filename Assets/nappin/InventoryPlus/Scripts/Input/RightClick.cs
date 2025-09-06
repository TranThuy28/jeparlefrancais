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
        public GameObject confirmationDialogPrefab; // Prefab của confirmation dialog
        
        [Header("Menu Options")]
        public List<MenuOption> menuOptions = new List<MenuOption>();
        
        public UISlot UISlot;
        public Inventory inventory;
        public GameObject currentMenu;
        public GameObject currentConfirmationDialog; // Dialog hiện tại

        private String[] buttonColor = { "28button_green", "30button_red", "29button_blue" };
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
            menuOptions.Add(new MenuOption("Vendre", null, SellItem));
            menuOptions.Add(new MenuOption("Vendre Tout", null, SellAllItem));
            menuOptions.Add(new MenuOption("Trier", null, SortItem));
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
            Debug.Log("Création du menu contextuel dynamique à la position: " + position);
            // Create menu container
            currentMenu = new GameObject("MenuContextuel");
            currentMenu.transform.SetParent(inventory.transform);
            
            // Add background
            Image bg = currentMenu.AddComponent<Image>();
            bg.sprite = Resources.Load<Sprite>("Btn_Rectangle00_nn_Navy"); // Replace with your background sprite
            bg.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            
            // Add vertical layout
            VerticalLayoutGroup layout = currentMenu.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(5, 5, 5, 5);
            layout.spacing = 30;
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
            buttonImage.sprite = Resources.Load<Sprite>(buttonColor[optionIndex%3]); // Replace with your button sprite
            buttonImage.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            button.targetGraphic = buttonImage;
            
            // Add text
            GameObject textObj = new GameObject("Texte");
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
            buttonRect.sizeDelta = new Vector2(30, 100);
            
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

        #region Confirmation Dialog
        
        private void ShowConfirmationDialog(string title, string message, System.Action onConfirm, System.Action onCancel = null)
        {
            // Ẩn dialog cũ nếu có
            HideConfirmationDialog();
            
            if (confirmationDialogPrefab != null)
            {
                CreateConfirmationFromPrefab(title, message, onConfirm, onCancel);
            }
            else
            {
                CreateConfirmationDynamically(title, message, onConfirm, onCancel);
            }
        }
        
        private void CreateConfirmationFromPrefab(string title, string message, System.Action onConfirm, System.Action onCancel)
        {
            currentConfirmationDialog = Instantiate(confirmationDialogPrefab, inventory.transform.root);
            
            // Tìm và setup các component
            TextMeshProUGUI titleText = currentConfirmationDialog.transform.Find("Titre")?.GetComponent<TextMeshProUGUI>();
            if (titleText != null) titleText.text = title;
            
            TextMeshProUGUI messageText = currentConfirmationDialog.transform.Find("Message")?.GetComponent<TextMeshProUGUI>();
            if (messageText != null) messageText.text = message;
            
            Button confirmButton = currentConfirmationDialog.transform.Find("BoutonConfirmer")?.GetComponent<Button>();
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(() => {
                    onConfirm?.Invoke();
                    HideConfirmationDialog();
                });
            }
            
            Button cancelButton = currentConfirmationDialog.transform.Find("BoutonAnnuler")?.GetComponent<Button>();
            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveAllListeners();
                cancelButton.onClick.AddListener(() => {
                    onCancel?.Invoke();
                    HideConfirmationDialog();
                });
            }
        }
        
        private void CreateConfirmationDynamically(string title, string message, System.Action onConfirm, System.Action onCancel)
        {
            // Tạo overlay background
            GameObject overlay = new GameObject("SuperpositionConfirmation");
            overlay.transform.SetParent(inventory.transform.root);
            
            Image overlayImage = overlay.AddComponent<Image>();
            overlayImage.color = new Color(0, 0, 0, 0f);
            
            RectTransform overlayRect = overlay.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            
            // Tạo dialog container
            currentConfirmationDialog = new GameObject("DialogueConfirmation");
            currentConfirmationDialog.transform.SetParent(overlay.transform);
            
            // Background của dialog
            Image dialogBg = currentConfirmationDialog.AddComponent<Image>();
            dialogBg.sprite = Resources.Load<Sprite>("Btn_Rectangle00_nn_Navy");
            dialogBg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            
            // Layout cho dialog
            VerticalLayoutGroup dialogLayout = currentConfirmationDialog.AddComponent<VerticalLayoutGroup>();
            dialogLayout.padding = new RectOffset(20, 20, 20, 20);
            dialogLayout.spacing = 15;
            dialogLayout.childControlWidth = true;
            
            ContentSizeFitter dialogFitter = currentConfirmationDialog.AddComponent<ContentSizeFitter>();
            dialogFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            dialogFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // Position dialog ở giữa màn hình
            RectTransform dialogRect = currentConfirmationDialog.GetComponent<RectTransform>();
            dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
            dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRect.pivot = new Vector2(0.5f, 0.5f);
            dialogRect.anchoredPosition = Vector2.zero;
            dialogRect.localScale = new Vector2(2, 2);
            
            LayoutElement dialogLayoutElement = currentConfirmationDialog.AddComponent<LayoutElement>();
            dialogLayoutElement.minWidth = 600;
            dialogLayoutElement.minHeight = 500;
            // Tạo title
            CreateDialogText(title, 50, TextAlignmentOptions.Center, "TitreDialogue");
            
            // Tạo message
            CreateDialogText(message, 35, TextAlignmentOptions.Center, "MessageDialogue");
            
            // Tạo button container
            GameObject buttonContainer = new GameObject("ConteneurBoutons");
            buttonContainer.transform.SetParent(currentConfirmationDialog.transform);
            
            HorizontalLayoutGroup buttonLayout = buttonContainer.AddComponent<HorizontalLayoutGroup>();
            buttonLayout.spacing = 20;
            buttonLayout.childControlHeight = false;
            buttonLayout.childControlWidth = false;
            buttonLayout.childForceExpandWidth = true;
            buttonLayout.childAlignment = TextAnchor.MiddleCenter;
            RectTransform buttonContainerRect = buttonContainer.GetComponent<RectTransform>();
            buttonContainerRect.sizeDelta = new Vector2(0, 80);
            buttonContainer.transform.localScale = Vector3.one;

            // Tạo Confirm button
            CreateDialogButton("Oui", buttonContainer, () => {
                onConfirm?.Invoke();
                HideConfirmationDialog();
            }, new Color(0.2f, 0.7f, 0.2f, 1f));
            
            // Tạo Cancel button
            CreateDialogButton("Non", buttonContainer, () => {
                onCancel?.Invoke();
                HideConfirmationDialog();
            }, new Color(0.7f, 0.2f, 0.2f, 1f));
        }

        private void CreateDialogText(string text, int fontSize, TextAlignmentOptions alignment, string name)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(currentConfirmationDialog.transform);
            textObj.transform.localScale = Vector3.one;

            TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.font = Resources.Load<TMP_FontAsset>("AntonFontAsset");
            textComponent.fontSize = fontSize;
            textComponent.color = Color.white;
            textComponent.alignment = alignment;
            textComponent.textWrappingMode = TextWrappingModes.Normal;
            

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(0, fontSize + 10);

            LayoutElement textLayout = textObj.AddComponent<LayoutElement>();
            textLayout.preferredHeight = fontSize + 10;
            textLayout.preferredWidth = 700;
        }
        
        private void CreateDialogButton(string buttonText, GameObject parent, System.Action onClick, Color buttonColor)
        {
            GameObject buttonObj = new GameObject(buttonText + "Bouton");
            buttonObj.transform.SetParent(parent.transform);
            
            Button button = buttonObj.AddComponent<Button>();
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.sprite = Resources.Load<Sprite>(buttonText == "Oui" ? "28button_green" : "30button_red");
            buttonImage.color = buttonColor;
            button.targetGraphic = buttonImage;
            buttonObj.transform.localScale = Vector3.one;
            
            // Button text
            GameObject textObj = new GameObject("Texte");
            textObj.transform.SetParent(buttonObj.transform);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = buttonText;
            text.font = Resources.Load<TMP_FontAsset>("AntonFontAsset");
            text.fontSize = 30;
            text.color = Color.white;
            textObj.transform.localScale = Vector3.one;
            text.alignment = TextAlignmentOptions.Center;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            // Button rect
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(150, 60);
            
            LayoutElement buttonLayoutElement = buttonObj.AddComponent<LayoutElement>();
            buttonLayoutElement.preferredWidth = 150;
            buttonLayoutElement.preferredHeight = 60;
            
            // Click listener
            button.onClick.AddListener(() => onClick?.Invoke());
            
            // Hover effects
            ColorBlock colors = button.colors;
            colors.highlightedColor = Color.Lerp(buttonColor, Color.white, 0.2f);
            colors.pressedColor = Color.Lerp(buttonColor, Color.black, 0.2f);
            button.colors = colors;
        }
        
        private void HideConfirmationDialog()
        {
            if (currentConfirmationDialog != null)
            {
                // Nếu có overlay, destroy overlay (sẽ destroy cả dialog)
                if (currentConfirmationDialog.transform.parent != null && 
                    currentConfirmationDialog.transform.parent.name == "SuperpositionConfirmation")
                {
                    Destroy(currentConfirmationDialog.transform.parent.gameObject);
                }
                else
                {
                    Destroy(currentConfirmationDialog);
                }
                currentConfirmationDialog = null;
            }
        }
        
        #endregion

        #region Menu Actions
        
        private void SellAllItem(UISlot slot)
        {
            ItemSlot inventorySlot = inventory.GetInventorySlot(slot);
            int itemCount = inventorySlot.GetItemNum();
            string itemName = inventorySlot.GetItemType().itemName;
            int totalPrice = inventorySlot.GetItemType().sellingPrice * itemCount;
            
            string message = $"Voulez-vous vendre tous les {itemName}?\n" +
                        $"Quantité: {itemCount}\n" +
                        $"Prix total: {totalPrice} pièces";
            
            ShowConfirmationDialog("Confirmer Vendre Tout", message, () => {
                // Execute sell all
                gameManager.currencyManager.AddCoins(totalPrice);
                for (int i = 0; i < itemCount; i++)
                {
                    inventory.UseItem(slot);
                }
                
                // Show success notification instead of debug log
                string successMessage = $"Vendu avec succès {itemCount} {itemName} pour {totalPrice} pièces";
                // ShowNotification("Vente Terminée", successMessage);
            });
        }
        
        private void DropItem(UISlot slot)
        {
            Debug.Log("Lâcher l'objet: " + slot.name);
            // Implement drop item logic here
            // Example: inventory.DropItem(slot);
        }
        
        private void SellItem(UISlot slot)
        {
            ItemSlot inventorySlot = inventory.GetInventorySlot(slot);
            string itemName = inventorySlot.GetItemType().itemName;
            int sellingPrice = inventorySlot.GetItemType().sellingPrice;
            
            string message = $"Voulez-vous vendre {itemName}?\n" +
                        $"Prix de vente: {sellingPrice} pièces";
            
            ShowConfirmationDialog("Confirmer Vente d'Objet", message, () => {
                // Execute sale
                gameManager.currencyManager.AddCoins(sellingPrice);
                inventory.UseItem(slot);
                
                // Show success notification instead of debug log
                string successMessage = $"Vendu avec succès {itemName} pour {sellingPrice} pièces";
                //ShowNotification("Objet Vendu", successMessage);
            });
        }

        private void SortItem(UISlot slot)
        {
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
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (currentConfirmationDialog != null)
                {
                    HideConfirmationDialog();
                }
                else if (currentMenu != null)
                {
                    HideContextMenu();
                }
            }
        }
    }
}
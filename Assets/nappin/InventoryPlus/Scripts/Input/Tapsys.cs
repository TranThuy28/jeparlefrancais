using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
namespace InventoryPlus
{
    public class TabSystem : MonoBehaviour
    {
        [System.Serializable]
        public class TabData
        {
            public Button tabButton;        // Nút tab
            public GameObject tabContent;   // Nội dung của tab
            public Color normalColor = Color.white;     // Màu bình thường
            public Color selectedColor = Color.yellow;  // Màu khi được chọn
        }

        [Header("Tab Settings")]
        public List<TabData> tabs = new List<TabData>();
        public int defaultTab = 0; // Tab mặc định hiện đầu tiên
        
        [Header("Toggle Settings")]
        public bool allowToggleOff = true; // Cho phép tắt tab đang được chọn
        public KeyCode toggleKey = KeyCode.I; // Phím để toggle on/off toàn bộ tab system

        private int currentSelectedTab = -1;
        private bool isTabSystemActive = true; // Trạng thái bật/tắt của toàn bộ tab system
        private List<GameObject> tabUIElements = new List<GameObject>(); // Danh sách các UI elements cần ẩn/hiện

        void Start()
        {
            // Thu thập tất cả UI elements cần ẩn/hiện
            CollectTabUIElements();

            // Gán sự kiện click cho từng tab button
            for (int i = 0; i < tabs.Count; i++)
            {
                int tabIndex = i; // Capture index for closure
                if (tabs[i].tabButton != null)
                {
                    tabs[i].tabButton.onClick.AddListener(() => SelectTab(tabIndex));
                }
            }

            // Initialize all tabs to hidden state first
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i].tabContent != null)
                {
                    tabs[i].tabContent.SetActive(false);
                }

                if (tabs[i].tabButton != null)
                {
                    Image buttonImage = tabs[i].tabButton.GetComponent<Image>();
                    if (buttonImage != null)
                    {
                        buttonImage.color = tabs[i].normalColor;
                    }
                }
            }

            // Validate defaultTab index
            if (defaultTab < 0 || defaultTab >= tabs.Count)
            {
                Debug.LogWarning($"Default tab index {defaultTab} is out of range. Using 0 instead.");
                defaultTab = 0;
            }

            // Chọn tab mặc định
            if (currentSelectedTab == -1)
            {
                SelectTab(defaultTab);
            }
            ToggleTabSystem();
        }

        void Update()
        {
            // Kiểm tra phím toggle
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleTabSystem();
                //Debug.Log("ToggleTabSystem called. Current state: " + isTabSystemActive);
            }
        }

        public void SelectTab(int tabIndex)
        {

            
            // Kiểm tra nếu tab system bị tắt
            if (!isTabSystemActive)
            {
                Debug.Log("Tab system is inactive");
                return;
            }
                
            // Kiểm tra index hợp lệ
            if (tabIndex < 0 || tabIndex >= tabs.Count)
            {
                Debug.LogWarning("Tab index out of range: " + tabIndex);
                return;
            }

            
            // Nếu tab hiện tại đã được chọn và không cho phép toggle off thì không làm gì
            if (currentSelectedTab == tabIndex)
            {
                Debug.Log("Tab already selected and toggle off is disabled");
                return;
            }

            // Ẩn tab hiện tại
            if (currentSelectedTab >= 0 && currentSelectedTab < tabs.Count)
            {
                HideTab(currentSelectedTab);
            }

            // Hiện tab mới
            ShowTab(tabIndex);
            currentSelectedTab = tabIndex;
        }

        private void ShowTab(int tabIndex)
        {
            if (tabs[tabIndex].tabContent != null)
            {
                tabs[tabIndex].tabContent.SetActive(true);
                // if (tabs[tabIndex].tabContent.name == "(Prb)Inventory")
                // {
                //     tabs[tabIndex].tabContent.GetComponent<Inventory>().ToggleInventory();
                // }
            }

            // Đổi màu button thành màu selected
            if (tabs[tabIndex].tabButton != null)
            {
                Image buttonImage = tabs[tabIndex].tabButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = tabs[tabIndex].selectedColor;
                }
            }
        }

        private void HideTab(int tabIndex)
        {
            if (tabs[tabIndex].tabContent != null)
            {
                tabs[tabIndex].tabContent.SetActive(false);
            }

            // Đổi màu button về màu bình thường
            if (tabs[tabIndex].tabButton != null)
            {
                Image buttonImage = tabs[tabIndex].tabButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = tabs[tabIndex].normalColor;
//                    Debug.Log("Deselected tab: " + tabIndex);
                }
            }
        }

        // Toggle off tab hiện tại
        public void ToggleOffCurrentTab()
        {
            Debug.Log("ToggleOffCurrentTab called. Current tab: " + currentSelectedTab);
            
            if (currentSelectedTab >= 0 && currentSelectedTab < tabs.Count)
            {
                HideTab(currentSelectedTab);
                currentSelectedTab = -1; // Không có tab nào được chọn
                Debug.Log("Successfully toggled off current tab");
            }
            else
            {
                Debug.LogWarning("No valid tab to toggle off. Current tab: " + currentSelectedTab);
            }
        }

        // Thu thập tất cả UI elements của tab system
        private void CollectTabUIElements()
        {
            tabUIElements.Clear();

            for (int i = 0; i < tabs.Count; i++)
            {
                // Thêm tab button
                if (tabs[i].tabButton != null && !tabUIElements.Contains(tabs[i].tabButton.gameObject))
                {
                    tabUIElements.Add(tabs[i].tabButton.gameObject);
                }

                // Thêm tab content
                if (tabs[i].tabContent != null && !tabUIElements.Contains(tabs[i].tabContent))
                {
                    tabUIElements.Add(tabs[i].tabContent);
                }
            }

        }

        // Toggle toàn bộ tab system on/off
        public void ToggleTabSystem()
        {
            isTabSystemActive = !isTabSystemActive;
            
            if (isTabSystemActive)
            {
                // Bật tab system - hiện tất cả UI elements
                foreach (GameObject element in tabUIElements)
                {
                    if (element != null)
                    {
                        // Chỉ hiện tab buttons, tab contents sẽ được quản lý bởi SelectTab
                        if (IsTabButton(element))
                        {
                            element.SetActive(true);
                        }
                    }
                }
                
                // Khôi phục tab đã chọn hoặc chọn tab mặc định
                if (currentSelectedTab >= 0 && currentSelectedTab < tabs.Count)
                {
                    ShowTab(currentSelectedTab);
                }
                else
                {
                    Debug.Log("No tab selected, selecting default tab: " + defaultTab);
                    SelectTab(defaultTab);
                }
                
                Debug.Log("Tab system enabled");
            }
            else
            {
                // Tắt tab system - ẩn tất cả UI elements
                foreach (GameObject element in tabUIElements)
                {
                    if (element != null)
                    {
                        element.SetActive(false);
                    }
                }
//                Debug.Log("Tab system disabled");
            }
        }

        // Kiểm tra xem GameObject có phải là tab button không
        private bool IsTabButton(GameObject obj)
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i].tabButton != null && tabs[i].tabButton.gameObject == obj)
                {
                    return true;
                }
            }
            return false;
        }

        // Hàm công khai để chọn tab từ bên ngoài
        public void SelectTabByName(string tabName)
        {
            if (!isTabSystemActive)
                return;
                
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i].tabButton != null && tabs[i].tabButton.name == tabName)
                {
                    SelectTab(i);
                    return;
                }
            }
            Debug.LogWarning("Tab not found: " + tabName);
        }

        // Lấy tab hiện tại
        public int GetCurrentTab()
        {
            return currentSelectedTab;
        }

        // Kiểm tra tab system có đang hoạt động không
        public bool IsTabSystemActive()
        {
            return isTabSystemActive;
        }

        // Chuyển sang tab tiếp theo
        public void NextTab()
        {
            if (!isTabSystemActive)
                return;
                
            int nextTab = (currentSelectedTab + 1) % tabs.Count;
            SelectTab(nextTab);
        }

        // Chuyển sang tab trước đó
        public void PreviousTab()
        {
            if (!isTabSystemActive)
                return;
                
            int prevTab = (currentSelectedTab - 1 + tabs.Count) % tabs.Count;
            SelectTab(prevTab);
        }

        // Bật tab system
        public void EnableTabSystem()
        {
            if (!isTabSystemActive)
            {
                ToggleTabSystem();
            }
        }

        // Tắt tab system
        public void DisableTabSystem()
        {
            if (isTabSystemActive)
            {
                ToggleTabSystem();
            }
        }
    }
}
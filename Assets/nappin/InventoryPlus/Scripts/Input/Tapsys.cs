using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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

    private int currentSelectedTab = -1;

    void Start()
    {
        // Gán sự kiện click cho từng tab button
        for (int i = 0; i < tabs.Count; i++)
        {
            int tabIndex = i; // Capture index for closure
            if (tabs[i].tabButton != null)
            {
                tabs[i].tabButton.onClick.AddListener(() => SelectTab(tabIndex));
            }
        }

        for (int i = 0; i < tabs.Count; i++)
        {
            Image buttonImage = tabs[i].tabButton.GetComponent<Image>();
            buttonImage.color = tabs[i].selectedColor;

            if (i != defaultTab)
            {
                HideTab(i);
                buttonImage.color = tabs[i].normalColor;
            }
        }
        // Chọn tab mặc định
        SelectTab(defaultTab);
    }

    public void SelectTab(int tabIndex)
    {
        // Kiểm tra index hợp lệ
        if (tabIndex < 0 || tabIndex >= tabs.Count)
        {
            Debug.LogWarning("Tab index out of range: " + tabIndex);
            return;
        }

        // Nếu tab hiện tại đã được chọn thì không làm gì
        if (currentSelectedTab == tabIndex)
            return;

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
                Debug.Log("Deselected tab: " + tabIndex);
            }
        }
    }

    // Hàm công khai để chọn tab từ bên ngoài
    public void SelectTabByName(string tabName)
    {
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

    // Chuyển sang tab tiếp theo
    public void NextTab()
    {
        int nextTab = (currentSelectedTab + 1) % tabs.Count;
        SelectTab(nextTab);
    }

    // Chuyển sang tab trước đó
    public void PreviousTab()
    {
        int prevTab = (currentSelectedTab - 1 + tabs.Count) % tabs.Count;
        SelectTab(prevTab);
    }
}
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace InventoryPlus
{
    // Script chính để quản lý hệ thống nhiệm vụ với Scroll View
    public class QuestManager : MonoBehaviour
    {
        [Header("UI References")]
        public Button mainQuestButton;
        public Button dailyQuestButton;
        public ScrollRect questScrollView;  // Thay đổi: Thêm ScrollRect reference
        public Transform questContainer;    // Đây sẽ là Content của ScrollView
        public GameObject questPrefab;
        public TextMeshProUGUI headerText;
        
        [Header("Button Colors")]
        public Color activeButtonColor = new Color(0.2f, 0.7f, 1f, 1f);
        public Color inactiveButtonColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        
        [Header("Quest Data")]
        public List<Quest> mainQuests = new List<Quest>();
        public List<Quest> dailyQuests = new List<Quest>();
        
        [Header("Scroll Settings")]
        public float questItemHeight = 200f;  // Chiều cao của mỗi quest item
        public float spacing = 10f;          // Khoảng cách giữa các quest items
        
        private bool showingMainQuests = true;
        private List<GameObject> currentQuestObjects = new List<GameObject>();
        
        void Start()
        {
            Debug.Log("QuestManager Start - Initializing quests and UI.");
            InitializeQuests();
            SetupUI();
            SetupScrollView();
            DisplayQuests();
        }
        
        void InitializeQuests()
        {
            // Khởi tạo nhiệm vụ chính
            mainQuests.Add(new Quest("Khám Phá Thế Giới", "Khám phá 5 khu vực mới", 5, 500, 100));
            mainQuests.Add(new Quest("Thành Thạo Chiến Đấu", "Đánh bại 50 kẻ thù", 50, 1000, 200));
            mainQuests.Add(new Quest("Thu Thập Kho Báu", "Thu thập 20 vật phẩm quý hiếm", 20, 800, 300));
            mainQuests.Add(new Quest("Nâng Cấp Trang Bị", "Nâng cấp vũ khí lên cấp 10", 10, 1200, 500));
            mainQuests.Add(new Quest("Chinh Phục Boss", "Đánh bại 3 boss mạnh", 3, 2000, 1000));
            mainQuests.Add(new Quest("Hoàn Thành Dungeon", "Vượt qua 10 dungeon", 10, 1500, 750));
            
            // Khởi tạo nhiệm vụ hàng ngày
            dailyQuests.Add(new Quest("Tập Luyện Hàng Ngày", "Hoàn thành 3 trận đấu", 3, 200, 50));
            dailyQuests.Add(new Quest("Thu Hoạch Tài Nguyên", "Thu thập 15 tài nguyên", 15, 150, 30));
            dailyQuests.Add(new Quest("Giao Lưu Bạn Bè", "Tham gia 2 hoạt động cùng bạn", 2, 100, 25));
            dailyQuests.Add(new Quest("Hoàn Thành Thử Thách", "Vượt qua 1 thử thách khó", 1, 300, 75));
            dailyQuests.Add(new Quest("Săn Lùng Quái Vật", "Tiêu diệt 25 quái vật", 25, 250, 60));
            dailyQuests.Add(new Quest("Thu Thập Nguyên Liệu", "Thu thập 30 nguyên liệu chế tạo", 30, 180, 40));
            dailyQuests.Add(new Quest("Tăng Cường Kỹ Năng", "Sử dụng kỹ năng đặc biệt 5 lần", 5, 120, 35));
            
            // Tạo tiến độ ngẫu nhiên cho demo
            foreach (var quest in mainQuests)
            {
                quest.UpdateProgress(Random.Range(0, quest.targetAmount));
            }
            
            foreach (var quest in dailyQuests)
            {
                quest.UpdateProgress(Random.Range(0, quest.targetAmount));
            }
        }
        
        void SetupUI()
        {
            if (mainQuestButton != null)
            {
                mainQuestButton.onClick.AddListener(() => ShowMainQuests());
            }
            
            if (dailyQuestButton != null)
            {
                dailyQuestButton.onClick.AddListener(() => ShowDailyQuests());
            }
            
            UpdateButtonAppearance();
            UpdateHeaderText();
        }
        
        void SetupScrollView()
        {
            Debug.Log(questScrollView != null ? "Quest ScrollView found." : "Quest ScrollView is null.");
            // Đảm bảo ScrollView được thiết lập đúng cách
            if (questScrollView != null)
            {
                // Thiết lập scroll direction (vertical)
                questScrollView.horizontal = false;
                questScrollView.vertical = true;

                // Thiết lập content container nếu chưa có
                if (questContainer == null && questScrollView.content != null)
                {
                    questContainer = questScrollView.content;
                }

                // Thiết lập VerticalLayoutGroup cho content
                SetupContentLayoutGroup();
            }
        }
        
        void SetupContentLayoutGroup()
        {
            Debug.Log(questContainer != null ? "Quest container found." : "Quest container is null.");
            if (questContainer != null)
            {
                // Thêm VerticalLayoutGroup nếu chưa có
                VerticalLayoutGroup layoutGroup = questContainer.GetComponent<VerticalLayoutGroup>();
                if (layoutGroup == null)
                {
                    layoutGroup = questContainer.gameObject.AddComponent<VerticalLayoutGroup>();
                }

                // Thiết lập layout properties
                layoutGroup.spacing = spacing;
                layoutGroup.childAlignment = TextAnchor.UpperCenter;
                layoutGroup.childControlWidth = true;
                layoutGroup.childControlHeight = false;
                layoutGroup.childForceExpandWidth = true;
                layoutGroup.childForceExpandHeight = false;

                // Thêm ContentSizeFitter để tự động điều chỉnh kích thước content
                ContentSizeFitter sizeFitter = questContainer.GetComponent<ContentSizeFitter>();
                if (sizeFitter == null)
                {
                    sizeFitter = questContainer.gameObject.AddComponent<ContentSizeFitter>();
                }
                sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
        }
        
        void UpdateButtonAppearance()
        {
            if (mainQuestButton != null)
            {
                var mainButtonImage = mainQuestButton.GetComponent<Image>();
                if (mainButtonImage != null)
                {
                    mainButtonImage.color = showingMainQuests ? activeButtonColor : inactiveButtonColor;
                }
            }
            
            if (dailyQuestButton != null)
            {
                var dailyButtonImage = dailyQuestButton.GetComponent<Image>();
                if (dailyButtonImage != null)
                {
                    dailyButtonImage.color = !showingMainQuests ? activeButtonColor : inactiveButtonColor;
                }
            }
        }
        
        void UpdateHeaderText()
        {
            if (headerText != null)
            {
                headerText.text = showingMainQuests ? "NHIỆM VỤ CHÍNH" : "NHIỆM VỤ HÀNG NGÀY";
            }
        }
        
        public void ShowMainQuests()
        {
            if (!showingMainQuests)
            {
                showingMainQuests = true;
                UpdateButtonAppearance();
                UpdateHeaderText();
                DisplayQuests();
                ResetScrollPosition();
            }
        }
        
        public void ShowDailyQuests()
        {
            if (showingMainQuests)
            {
                showingMainQuests = false;
                UpdateButtonAppearance();
                UpdateHeaderText();
                DisplayQuests();
                ResetScrollPosition();
            }
        }
        
        void ResetScrollPosition()
        {
            // Reset scroll position về đầu danh sách
            if (questScrollView != null)
            {
                questScrollView.verticalNormalizedPosition = 1f;
            }
        }
        
        void DisplayQuests()
        {
            // Xóa các quest object cũ
            foreach (GameObject obj in currentQuestObjects)
            {
                if (obj != null)
                    DestroyImmediate(obj);
            }
            currentQuestObjects.Clear();
            
            // Hiển thị quest mới
            List<Quest> questsToShow = showingMainQuests ? mainQuests : dailyQuests;
            
            foreach (Quest quest in questsToShow)
            {
                GameObject questObj = Instantiate(questPrefab, questContainer);
                QuestUI questUI = questObj.GetComponent<QuestUI>();
                
                // Thiết lập kích thước cho quest item
                RectTransform questRect = questObj.GetComponent<RectTransform>();
                if (questRect != null)
                {
                    questRect.sizeDelta = new Vector2(questRect.sizeDelta.x, questItemHeight);
                }
                
                if (questUI != null)
                {
                    questUI.SetupQuest(quest);
                }
                
                currentQuestObjects.Add(questObj);
            }
            
            // Force layout rebuild để đảm bảo scroll view cập nhật đúng
            StartCoroutine(ForceLayoutRebuild());
        }
        
        System.Collections.IEnumerator ForceLayoutRebuild()
        {
            yield return new WaitForEndOfFrame();
            if (questContainer != null)
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(questContainer.GetComponent<RectTransform>());
            }
        }
        
        // Hàm để cập nhật tiến độ nhiệm vụ từ bên ngoài
        public void UpdateQuestProgress(string questTitle, int amount)
        {
            Quest quest = mainQuests.Find(q => q.title == questTitle);
            if (quest == null)
                quest = dailyQuests.Find(q => q.title == questTitle);
                
            if (quest != null)
            {
                quest.UpdateProgress(amount);
                DisplayQuests(); // Cập nhật UI
            }
        }
        
        // Hàm để hoàn thành nhiệm vụ
        public void CompleteQuest(Quest quest)
        {
            if (!quest.isCompleted) return;
            
            // Thêm phần thưởng cho người chơi
            Debug.Log($"Hoàn thành nhiệm vụ: {quest.title}");
            Debug.Log($"Nhận được: {quest.rewardExp} EXP, {quest.rewardGold} Gold");
            
            // Có thể thêm logic xử lý phần thưởng ở đây
        }
        
        // Utility functions cho scroll view
        public void ScrollToTop()
        {
            if (questScrollView != null)
            {
                questScrollView.verticalNormalizedPosition = 1f;
            }
        }
        
        public void ScrollToBottom()
        {
            if (questScrollView != null)
            {
                questScrollView.verticalNormalizedPosition = 0f;
            }
        }
    }
}
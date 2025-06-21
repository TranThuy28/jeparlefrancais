using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
namespace InventoryPlus
{
    public class QuestUI : MonoBehaviour
    {
        [Header("UI Components")]
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI progressText;
        public Slider progressBar;
        public TextMeshProUGUI rewardText;
        public Button completeButton;
        public Image backgroundImage;

        private Quest currentQuest;

        public void SetupQuest(Quest quest)
        {
            currentQuest = quest;

            if (titleText != null)
                titleText.text = quest.title;

            if (descriptionText != null)
                descriptionText.text = quest.description;

            if (progressText != null)
                progressText.text = $"{quest.currentAmount}/{quest.targetAmount}";

            if (progressBar != null)
            {
                progressBar.value = quest.GetProgressPercent();
            }

            if (rewardText != null)
                rewardText.text = $"Phần thưởng: {quest.rewardExp} EXP, {quest.rewardGold} Gold";

            if (completeButton != null)
            {
                completeButton.gameObject.SetActive(quest.isCompleted);
                completeButton.onClick.RemoveAllListeners();
                completeButton.onClick.AddListener(() => OnCompleteButtonClicked());
            }

            // Thay đổi màu nền dựa trên trạng thái hoàn thành
            if (backgroundImage != null)
            {
                if (quest.isCompleted)
                    backgroundImage.color = new Color(0.7f, 1f, 0.7f, 0.8f); // Màu xanh nhạt
                else
                    backgroundImage.color = new Color(1f, 1f, 1f, 0.8f); // Màu trắng
            }
        }

        void OnCompleteButtonClicked()
        {
            if (currentQuest != null && currentQuest.isCompleted)
            {
                QuestManager questManager = FindObjectOfType<QuestManager>();
                if (questManager != null)
                {
                    questManager.CompleteQuest(currentQuest);
                }

                // Ẩn button sau khi hoàn thành
                if (completeButton != null)
                    completeButton.gameObject.SetActive(false);
            }
        }
    }
}
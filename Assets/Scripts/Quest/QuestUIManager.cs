using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject questPanel;
    public Transform questListParent;
    public GameObject questItemPrefab;
    public Button closeButton;
    
    [Header("Daily Mission Panel")]
    public GameObject dailyMissionPanel;
    public Transform dailyQuestParent;
    
    [Header("Claim Rewards Panel")]
    public GameObject claimRewardsPanel;
    public Button claimAllButton;
    
    private QuestManager questManager;
    private List<QuestUIItem> questUIItems = new List<QuestUIItem>();

    private void Start()
    {
        questManager = FindFirstObjectByType<QuestManager>();
        SetupUIEvents();
        RefreshQuestUI();
    }

    private void OnEnable()
    {
        QuestManager.OnQuestCompleted += OnQuestCompleted;
        QuestManager.OnQuestProgressUpdated += OnQuestProgressUpdated;
        QuestManager.OnQuestClaimed += OnQuestClaimed;
        QuestManager.OnQuestListUpdated += RefreshQuestUI;
    }

    private void OnDisable()
    {
        QuestManager.OnQuestCompleted -= OnQuestCompleted;
        QuestManager.OnQuestProgressUpdated -= OnQuestProgressUpdated;
        QuestManager.OnQuestClaimed -= OnQuestClaimed;
        QuestManager.OnQuestListUpdated -= RefreshQuestUI;
    }

    private void SetupUIEvents()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseQuestPanel);
        
        if (claimAllButton != null)
            claimAllButton.onClick.AddListener(ClaimAllRewards);
    }

    public void OpenQuestPanel()
    {
        questPanel.SetActive(true);
        RefreshQuestUI();
    }

    public void CloseQuestPanel()
    {
        questPanel.SetActive(false);
    }

    public void RefreshQuestUI()
    {
        ClearQuestItems();
        CreateQuestItems();
        UpdateClaimAllButton();
    }

    private void ClearQuestItems()
    {
        foreach (QuestUIItem item in questUIItems)
        {
            if (item != null && item.gameObject != null)
                Destroy(item.gameObject);
        }
        questUIItems.Clear();
    }

    private void CreateQuestItems()
    {
        List<Quest> activeQuests = questManager.GetActiveQuests();
        
        foreach (Quest quest in activeQuests)
        {
            GameObject questItemObj = Instantiate(questItemPrefab, questListParent);
            QuestUIItem questUIItem = questItemObj.GetComponent<QuestUIItem>();
            
            if (questUIItem != null)
            {
                questUIItem.SetupQuestItem(quest, questManager);
                questUIItems.Add(questUIItem);
            }
        }
    }

    private void UpdateClaimAllButton()
    {
        List<Quest> completedQuests = questManager.GetCompletedQuests();
        claimAllButton.interactable = completedQuests.Count > 0;
    }

    private void ClaimAllRewards()
    {
        List<Quest> completedQuests = questManager.GetCompletedQuests();
        foreach (Quest quest in completedQuests)
        {
            questManager.ClaimQuestReward(quest);
        }
    }

    private void OnQuestCompleted(Quest quest)
    {
        // Hiển thị hiệu ứng hoàn thành quest
        ShowQuestCompletedEffect(quest);
    }

    private void OnQuestProgressUpdated(Quest quest)
    {
        // Cập nhật progress bar của quest tương ứng
        UpdateQuestProgress(quest);
    }

    private void OnQuestClaimed(Quest quest)
    {
        // Hiển thị hiệu ứng claim reward
        ShowRewardClaimedEffect(quest);
    }

    private void ShowQuestCompletedEffect(Quest quest)
    {
        // Implement quest completion effect
        Debug.Log($"Quest Completed: {quest.title}");
    }

    private void UpdateQuestProgress(Quest quest)
    {
        QuestUIItem questUIItem = questUIItems.Find(item => item.GetQuestId() == quest.id);
        if (questUIItem != null)
        {
            questUIItem.UpdateProgress();
        }
    }

    private void ShowRewardClaimedEffect(Quest quest)
    {
        // Implement reward claimed effect
        Debug.Log($"Reward Claimed for: {quest.title}");
    }
}
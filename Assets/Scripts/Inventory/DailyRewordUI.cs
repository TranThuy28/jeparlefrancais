using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Individual reward item component
[System.Serializable]
public class DailyRewardItem : MonoBehaviour
{
    [Header("UI Components")]
    public Image rewardIcon;
    public TextMeshProUGUI rewardAmountText;
    public Button claimButton;
    public Image progressFill;
    public Image backgroundImage;
    public GameObject claimedCheckmark;
    public GameObject lockIcon;
    
    [Header("Multiple Reward Display")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI gemsText;
    public TextMeshProUGUI experienceText;
    public Transform itemsContainer;
    public GameObject itemDisplayPrefab;

    private QuestReward questRewardData;
    private DailyRewardSystem rewardSystem;
    private int requiredMissions;

    public void Initialize(QuestReward questReward, DailyRewardSystem system)
    {
        questRewardData = questReward;
        rewardSystem = system;
        requiredMissions = system.GetTotalMissionsCount();

        // Setup UI based on quest reward
        SetupRewardDisplay();

        if (claimButton != null)
            claimButton.onClick.AddListener(() => rewardSystem.ClaimReward());
    }

    private void SetupRewardDisplay()
    {
        if (questRewardData == null) return;

        // Display individual reward amounts
        if (coinsText != null)
        {
            coinsText.text = questRewardData.coins > 0 ? questRewardData.coins.ToString() : "0";
            coinsText.gameObject.SetActive(questRewardData.coins > 0);
        }

        if (gemsText != null)
        {
            gemsText.text = questRewardData.gems > 0 ? questRewardData.gems.ToString() : "0";
            gemsText.gameObject.SetActive(questRewardData.gems > 0);
        }

        if (experienceText != null)
        {
            experienceText.text = questRewardData.experience > 0 ? questRewardData.experience.ToString() : "0";
            experienceText.gameObject.SetActive(questRewardData.experience > 0);
        }

        // Display main reward amount (total value or primary reward)
        if (rewardAmountText != null)
        {
            int totalValue = questRewardData.coins + questRewardData.gems + questRewardData.experience;
            rewardAmountText.text = totalValue > 0 ? totalValue.ToString() : "Reward";
        }

        // Setup items display
        SetupItemsDisplay();

        // Set default colors
        if (backgroundImage != null)
            backgroundImage.color = Color.white;
    }

    private void SetupItemsDisplay()
    {
        if (itemsContainer == null || itemDisplayPrefab == null) return;

        // Clear existing item displays
        foreach (Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }

        // Create displays for each item
        foreach (var itemEntry in questRewardData.items)
        {
            if (itemEntry.item != null && itemEntry.quantity > 0)
            {
                GameObject itemDisplay = Instantiate(itemDisplayPrefab, itemsContainer);
                
                // Setup item display (assuming it has Image and Text components)
                Image itemIcon = itemDisplay.GetComponent<Image>();
                TextMeshProUGUI itemQuantity = itemDisplay.GetComponentInChildren<TextMeshProUGUI>();

                if (itemQuantity != null)
                    itemQuantity.text = itemEntry.quantity.ToString();
            }
        }
    }

    public void UpdateState(bool canClaim, bool alreadyClaimed, int currentProgress, int maxProgress)
    {
        // Update progress fill
        if (progressFill != null)
        {
            float fillAmount = (float)currentProgress / Mathf.Max(requiredMissions, 1);
            progressFill.fillAmount = Mathf.Clamp01(fillAmount);
        }

        // Update claim button
        if (claimButton != null)
        {
            claimButton.interactable = canClaim && !alreadyClaimed;
        }

        // Update visual states
        if (claimedCheckmark != null)
            claimedCheckmark.SetActive(alreadyClaimed);

        if (lockIcon != null)
            lockIcon.SetActive(!canClaim && !alreadyClaimed);

        // Update colors based on state
        // Color targetColor = Color.white;
        // if (alreadyClaimed)
        //     targetColor = Color.green;
        // else if (!canClaim)
        //     targetColor = Color.gray;

        // // Apply color to reward displays
        // if (rewardIcon != null)
        //     rewardIcon.color = targetColor;

        // if (coinsText != null)
        //     coinsText.color = targetColor;

        // if (gemsText != null)
        //     gemsText.color = targetColor;

        // if (experienceText != null)
        //     experienceText.color = targetColor;

        // Update background color
        // if (backgroundImage != null)
        // {
        //     Color bgColor = Color.white;
        //     if (alreadyClaimed)
        //         bgColor = Color.green * 0.8f;
        //     else if (!canClaim)
        //         bgColor = Color.gray * 0.8f;
        //     else
        //         bgColor = Color.yellow * 0.8f; // Available to claim
            
        //     bgColor.a = backgroundImage.color.a; // Keep original alpha
        //     backgroundImage.color = bgColor;
        // }

        // Update button text
        if (claimButton != null)
        {
            TextMeshProUGUI buttonText = claimButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                if (alreadyClaimed)
                    buttonText.text = "Claimed";
                else if (canClaim)
                    buttonText.text = "Claim";
                else
                    buttonText.text = $"Complete All ({currentProgress}/{maxProgress})";
            }
        }
    }

    // Method để lấy thông tin reward (để UI khác có thể sử dụng)
    public QuestReward GetQuestReward()
    {
        return questRewardData;
    }

    // Method để kiểm tra có reward items không
    public bool HasItems()
    {
        return questRewardData != null && questRewardData.items.Count > 0;
    }

    // Method để lấy tổng giá trị reward
    public int GetTotalRewardValue()
    {
        if (questRewardData == null) return 0;
        return questRewardData.coins + questRewardData.gems + questRewardData.experience;
    }
}
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace InventoryPlus
{
    // Class để định nghĩa một nhiệm vụ
    [System.Serializable]
    [CreateAssetMenu(menuName = "InventoryPlus/Quest", order = 1)]
    public class Quest : ScriptableObject
    {
        public string title;
        public string description;
        public int targetAmount;
        public int currentAmount;
        public bool isCompleted;
        public int rewardExp;
        public int rewardGold;

        public Quest(string title, string description, int targetAmount, int rewardExp, int rewardGold)
        {
            this.title = title;
            this.description = description;
            this.targetAmount = targetAmount;
            this.currentAmount = 0;
            this.isCompleted = false;
            this.rewardExp = rewardExp;
            this.rewardGold = rewardGold;
        }

        public void UpdateProgress(int amount)
        {
            currentAmount += amount;
            if (currentAmount >= targetAmount)
            {
                currentAmount = targetAmount;
                isCompleted = true;
            }
        }

        public float GetProgressPercent()
        {
            return (float)currentAmount / targetAmount;
        }
    }
}
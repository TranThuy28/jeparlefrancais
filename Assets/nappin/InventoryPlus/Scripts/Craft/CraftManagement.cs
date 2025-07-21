using System.Collections.Generic;
using UnityEngine;

namespace InventoryPlus
{
    public static class CraftingUtils
    {
        public static (bool isValid, Item result, string failureReason) CheckCraftingRecipe(List<ItemSlot> inputSlots, List<CraftingRecipe> recipes)
        {
            // Đếm số lượng itemCategory trong input
            Dictionary<string, int> categoryCounts = new Dictionary<string, int>();
            foreach (var slot in inputSlots)
            {
                if (slot != null && slot.GetItemType() != null)
                {
                    string category = slot.GetItemType().itemCategory;
                    if (!categoryCounts.ContainsKey(category))
                        categoryCounts[category] = 0;
                    categoryCounts[category]++;
                }
            }

            // Kiểm tra từng công thức
            foreach (var recipe in recipes)
            {
                bool allRequirementsMet = true;
                string failureReason = "";

                // Kiểm tra từng yêu cầu trong công thức
                foreach (var requirement in recipe.categoryRequirements)
                {
                    if (categoryCounts.ContainsKey(requirement.itemCategory))
                    {
                        if (categoryCounts[requirement.itemCategory] < requirement.minCount)
                        {
                            allRequirementsMet = false;
                            failureReason = $"Not enough {requirement.itemCategory} (need {requirement.minCount}, got {categoryCounts[requirement.itemCategory]}).";
                            break;
                        }
                    }
                    else
                    {
                        allRequirementsMet = false;
                        failureReason = $"Missing {requirement.itemCategory} (need {requirement.minCount}).";
                        break;
                    }
                }

                // Nếu tất cả yêu cầu được thỏa mãn, trả về kết quả thành công
                if (allRequirementsMet)
                {
                    return (true, recipe.result, "");
                }
            }

            // Nếu không công thức nào khớp, trả về item thất bại
            return (false, null, "No matching recipe found. Output: Broken Shard.");
        }
    }

    [System.Serializable]
    public class CategoryRequirement
    {
        public string itemCategory;
        public int minCount;
    }

    [CreateAssetMenu(menuName = "InventoryPlus/CraftingRecipe")]
    public class CraftingRecipe : ScriptableObject
    {
        public string recipeName;
        public Item result;
        public List<CategoryRequirement> categoryRequirements;
    }
}
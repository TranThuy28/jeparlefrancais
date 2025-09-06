using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace InventoryPlus
{
    public static class CraftingUtils
    {
        public static (bool isValid, Item result, string message) CheckCraftingRecipe(List<ItemSlot> inputSlots, List<CraftingRecipe> recipes)
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

            // Tìm tất cả các recipes thỏa mãn điều kiện
            List<CraftingRecipe> validRecipes = new List<CraftingRecipe>();
            
            foreach (var recipe in recipes)
            {
                bool allRequirementsMet = true;

                // Kiểm tra từng yêu cầu trong công thức
                foreach (var requirement in recipe.categoryRequirements)
                {
                    if (categoryCounts.ContainsKey(requirement.itemCategory))
                    {
                        if (categoryCounts[requirement.itemCategory] < requirement.minCount)
                        {
                            allRequirementsMet = false;
                            break;
                        }
                    }
                    else
                    {
                        allRequirementsMet = false;
                        break;
                    }
                }

                if (allRequirementsMet)
                {
                    validRecipes.Add(recipe);
                }
            }

            // Nếu không có recipe nào thỏa mãn
            if (validRecipes.Count == 0)
            {
                return (false, null, "No matching recipe found. Output: Broken Shard.");
            }

            // Chọn recipe dựa trên tier weight (tier cao hơn = tỷ lệ cao hơn)
            CraftingRecipe selectedRecipe = SelectRecipeByTierWeight(validRecipes);
            string tierName = GetTierDisplayName(selectedRecipe.tier);
            
            return (true, selectedRecipe.result, $"Crafted successfully! Got {selectedRecipe.result.name} ({tierName} tier).");
        }

        /// <summary>
        /// Chọn recipe dựa trên tier weight
        /// Tier cao hơn = weight cao hơn = tỷ lệ được chọn cao hơn
        /// </summary>
        private static CraftingRecipe SelectRecipeByTierWeight(List<CraftingRecipe> validRecipes)
        {
            if (validRecipes.Count == 1)
                return validRecipes[0];

            // Tính weight cho từng recipe dựa trên tier
            List<float> weights = new List<float>();
            float totalWeight = 0f;

            foreach (var recipe in validRecipes)
            {
                float weight = GetTierWeight(recipe.tier);
                weights.Add(weight);
                totalWeight += weight;
            }

            // Roll random để chọn recipe
            float roll = Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            for (int i = 0; i < validRecipes.Count; i++)
            {
                currentWeight += weights[i];
                if (roll <= currentWeight)
                {
                    return validRecipes[i];
                }
            }

            // Fallback (không nên xảy ra)
            return validRecipes[validRecipes.Count - 1];
        }

        /// <summary>
        /// Lấy weight dựa trên tier
        /// Tier cao hơn = weight cao hơn = tỷ lệ được chọn cao hơn
        /// </summary>
        private static float GetTierWeight(CraftingTier tier)
        {
            return tier switch
            {
                CraftingTier.Common    => 100f,  // dễ ra nhất
                CraftingTier.Uncommon  => 80f,
                CraftingTier.Rare      => 55f,
                CraftingTier.Epic      => 35f,
                CraftingTier.Legendary => 20f,
                CraftingTier.Mythic    => 10f,   // khó ra nhất
                _ => 10f
            };
        }

        /// <summary>
        /// Lấy màu sắc hiển thị cho tier
        /// </summary>
        public static Color GetTierColor(CraftingTier tier)
        {
            return tier switch
            {
                CraftingTier.Common => Color.white,
                CraftingTier.Uncommon => Color.green,
                CraftingTier.Rare => Color.blue,
                CraftingTier.Epic => new Color(0.6f, 0f, 1f), // Purple
                CraftingTier.Legendary => new Color(1f, 0.5f, 0f), // Orange
                CraftingTier.Mythic => Color.red,
                _ => Color.white
            };
        }

        /// <summary>
        /// Lấy tên hiển thị cho tier
        /// </summary>
        public static string GetTierDisplayName(CraftingTier tier)
        {
            return tier switch
            {
                CraftingTier.Common => "Common",
                CraftingTier.Uncommon => "Uncommon",
                CraftingTier.Rare => "Rare",
                CraftingTier.Epic => "Epic",
                CraftingTier.Legendary => "Legendary",
                CraftingTier.Mythic => "Mythic",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Hiển thị tỷ lệ của các tier (để debug hoặc UI)
        /// </summary>
        public static void ShowTierProbabilities(List<CraftingRecipe> validRecipes)
        {
            if (validRecipes == null || validRecipes.Count == 0)
                return;

            float totalWeight = 0f;
            foreach (var recipe in validRecipes)
            {
                totalWeight += GetTierWeight(recipe.tier);
            }

            Debug.Log("=== Crafting Probabilities ===");
            foreach (var recipe in validRecipes)
            {
                float weight = GetTierWeight(recipe.tier);
                float probability = (weight / totalWeight) * 100f;
                Debug.Log($"{recipe.result.name} ({GetTierDisplayName(recipe.tier)}): {probability:F1}%");
            }
        }
    }

    /// <summary>
    /// Enum định nghĩa các tier crafting
    /// Tier cao hơn = weight cao hơn = tỷ lệ được chọn cao hơn
    /// </summary>
    public enum CraftingTier
    {
        Common = 1,     // Weight thấp nhất
        Uncommon = 2,   
        Rare = 3,       
        Epic = 4,       
        Legendary = 5,  
        Mythic = 6      // Weight cao nhất
    }

    [System.Serializable]
    public class CategoryRequirement
    {
        public string itemCategory;
        public int minCount;
        public CategoryRequirement(string category, int count)
        {
            itemCategory = category;
            minCount = count;
        }
    }

    [CreateAssetMenu(menuName = "InventoryPlus/CraftingRecipe")]
    public class CraftingRecipe : ScriptableObject
    {
        [Header("Recipe Info")]
        public string recipeName;
        public Item result;
        
        [Header("Tier System")]
        public CraftingTier tier = CraftingTier.Common;
        
        [Header("Requirements")]
        public List<CategoryRequirement> categoryRequirements;
        
        [Header("Display")]
        [TextArea(2, 4)]
        public string description;
        
        /// <summary>
        /// Lấy weight của recipe này
        /// </summary>
        public float GetWeight()
        {
            return tier switch
            {
                CraftingTier.Common => 1000f,
                CraftingTier.Uncommon => 20f,
                CraftingTier.Rare => 35f,
                CraftingTier.Epic => 55f,
                CraftingTier.Legendary => 80f,
                CraftingTier.Mythic => 100f,
                _ => 10f
            };
        }
        
        /// <summary>
        /// Tính tỷ lệ của recipe này trong danh sách recipes
        /// </summary>
        public float GetProbability(List<CraftingRecipe> allValidRecipes)
        {
            if (allValidRecipes == null || allValidRecipes.Count == 0)
                return 0f;

            float totalWeight = 0f;
            foreach (var recipe in allValidRecipes)
            {
                totalWeight += recipe.GetWeight();
            }
            Debug.Log($"Total Weight: {totalWeight}");
            Debug.Log($"This Recipe Weight: {GetWeight()}");
            return (GetWeight() / totalWeight) * 100f;
        }
        
        /// <summary>
        /// Lấy màu sắc của tier
        /// </summary>
        public Color GetTierColor()
        {
            return CraftingUtils.GetTierColor(tier);
        }
        
        /// <summary>
        /// Lấy tên hiển thị của tier
        /// </summary>
        public string GetTierDisplayName()
        {
            return CraftingUtils.GetTierDisplayName(tier);
        }

        // Thêm code này vào cuối class
        #if UNITY_EDITOR
        [ContextMenu("Refresh Asset")]
        public void RefreshAsset()
        {
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }

        [ContextMenu("Validate Recipe")]
        public void ValidateRecipe()
        {
            List<string> issues = new List<string>();

            if (string.IsNullOrEmpty(recipeName))
                issues.Add("• Recipe name is empty");

            if (result == null)
                issues.Add("• Result item is not assigned");

            if (categoryRequirements == null || categoryRequirements.Count == 0)
                issues.Add("• No category requirements defined");
            else
            {
                foreach (var req in categoryRequirements)
                {
                    if (string.IsNullOrEmpty(req.itemCategory))
                        issues.Add("• Empty item category found");
                    if (req.minCount <= 0)
                        issues.Add($"• Invalid min count for {req.itemCategory}");
                }
            }

            if (issues.Count == 0)
            {
                Debug.Log($"✓ Recipe '{recipeName}' is valid!");
            }
            else
            {
                Debug.LogWarning($"❌ Recipe '{recipeName}' has issues:\n" + string.Join("\n", issues));
            }
        }

        [ContextMenu("Generate Description")]
        public void GenerateDescription()
        {
            if (result != null)
            {
                description = $"Recipe to craft {result.name}. " +
                             $"Tier: {GetTierDisplayName()}. ";
                
                if (categoryRequirements != null && categoryRequirements.Count > 0)
                {
                    description += "Requirements: ";
                    for (int i = 0; i < categoryRequirements.Count; i++)
                    {
                        var req = categoryRequirements[i];
                        description += $"{req.minCount}x {req.itemCategory}";
                        if (i < categoryRequirements.Count - 1) description += ", ";
                    }
                }
                
                UnityEditor.EditorUtility.SetDirty(this);
                Debug.Log($"Generated description for '{recipeName}'");
            }
            else
            {
                Debug.LogWarning("Cannot generate description: Result item not assigned");
            }
        }
        #endif
    }
}
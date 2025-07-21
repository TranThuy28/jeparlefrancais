using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using InventoryPlus;

namespace InventoryPlus
{
    [RequireComponent(typeof(Button))]
    public class CraftButtonDirect : MonoBehaviour
    {
        [Header("References")]
        public Inventory inventory; // Tham chiếu đến Inventory để lấy item
        public UISlot[] inputSlots = new UISlot[4]; // 4 ô input
        public UISlot outputSlot; // Ô output
        public List<ItemSlot> inputItems = new List<ItemSlot>();
        public List<CraftingRecipe> recipes = new List<CraftingRecipe>(); // Danh sách công thức

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            if (button == null)
            {
                Debug.LogError("Button component not found on " + gameObject.name);
                return;
            }
            button.onClick.AddListener(TryCraft);
    
            SubscribeToSlotEvents();

            UpdateButtonState(); // Khởi tạo trạng thái nút
        }

        private void OnEnable()
        {
            SubscribeToSlotEvents(); 
            UpdateButtonState(); // Cập nhật khi nút được kích hoạt
        }

        private void OnDisable()
        {
            // Hủy đăng ký events khi disable
            UnsubscribeFromSlotEvents();
        }

        private void OnDestroy()
        {
            // Hủy đăng ký events khi destroy
            UnsubscribeFromSlotEvents();
        }

        private void SubscribeToSlotEvents()
        {
            InvokeRepeating(nameof(CheckSlotsAndUpdateButton), 0.5f, 0.5f);
        }
        
        private void UnsubscribeFromSlotEvents()
        {
            CancelInvoke(nameof(CheckSlotsAndUpdateButton));

            if (inputSlots != null)
            {
                foreach (var slot in inputSlots)
                {
                    if (slot != null)
                    {
                        // slot.OnSlotChanged -= OnSlotContentChanged;
                    }
                }
            }
        }

        // Phương thức này sẽ được gọi định kỳ để kiểm tra slot
        private void CheckSlotsAndUpdateButton()
        {
            Debug.Log("CheckSlotsAndUpdateButton() called at: " + Time.time);
            UpdateButtonState();
        }

        // Nếu có event từ UISlot, dùng phương thức này
        private void OnSlotContentChanged()
        {
            UpdateButtonState();
        }

        private void TryCraft()
        {
            try
            {
                RefreshInputItems(); // Cập nhật inputItems từ slots

                // Kiểm tra công thức
                var (isValid, result, failureReason) = CraftingUtils.CheckCraftingRecipe(inputItems, recipes);

                if (isValid && result != null)
                {
                    // Chỉ cập nhật output slot khi craft thành công
                    if (outputSlot != null)
                    {
                        try
                        {
                            // Tạo ItemSlot một cách an toàn
                            ItemSlot craftedItem = new ItemSlot(result, 1, 1f);
                            SafeUpdateOutputSlot(craftedItem);
                            Debug.Log($"Crafting Success! Created: {result.name}");
                            
                            // Sử dụng items từ input slots
                            foreach (var slot in inputSlots)
                            {
                                if (slot != null)
                                {
                                    inventory.UseItem(slot); // -1 item
                                }
                            }
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError($"Error creating crafted item: {e.Message}");
                        }
                    }
                }
                else
                {
                    // Chỉ log thất bại, không xóa output slot
                    Debug.Log($"Crafting Failed: {failureReason}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in TryCraft(): {e.Message}");
            }
            finally
            {
                // Luôn cập nhật trạng thái nút
                UpdateButtonState();
            }
        }

        private void RefreshInputItems()
        {
            inputItems.Clear();
            
            // Cập nhật inputItems từ các slot hiện tại
            foreach (var slot in inputSlots)
            {
                if (slot != null && inventory != null)
                {
                    ItemSlot itemSlot = inventory.GetInventorySlot(slot);
                    inputItems.Add(itemSlot);
                }
                else
                {
                    inputItems.Add(null); // Thêm null cho slot rỗng
                }
            }
        }

        // Phương thức an toàn để cập nhật output slot
        private void SafeUpdateOutputSlot(ItemSlot itemSlot)
        {
            if (outputSlot != null)
            {
                try
                {
                    outputSlot.UpdateUI(itemSlot, true, true);
                    int index = inventory.hotbarUISlots.IndexOf(outputSlot);
                    inventory.slots[index + 30] = itemSlot;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error updating output slot: {e.Message}");
                    // Nếu không thể update UI, ít nhất log lỗi
                }
            }
        }

        private void UpdateButtonState()
        {
            if (button == null) return;

            // Cập nhật inputItems trước khi kiểm tra
            RefreshInputItems();

            // Kiểm tra xem tất cả slots có được điền không
            bool allSlotsFilled = true;
            int filledSlots = 0;

            foreach (var itemSlot in inputItems)
            {
                if (itemSlot != null)
                {
                    filledSlots++;
                }
            }

            // Có thể điều chỉnh điều kiện này tùy theo yêu cầu
            // Ví dụ: cần tất cả 4 slots hoặc chỉ cần có ít nhất 1 slot
            allSlotsFilled = filledSlots >= 4; // Cần tất cả 4 slots

            // Thêm kiểm tra công thức để chắc chắn có thể craft
            bool canCraft = allSlotsFilled;
            // if (allSlotsFilled)
            // {
            //     try
            //     {
            //         var (isValid, result, failureReason) = CraftingUtils.CheckCraftingRecipe(inputItems, recipes);
            //         canCraft = isValid && result != null;
            //     }
            //     catch (System.Exception e)
            //     {
            //         Debug.LogError($"Error checking crafting recipe: {e.Message}");
            //         canCraft = false;
            //     }
            //}

            button.interactable = canCraft;
            
            if (!canCraft)
            {
                Debug.Log($"Craft button disabled: Filled slots: {filledSlots}/4");
            }
        }

        // Phương thức công khai để force update button từ bên ngoài
        public void ForceUpdateButton()
        {
            UpdateButtonState();
        }

        // Phương thức để thêm item vào slot thủ công
        public void AddItemToSlot(int index, ItemSlot itemSlot)
        {
            if (index >= 0 && index < inputSlots.Length && inputSlots[index] != null)
            {
                inputSlots[index].UpdateUI(itemSlot, true, true);
                UpdateButtonState();
            }
        }

        // Phương thức để xóa item khỏi slot
        public void RemoveItemFromSlot(int index)
        {
            if (index >= 0 && index < inputSlots.Length && inputSlots[index] != null)
            {
                inputSlots[index].UpdateUI(null, true, true);
                UpdateButtonState();
            }
        }
    }
}
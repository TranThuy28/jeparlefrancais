using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


namespace InventoryPlus
{
    public class Inventory : Storage
    {
        [SerializeField] public List<ItemSlot> inventoryItems;
        [SerializeField] public List<UISlot> inventoryUISlots;
        [SerializeField] public bool hasHotbar = false;
        [SerializeField] public List<UISlot> hotbarUISlots;

        [SerializeField] public bool displayNotificationOnNewItems;
        [SerializeField] public bool showDurabilityValues = false;
        [SerializeField] public bool fillReservedFirst;
        [SerializeField] public bool enableMouseDrag = true;

        [SerializeField] public Animator anim;
        
        [SerializeField] public bool instanciatePickuppableOnDrop = false;
        [SerializeField] public GameObject pickupPrefab;
        [SerializeField] public GameObject player;

        [SerializeField] public AudioSource itemsAudio;
        [SerializeField] public AudioSource sortAudio;        
        [SerializeField] public AudioSource swapAudio;

        [SerializeField] public bool enableDebug;


        private bool inChestRange = false;
        private bool isInventoryOpen = false;

        private List<ItemSlot> dropList = new List<ItemSlot>();
        private UISlot swapUISlot;
        private bool wasLoaded = false;


        /**/

        private void Update()
        {
            // Sử dụng Input System mới để kiểm tra phím I
            if (UnityEngine.InputSystem.Keyboard.current != null && 
                UnityEngine.InputSystem.Keyboard.current.iKey.wasPressedThisFrame)
            {
                ToggleInventory();
            }
        }

        // Thêm method để toggle inventory
        public void ToggleInventory()
        {
            isInventoryOpen = !isInventoryOpen;
            ShowInventory(isInventoryOpen);
            
            if (enableDebug) 
            {
                Debug.Log("Inventory " + (isInventoryOpen ? "opened" : "closed"));
            }
        }


        #region Setup

        private void Start()
        {
            if (!wasLoaded)
            {
                AssignHotbarSlots();
                AssignInventorySlots();

                AddStartingInventory();
            }
            ShowInventory(false);
        }


        public void LoadInventory(List<ItemSlot> _items)
        {
            slots.Clear();
            UISlots.Clear();

            AssignInventorySlots();
            AssignHotbarSlots();

            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] != null && _items[i].GetItemType() != null)
                {
                    if (!_items[i].GetItemType().isStackable)
                    {
                        slots[i] = new ItemSlot(_items[i].GetItemType(), 1, _items[i].GetItemDurability());
                        UISlots[i].UpdateUI(slots[i], showDurabilityValues, UISlots[i].enableNotification);
                    }
                    else
                    {
                        slots[i] = new ItemSlot(_items[i].GetItemType(), _items[i].GetItemNum(), 1f);
                        UISlots[i].UpdateUI(slots[i], showDurabilityValues, UISlots[i].enableNotification);
                    }
                }
            }

            wasLoaded = true;
            if (enableDebug) Debug.Log("Inventory loaded");
        }


        private void AssignInventorySlots()
        {
            for (int i = 0; i < inventoryUISlots.Count; i++)
            {
                UISlots.Add(inventoryUISlots[i]);
                slots.Add(null);

                if(enableMouseDrag) inventoryUISlots[i].SetupMouseDrag(this);
                inventoryUISlots[i].SetupUISlot(this);
            }
        }


        private void AssignHotbarSlots()
        {
            if (hasHotbar)
            {
                for (int i = 0; i < hotbarUISlots.Count; i++)
                {
                    UISlots.Add(hotbarUISlots[i]);
                    slots.Add(null);

                    if (enableMouseDrag) hotbarUISlots[i].SetupMouseDrag(this);
                    hotbarUISlots[i].SetupUISlot(this);
                }
            }
        }


        private void AddStartingInventory()
        {
            if (inventoryItems.Count > 0)
            {
                for (int i = 0; i < inventoryItems.Count; i++)
                {
                    AddInventory(inventoryItems[i].GetItemType(), inventoryItems[i].GetItemNum(), inventoryItems[i].GetItemDurability(), false);
                }
            }
        }

        #endregion


        #region AddToInventory

        public void AddInventory(Item _itemType, int _itemNum, float _itemDurability, bool _forceDisableNotification)
        {
            if(_itemType != null && _itemNum > 0)
            {
                if (!_itemType.isStackable) AddNotStackable(_itemType, _itemNum, _itemDurability, _forceDisableNotification);
                else AddStackable(_itemType, _itemNum, _forceDisableNotification);
            }
        }


        private void AddNotStackable(Item _itemType, int _itemNum, float _itemDurability, bool _forceDisableNotification)
        {
            bool notificationDisplay = displayNotificationOnNewItems;
            if (_forceDisableNotification) notificationDisplay = false;

            int remainingNum = _itemNum;


            if (fillReservedFirst)
            {
                //fill all empty reserved slots
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i] == null && UISlots[i].restrictedToCategory && UISlots[i].categoryName == _itemType.itemCategory)
                    {
                        slots[i] = new ItemSlot(_itemType, 1, _itemDurability);
                        UISlots[i].UpdateUI(slots[i], showDurabilityValues, notificationDisplay);

                        remainingNum--;
                        if (remainingNum == 0) break;
                    }
                }
            }


            if (remainingNum > 0)
            {
                //fill all empty slots
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i] == null && !UISlots[i].restrictedToCategory)
                    {
                        slots[i] = new ItemSlot(_itemType, 1, _itemDurability);
                        UISlots[i].UpdateUI(slots[i], showDurabilityValues, notificationDisplay);

                        remainingNum--;
                        if (remainingNum == 0) break;
                    }
                }
            }

            if (remainingNum > 0) dropList.Add(new ItemSlot(_itemType, _itemNum, _itemDurability));
            if (enableDebug) AddInventoryDebug(remainingNum, _itemNum, _itemType.itemName);
        }


        private void AddStackable(Item _itemType, int _itemNum, bool _forceDisableNotification)
        {
            bool notificationDisplay = displayNotificationOnNewItems;
            if (_forceDisableNotification) notificationDisplay = false;

            int remainingNum = _itemNum;


            //fill already partially filled slots
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null && slots[i].GetItemType() == _itemType)
                {
                    int slotFill = _itemType.stackSize - slots[i].GetItemNum();

                    if (remainingNum < slotFill)
                    {
                        slots[i].AddItemNum(remainingNum);
                        UISlots[i].UpdateUI(slots[i], showDurabilityValues, notificationDisplay);

                        remainingNum = 0;
                        break;
                    }
                    else
                    {
                        slots[i].AddItemNum(slotFill);
                        UISlots[i].UpdateUI(slots[i], showDurabilityValues, notificationDisplay);

                        remainingNum -= slotFill;
                    }
                }
            }


            //fill all empty slots
            if (remainingNum > 0)
            {
                if (fillReservedFirst)
                {
                    for (int i = 0; i < slots.Count; i++)
                    {
                        if ((slots[i] == null && remainingNum > _itemType.stackSize) && UISlots[i].restrictedToCategory && _itemType.itemCategory == UISlots[i].categoryName)
                        {
                            slots[i] = new ItemSlot(_itemType, _itemType.stackSize, 1f);
                            UISlots[i].UpdateUI(slots[i], showDurabilityValues, false);

                            remainingNum -= _itemType.stackSize;
                        }
                        else if ((slots[i] == null && remainingNum <= _itemType.stackSize) && UISlots[i].restrictedToCategory && _itemType.itemCategory == UISlots[i].categoryName)
                        {
                            slots[i] = new ItemSlot(_itemType, remainingNum, 1f);
                            UISlots[i].UpdateUI(slots[i], showDurabilityValues, false);

                            remainingNum = 0;
                            break;
                        }
                    }
                }

                if (remainingNum > 0)
                {
                    for (int i = 0; i < slots.Count; i++)
                    {
                        if ((slots[i] == null && remainingNum > _itemType.stackSize) && !UISlots[i].restrictedToCategory)
                        {
                            slots[i] = new ItemSlot(_itemType, _itemType.stackSize, 1f);
                            UISlots[i].UpdateUI(slots[i], showDurabilityValues, notificationDisplay);

                            remainingNum -= _itemType.stackSize;
                        }
                        else if ((slots[i] == null && remainingNum <= _itemType.stackSize) && !UISlots[i].restrictedToCategory)
                        {
                            slots[i] = new ItemSlot(_itemType, remainingNum, 1f);
                            UISlots[i].UpdateUI(slots[i], showDurabilityValues, notificationDisplay);

                            remainingNum = 0;
                            break;
                        }
                    }
                }
            }

            if (remainingNum > 0) dropList.Add(new ItemSlot(_itemType, remainingNum, 1f));
            if (enableDebug) AddInventoryDebug(remainingNum, _itemNum, _itemType.itemName);
        }


        private void AddInventoryDebug(int _remainingNum, int _itemNum, string _itemName)
        {
            if (_remainingNum == 0) Debug.Log("Added " + _itemNum + " " + _itemName + " to " + this.gameObject.name);
            else if (_remainingNum == _itemNum) Debug.Log("Nothing was added, " + this.gameObject.name + " is at full capacity");
            else Debug.Log("Added " + (_itemNum - _remainingNum) + " " + _itemName + " to " + this.gameObject.name + " until inventory capacity was reached");
        }

        #endregion


        #region RemoveFromInventory

        public void RemoveInventory(Item _itemType, int _itemNum)
        {
            if (!_itemType.isStackable) RemoveNotStackable(_itemType, _itemNum);
            else RemoveStackable(_itemType, _itemNum);
        }


        private void RemoveNotStackable(Item _itemType, int _itemNum)
        {
            int remainingNum = _itemNum;


            //remove item type from all slots
            for (int i = slots.Count - 1; i >= 0; i--)
            {
                if (slots[i] != null && slots[i].GetItemType() == _itemType)
                {
                    slots[i] = null;
                    UISlots[i].ShowUI(false);

                    remainingNum--;
                    if (remainingNum == 0) break;
                }
            }

            if (enableDebug) RemoveInventoryDebug(remainingNum, _itemNum, _itemType.itemName);
        }


        private void RemoveStackable(Item _itemType, int _itemNum)
        {
            int remainingNum = _itemNum;


            //remove from not empty slots
            for (int i = slots.Count - 1; i >= 0; i--)
            {
                if (slots[i] != null && slots[i].GetItemType() == _itemType)
                {
                    if (_itemNum >= slots[i].GetItemNum())
                    {
                        remainingNum -= slots[i].GetItemNum();

                        slots[i] = null;
                        UISlots[i].ShowUI(false);
                    }
                    else
                    {
                        slots[i].AddItemNum(-remainingNum);
                        UISlots[i].UpdateUI(slots[i], showDurabilityValues, displayNotificationOnNewItems);

                        remainingNum = 0;
                        break;
                    }
                }
            }

            if (enableDebug) RemoveInventoryDebug(remainingNum, _itemNum, _itemType.itemName);
        }


        private void RemoveInventoryDebug(int _remainingNum, int _itemNum, string _itemName)
        {
            if (_remainingNum == 0) Debug.Log("Removed " + _itemNum + " " + _itemName + " from " + this.gameObject.name);
            else if (_remainingNum == _itemNum) Debug.Log("Nothing was removed, there is no such item");
            else Debug.Log("Removed " + (_itemNum - _remainingNum) + " " + _itemName + " until the inventory run out of that item");
        }

        #endregion


        #region RemoveBlock

        public void RemoveCategory(string _category)
        {
            int remainingNum = 0;


            //remove slots that contain category
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null && slots[i].GetItemType().itemCategory == _category)
                {
                    slots[i] = null;
                    UISlots[i].ShowUI(false);

                    remainingNum++;
                }
            }

            if(enableDebug) Debug.Log("Removed " + remainingNum + " from the inventory");
        }


        public void RemoveID(string _ID)
        {
            int remainingNum = 0;


            //remove slots that contain ID
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null && slots[i].GetItemType().itemID == _ID)
                {
                    slots[i] = null;
                    UISlots[i].ShowUI(false);

                    remainingNum++;
                }
            }

            if (enableDebug) Debug.Log("Removed " + remainingNum + " from the inventory");
        }


        public void ClearInventory()
        {
            //remove everything
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null)
                {
                    slots[i] = null;
                    UISlots[i].ShowUI(false);
                }
            }

            if (enableDebug) Debug.Log("Cleared inventory");
        }

        #endregion


        #region Use / Drop / Equip

        public void UseItem(UISlot _UIslot)
        {
            ItemSlot slot = GetInventorySlot(_UIslot);
            bool isItemUsable = true;

            if (slot != null)
            {
                if (slot.GetItemType().isStackable)
                {
                    if (slot.GetItemNum() > 1)
                    {
                        //use stackable
                        slots[UISlots.IndexOf(_UIslot)].AddItemNum(-1);
                        _UIslot.UpdateUI(slot, showDurabilityValues, false);
                    }
                    else
                    {
                        //remove stackable
                        _UIslot.ShowUI(false);
                        slots[UISlots.IndexOf(_UIslot)] = null;
                    }
                }
                else
                {
                    if (slot.GetItemType().isDurable)
                    {
                        if (slot.GetItemDurability() - slot.GetItemType().usageCost > 0)
                        {
                            //use not stackable
                            slots[UISlots.IndexOf(_UIslot)].SetItemDurability(slot.GetItemDurability() - slot.GetItemType().usageCost);
                            _UIslot.UpdateUI(slot, showDurabilityValues, false);
                        }
                        else
                        {
                            //remove not stackable
                            slots[UISlots.IndexOf(_UIslot)] = null;
                            _UIslot.ShowUI(false);
                        }
                    }
                    else isItemUsable = false;
                }

                if(isItemUsable)
                {
                    if (itemsAudio != null && slot.GetItemType().useAudio != null) PlayItemsAudio(slot.GetItemType().useAudio);
                    if (enableDebug) Debug.Log("Used " + slot.GetItemType().itemName);
                }
                else if (enableDebug) Debug.Log("Can't use an item with no durability");
            }

            else if (enableDebug) Debug.Log("Not selecting an item, can't perform the Use action");
        }


        public void DropItem(UISlot _UISlot)
        {
            ItemSlot slot = GetInventorySlot(_UISlot);

            if (slot != null)
            {
                //instanciate pickup
                if (instanciatePickuppableOnDrop) dropList.Add(slots[UISlots.IndexOf(_UISlot)]);

                //remove selected
                _UISlot.ShowUI(false);
                slots[UISlots.IndexOf(_UISlot)] = null;

                if (itemsAudio != null && slot.GetItemType().dropAudio != null) PlayItemsAudio(slot.GetItemType().dropAudio);
                if (enableDebug) Debug.Log("Dropped " + slot.GetItemType().itemName);
            }

            else if (enableDebug) Debug.Log("Not selecting an item, can't perform the Drop action");
        }


        public void EquipItem(UISlot _UIinvetorySlot)
        {
            ItemSlot slot = GetInventorySlot(_UIinvetorySlot);
            bool equippedSelected = false;

            if (slot != null)
            {
                //loop and equip selected if possible
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i] == null && UISlots[i].restrictedToCategory && UISlots[i].categoryName == slot.GetItemType().itemCategory)
                    {
                        slots[i] = slot;
                        UISlots[i].UpdateUI(slots[i], showDurabilityValues, displayNotificationOnNewItems);

                        slots[UISlots.IndexOf(_UIinvetorySlot)] = null;
                        _UIinvetorySlot.ShowUI(false);

                        equippedSelected = true;
                        break;
                    }
                }
            }

            if (equippedSelected && itemsAudio != null && slot.GetItemType().equipAudio != null) PlayItemsAudio(slot.GetItemType().equipAudio);
            
            if (enableDebug)
            {
                if (equippedSelected) Debug.Log("Equipped selected slot");
                else Debug.Log("Could not equip selected slot");
            }
        }


        private void PlayItemsAudio(AudioClip _audioClip)
        {
            itemsAudio.clip = _audioClip;
            itemsAudio.Play();
        }

        #endregion


        #region Swap / Sort

        public void SwapItem(UISlot _UIInventorySlot)
        {
            if (swapUISlot == null)
            {
                swapUISlot = _UIInventorySlot;
                _UIInventorySlot.SetSwapState(true);
            }
            else if(_UIInventorySlot == swapUISlot)
            {
                swapUISlot = null;
                _UIInventorySlot.SetSwapState(false);
            }
            else
            {
                Storage slotContainer_1 = swapUISlot.GetSlotOwner();
                int index_1 = slotContainer_1.GetItemIndex(swapUISlot);
                ItemSlot itemSlot_1 = slotContainer_1.GetItemSlot(index_1);

                Storage slotContainer_2 = _UIInventorySlot.GetSlotOwner();
                int index_2 = slotContainer_2.GetItemIndex(_UIInventorySlot);
                ItemSlot itemSlot_2 = slotContainer_2.GetItemSlot(index_2);

                //attempt swap on restricted UIslots
                if ((swapUISlot.restrictedToCategory && (itemSlot_2 != null && itemSlot_2.GetItemType().itemCategory != swapUISlot.categoryName)) || (_UIInventorySlot.restrictedToCategory && (itemSlot_1 != null && itemSlot_1.GetItemType().itemCategory != _UIInventorySlot.categoryName)))
                {
                    if (itemSlot_1 != null) swapUISlot.UpdateUI(itemSlot_1, showDurabilityValues, false);
                    else swapUISlot.ShowUI(false);

                    if (itemSlot_2 != null) _UIInventorySlot.UpdateUI(itemSlot_2, showDurabilityValues, false);
                    else _UIInventorySlot.ShowUI(false);
                }
                
                //attempt swap
                else
                {
                    slotContainer_1.SetItemSlot(index_1, itemSlot_2);
                    slotContainer_2.SetItemSlot(index_2, itemSlot_1);

                    if (itemSlot_2 != null) swapUISlot.UpdateUI(itemSlot_2, showDurabilityValues, false);
                    else swapUISlot.ShowUI(false);

                    if (itemSlot_1 != null) _UIInventorySlot.UpdateUI(itemSlot_1, showDurabilityValues, false);
                    else _UIInventorySlot.ShowUI(false);
                }

                swapUISlot = null;
                if (swapAudio != null) swapAudio.Play();

                if (enableDebug) Debug.Log("Items swapped");
            }
        }


        public void Sort()
        {
            List<ItemSlot> tmpInventory = new List<ItemSlot>(slots);
            tmpInventory = tmpInventory.GetRange(0, inventoryUISlots.Count);

            List<ItemSlot> tmpHotbar = new List<ItemSlot>(slots);
            tmpHotbar = tmpHotbar.GetRange(inventoryUISlots.Count, hotbarUISlots.Count);

            //clear slots
            for (int i = 0; i < slots.Count; i++)
            {
                UISlots[i].ShowUI(false);
                slots[i] = null;
            }

            //assign hotbar
            for (int i = 0; i < tmpHotbar.Count; i++)
            {
                if (tmpHotbar[i] != null)
                {
                    slots[i + (inventoryUISlots.Count)] = tmpHotbar[i];
                    UISlots[i + (inventoryUISlots.Count)].UpdateUI(slots[i + (inventoryUISlots.Count)], showDurabilityValues, false);
                }
            }

            //sort inventory
            for (int i = 0; i < tmpInventory.Count; i++)
            {
                if (tmpInventory[i] != null) AddInventory(tmpInventory[i].GetItemType(), tmpInventory[i].GetItemNum(), tmpInventory[i].GetItemDurability(), true);
            }

            if (sortAudio != null) sortAudio.Play();
            if (enableDebug) Debug.Log("Inventory sorted");
        }


        public void ClearSwap()
        {
            if (swapUISlot != null) swapUISlot.SetSwapState(false);
            swapUISlot = null;
        }


        public void ForceEndSwap()
        {
            if (swapUISlot != null)
            {
                swapUISlot.SetSwapState(false);
                swapUISlot.ForceEndMouseDrag();
            }
            swapUISlot = null;
        }

        #endregion


        #region Utils

        public void SelectFirstInventorySlot()
        {
            inventoryUISlots[0].gameObject.GetComponent<Button>().Select();
            inventoryUISlots[0].gameObject.GetComponent<Button>().OnSelect(null);
        }


        public void SelectFirstHotbarSlot()
        {
            hotbarUISlots[0].gameObject.GetComponent<Button>().Select();
            hotbarUISlots[0].gameObject.GetComponent<Button>().OnSelect(null);
        }


        public void ShowInventory(bool _show)
        {
            if(_show) anim.Rebind();

            //handle pickuppable instances
            if (instanciatePickuppableOnDrop && dropList.Count > 0)
            {
                GameObject instObj = GameObject.Instantiate(pickupPrefab, player.transform.position, Quaternion.identity);
                instObj.GetComponent<PickUp>().ManageDrop(dropList, false);

                dropList.Clear();
            }

            anim.SetBool("Show", _show);
            anim.SetBool("hasChest", inChestRange);
        }


        public void DropExtra()
        {
            if (dropList.Count > 0)
            {
                GameObject instObj = GameObject.Instantiate(pickupPrefab, player.transform.position, Quaternion.identity);
                instObj.GetComponent<PickUp>().ManageDrop(dropList, false);

                dropList.Clear();
            }
        }


        public void InChestRange(bool _inChestRange)
        {
            inChestRange = _inChestRange;
        }


        #endregion


        #region Getters

        public ItemSlot GetInventorySlot(UISlot _UIInventorySlot)
        {
            int index = UISlots.IndexOf(_UIInventorySlot);

            if (index < 0) return null;
            else return slots[index];
        }


        public List<ItemSlot> GetInventoryContent()
        {
            return slots;
        }

        #endregion
    }
}
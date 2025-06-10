using System.Collections.Generic;
using UnityEngine;


namespace InventoryPlus
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Chest : Storage
    {
        [Header("Content")]
        public List<ItemSlot> chestItems;
        public List<UISlot> chestUISlots;

        [Space(15)]
        [Header("References")]
        public Transform chest;
        public Transform parentUI;
        public string playerTag = "Player";

        [Space(15)]
        public bool enableDebug = true;


        private Inventory inventory;
        private BoxCollider2D col;

        private bool showDurabilityValues = false;


        /**/


        #region Setup

        private void Start()
        {
            col = this.GetComponent<BoxCollider2D>();
            UISlots = chestUISlots;

            SetupChestSlots();
            AddChestItems();
        }


        private void SetupChestSlots()
        {
            for (int i = 0; i < UISlots.Count; i++)
            {
                UISlots[i].SetupUISlot(this);
                slots.Add(null);
            }
        }


        private void AddChestItems()
        {
            if (chestItems.Count > 0)
            {
                for (int i = 0; i < chestItems.Count; i++)
                {
                    AddToChest(chestItems[i].GetItemType(), chestItems[i].GetItemNum(), chestItems[i].GetItemDurability());
                }
            }
        }

        #endregion


        #region Interaction

        public void TriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag(playerTag))
            {
                //set chest as inventory child
                PlayerController player = collision.GetComponent<PlayerController>();
                chest.SetParent(player.inventory.transform);
                player.inventory.InChestRange(true);

                showDurabilityValues = player.inventory.showDurabilityValues;
                for (int i = 0; i < UISlots.Count; i++)
                {
                    if (player.inventory.enableMouseDrag) UISlots[i].SetupMouseDrag(player.inventory);
                    if (slots[i] != null) UISlots[i].UpdateUI(slots[i], showDurabilityValues, false);
                }
            }
        }


        public void TriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag(playerTag))
            {
                PlayerController player = collision.GetComponent<PlayerController>();
                chest.SetParent(parentUI);
                player.inventory.InChestRange(false);
            }
        }


        public void TriggerEnter3D(Collider collision)
        {
            if (collision.CompareTag(playerTag))
            {
                //set chest as inventory child
                PlayerController player = collision.GetComponent<PlayerController>();
                chest.SetParent(player.inventory.transform);
                player.inventory.InChestRange(true);

                showDurabilityValues = player.inventory.showDurabilityValues;
                for (int i = 0; i < UISlots.Count; i++)
                {
                    if (player.inventory.enableMouseDrag) UISlots[i].SetupMouseDrag(player.inventory);
                    if (slots[i] != null) UISlots[i].UpdateUI(slots[i], showDurabilityValues, false);
                }
            }
        }


        public void TriggerExit3D(Collider collision)
        {
            if (collision.CompareTag(playerTag))
            {
                PlayerController player = collision.GetComponent<PlayerController>();
                chest.SetParent(parentUI);
                player.inventory.InChestRange(false);
            }
        }

        #endregion


        #region AddToChest

        public void AddToChest(Item _itemType, int _itemNum, float _itemDurability)
        {
            if (_itemType != null && _itemNum > 0)
            {
                if (!_itemType.isStackable) AddNotStackable(_itemType, _itemNum, _itemDurability);
                else AddStackable(_itemType, _itemNum);
            }
        }


        private void AddNotStackable(Item _itemType, int _itemNum, float _itemDurability)
        {
            int remainingNum = _itemNum;

            //fill all empty slots
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] == null)
                {
                    slots[i] = new ItemSlot(_itemType, 1, _itemDurability);
                    UISlots[i].UpdateUI(slots[i], showDurabilityValues, false);

                    remainingNum--;
                    if (remainingNum == 0) break;
                }
            }

            if (enableDebug) AddToChestDebug(remainingNum, _itemNum, _itemType.itemName);
        }


        private void AddStackable(Item _itemType, int _itemNum)
        {
            int remainingNum = _itemNum;

            //fill all empty slots
            for (int i = 0; i < slots.Count; i++)
            {
                if ((slots[i] == null && remainingNum > _itemType.stackSize) && (!UISlots[i].restrictedToCategory || (UISlots[i].restrictedToCategory && _itemType.itemCategory == UISlots[i].categoryName)))
                {
                    slots[i] = new ItemSlot(_itemType, _itemType.stackSize, 1f);
                    UISlots[i].UpdateUI(slots[i], showDurabilityValues, false);

                    remainingNum -= _itemType.stackSize;
                }
                else if ((slots[i] == null && remainingNum <= _itemType.stackSize) && (!UISlots[i].restrictedToCategory || (UISlots[i].restrictedToCategory && _itemType.itemCategory == UISlots[i].categoryName)))
                {
                    slots[i] = new ItemSlot(_itemType, remainingNum, 1f);
                    UISlots[i].UpdateUI(slots[i], showDurabilityValues, false);

                    remainingNum = 0;
                    break;
                }
            }

            if (enableDebug) AddToChestDebug(remainingNum, _itemNum, _itemType.itemName);
        }


        private void AddToChestDebug(int _remainingNum, int _itemNum, string _itemName)
        {
            if (_remainingNum == 0) Debug.Log("Added " + _itemNum + " " + _itemName + " to " + this.gameObject.name);
            else if (_remainingNum == _itemNum) Debug.Log("Nothing was added, " +  this.gameObject.name + " is at full capacity");
            else Debug.Log("Added " + (_itemNum - _remainingNum) + " " + _itemName + " to " + this.gameObject.name  + " until chest capacity was reached");
        }

        #endregion


        #region Utils

        private void OnDrawGizmos()
        {
            if (enableDebug)
            {
                if (col == null) col = this.GetComponent<BoxCollider2D>();

                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(transform.position + (Vector3)col.offset, col.size);
            }
        }

        #endregion
    }
}
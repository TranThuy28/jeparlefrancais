using System;
using UnityEngine;


namespace InventoryPlus
{
    [Serializable]
    public class ItemSlot
    {
        [SerializeField] private Item itemType;
        [SerializeField] [Min(1)] private int itemNum = 1;
        [SerializeField] [Min(1f)] private float itemRawDurability = 100f;


        /**/


        public ItemSlot(Item _itemType, int _itemNum, float _itemRawDurability)
        {
            itemType = _itemType;
            itemNum = _itemNum;
            
            if (_itemRawDurability > _itemType.maxDurability) itemRawDurability = _itemType.maxDurability;
            else itemRawDurability = _itemRawDurability; 
        }


        public Item GetItemType() { return itemType; }


        public int GetItemNum() { return itemNum; }
        public void AddItemNum(int _itemNum) { itemNum += _itemNum; }


        public float GetItemDurability() { return itemRawDurability; }
        public void SetItemDurability(float _itemDur) { itemRawDurability = _itemDur; }
    }
}
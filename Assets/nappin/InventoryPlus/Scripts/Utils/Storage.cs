using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryPlus
{
    public class Storage : MonoBehaviour
    {
        protected List<UISlot> UISlots = new List<UISlot>();
        protected List<ItemSlot> slots = new List<ItemSlot>();


        /**/


        public int GetItemIndex(UISlot _UISlot) { return UISlots.IndexOf(_UISlot); }
        public ItemSlot GetItemSlot(int _index) { return slots[_index]; }
        public void SetItemSlot(int _index, ItemSlot _itemSlot) { slots[_index] = _itemSlot; }
        public List<ItemSlot> GetSlots() { return slots; }
    }
}
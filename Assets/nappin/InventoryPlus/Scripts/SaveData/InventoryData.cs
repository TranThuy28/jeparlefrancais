using System;
using System.Collections.Generic;


namespace InventoryPlus
{
    [Serializable]
    public class InventoryData
    {
        public List<ItemSlot> items;


        /**/


        public InventoryData(List<ItemSlot> _items)
        {
            this.items = _items;
        }
    }
}
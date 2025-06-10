using System;
using System.Collections.Generic;


namespace InventoryPlus
{
    [Serializable]
    public class PickUpData
    {
        public string instanceName;
        public float positionX;
        public float positionY;
        public List<ItemSlot> itemsAdd;
        public List<ItemSlot> itemsRemove;


        /**/


        public PickUpData(string _instanceName, float _positionX, float _positionY, List<ItemSlot> _itemsAdd, List<ItemSlot> _itemsRemove)
        {
            this.instanceName = _instanceName;

            this.positionX = _positionX;
            this.positionY = _positionY;

            this.itemsAdd = _itemsAdd;
            this.itemsRemove = _itemsRemove;
        }
    }
}
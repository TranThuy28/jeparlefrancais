using System;
using System.Collections.Generic;


namespace InventoryPlus
{
    [Serializable]
    public class ChestData
    {
        public string instanceName;
        public float positionX;
        public float positionY;
        public List<ItemSlot> items;


        /**/


        public ChestData(string _instanceName, float _positionX, float _positionY, List<ItemSlot> _items)
        {
            this.instanceName = _instanceName;

            this.positionX = _positionX;
            this.positionY = _positionY;

            this.items = _items;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace InventoryPlus
{
    public class Test : MonoBehaviour
    {
        public Inventory inventory;
        [SerializeField] public Item swordItem; // Assign trong Inspector
        
        private void Start()
        {
            // Kiểm tra xem inventory đã được gán hay chưa
            if (inventory == null)
            {
                Debug.LogError("Inventory is not assigned!");
                return;
            }
            inventory.AddInventory(swordItem, 1, 100f, false);
        }
    }
}
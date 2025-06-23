using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryPlus
{
    public class Spawn : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] public ItemSpawnManager spawner;
        
        [SerializeField] public Item item;
        
        private void Start()
        {
            Debug.Log("Spawn script bắt đầu chạy");

            if (spawner == null)
            {
                Debug.LogError("Spawner chưa được gán!");
                return;
            }

if(item.itemPrefab == null)
            {
                Debug.LogError("Item prefab chưa được gán!");
                return;
            }
            spawner.SpawnItem(item/*, new Vector3(27.42f, 15.5f, -17.89f)*/);
        }
    }
}
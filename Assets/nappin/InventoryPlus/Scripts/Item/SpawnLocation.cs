using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace InventoryPlus
{
    // 2. SpawnLocation - MonoBehaviour chỉ định vị trí spawn
    public class SpawnLocation : MonoBehaviour
    {
        [SerializeField] private bool isActive = true;
        [SerializeField] private List<string> allowedCategories = new List<string>();
        [SerializeField] private List<Item.ItemChapter> allowedChapters = new List<Item.ItemChapter>();
        [SerializeField] private float spawnRadius = 1f;
        [SerializeField] private LayerMask obstacleLayerMask = 1;

        public bool IsActive => isActive;
        public float SpawnRadius => spawnRadius;

        public bool IsValidForSpawning(Item item = null)
        {
            if (!isActive) return false;

            if (item != null)
            {
                // Kiểm tra category
                if (allowedCategories.Count > 0 && !allowedCategories.Contains(item.itemCategory))
                    return false;

                // Kiểm tra chapter
                if (allowedChapters.Count > 0 && !allowedChapters.Contains(item.itemChapter))
                    return false;
            }

            // Kiểm tra có vật cản không
            return !Physics.CheckSphere(transform.position, spawnRadius, obstacleLayerMask);
        }

        public Vector3 GetRandomSpawnPosition()
        {
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
            
            // Raycast từ trên cao xuống để tìm mặt đất
            RaycastHit hit;
            float raycastHeight = 100f; // Độ cao bắt đầu raycast
            Vector3 raycastStart = new Vector3(spawnPos.x, transform.position.y + raycastHeight, spawnPos.z);
            
            if (Physics.Raycast(raycastStart, Vector3.down, out hit, raycastHeight * 2))
            {
                // Trả về vị trí trên mặt đất
                return hit.point;
            }
            
            // Fallback: trả về vị trí gốc nếu không tìm thấy mặt đất
            return spawnPos;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = isActive ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
    }
}
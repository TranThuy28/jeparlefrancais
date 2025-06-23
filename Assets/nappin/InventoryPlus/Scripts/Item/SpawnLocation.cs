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
            return transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = isActive ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
    }
}
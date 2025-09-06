using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace InventoryPlus
{
    // 8. ItemSpawnManager - MonoBehaviour chính
    public class ItemSpawnManager : MonoBehaviour
    {
        [Header("Database & Locations")]
        [SerializeField] private ItemDatabase itemDatabase;
        [SerializeField] private List<SpawnLocation> spawnLocations = new List<SpawnLocation>();

        [Header("Spawn Settings")]
        [SerializeField] public int maxActiveItems = 3;
        [SerializeField] private float autoSpawnInterval = 5f;
        [SerializeField] private bool enableAutoSpawn = true;

        [Header("Filters")]
        [SerializeField] private ChapterSpawnFilter chapterFilter = new ChapterSpawnFilter();

        [Header("Effects")]
        [SerializeField] private SpawnEffects spawnEffects = new SpawnEffects();

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        // Events
        public System.Action<GameObject, Item> OnItemSpawned;
        public System.Action<GameObject, Item> OnItemDespawned;

        // Private fields
        private ItemPool itemPool = new ItemPool();
        private List<SpawnedItemTracker> spawnedItems = new List<SpawnedItemTracker>();
        private float lastAutoSpawnTime;

        void Start()
        {
            Initialize();
        }

        void Update()
        {
            if (enableAutoSpawn && Time.time - lastAutoSpawnTime >= autoSpawnInterval)
            {
                AutoSpawn();
                lastAutoSpawnTime = Time.time;
            }
        }

        private void Initialize()
        {
            // Tạo pool parent
            Transform poolParent = new GameObject("ItemPool").transform;
            poolParent.SetParent(transform);
            itemPool.Initialize(poolParent);

            // Validate configuration
            ValidateConfiguration();

            if (showDebugLogs)
            {
               // Debug.Log($"ItemSpawnManager initialized with {spawnLocations.Count} spawn locations");
            }
        }

        private void ValidateConfiguration()
        {
            if (itemDatabase == null)
            {
                Debug.LogError("ItemDatabase is not assigned!", this);
                return;
            }

            if (spawnLocations.Count == 0)
            {
                Debug.LogWarning("No spawn locations assigned!", this);
            }

            // Kiểm tra prefab missing
            foreach (Item item in itemDatabase.AllItems)
            {
                if (item.itemPrefab == null)
                {
                    Debug.LogWarning($"Item '{item.itemName}' is missing prefab!", this);
                }
            }
        }

        // Spawn item theo ID
        public GameObject SpawnItemByID(string itemID, Vector3? position = null)
        {
            Item item = itemDatabase.GetItemByID(itemID);
            if (item == null)
            {
                Debug.LogWarning($"Item with ID '{itemID}' not found in database");
                return null;
            }

            return SpawnItem(item, position);
        }

        // Spawn item theo Category
        public GameObject SpawnItemByCategory(string category, Vector3? position = null)
        {
            List<Item> items = itemDatabase.GetItemsByCategory(category);
            items = chapterFilter.FilterItems(items);

            if (items.Count == 0)
            {
                Debug.LogWarning($"No items found in category '{category}' for current chapter");
                return null;
            }

            Item selectedItem = WeightedItemSpawner.GetRandomWeightedItem(items);
            return SpawnItem(selectedItem, position);
        }

        // Spawn item theo Rarity
        public GameObject SpawnItemByRarity(int rarity, Vector3? position = null)
        {
            List<Item> items = itemDatabase.GetItemsByRarity(rarity);
            items = chapterFilter.FilterItems(items);

            if (items.Count == 0)
            {
                Debug.LogWarning($"No items found with rarity '{rarity}' for current chapter");
                return null;
            }

            Item selectedItem = items[UnityEngine.Random.Range(0, items.Count)];
            return SpawnItem(selectedItem, position);
        }

        // Spawn ngẫu nhiên có trọng số
        public GameObject SpawnRandomWeightedItem(Vector3? position = null)
        {
            List<Item> availableItems = chapterFilter.FilterItems(itemDatabase.AllItems);

            if (availableItems.Count == 0)
            {
                Debug.LogWarning("No items available for spawning in current chapter");
                return null;
            }

            Item selectedItem = WeightedItemSpawner.GetRandomWeightedItem(availableItems);
            return SpawnItem(selectedItem, position);
        }

        // Spawn item chính
        public GameObject SpawnItem(Item item, Vector3? position = null)
        {
            if (item == null /*|| item.itemPrefab == null*/)
            {
                Debug.LogWarning("Cannot spawn: Item or prefab is null");
                return null;
            }

            if (spawnedItems.Count >= maxActiveItems)
            {
                Debug.LogWarning($"Maximum active items ({maxActiveItems}) reached");
                return null;
            }

            if (!chapterFilter.CanSpawnItem(item))
            {
                Debug.LogWarning($"Item '{item.itemName}' cannot be spawned in current chapter");
                return null;
            }

            // Xác định vị trí spawn
            Vector3 spawnPosition;
            SpawnLocation spawnLocation = null;

            if (position.HasValue)
            {
                spawnPosition = position.Value;
            }
            else
            {
                spawnLocation = GetValidSpawnLocation(item);
                if (spawnLocation == null)
                {
                    Debug.LogWarning($"No valid spawn location found for item '{item.itemName}'");
                    return null;
                }
                spawnPosition = spawnLocation.GetRandomSpawnPosition();
            }

            // Spawn item từ pool
            GameObject spawnedObject = itemPool.GetPooledItem(item);
            if (spawnedObject == null)
            {
                Debug.LogError($"Failed to get pooled item for '{item.itemName}'");
                return null;
            }

            spawnedObject.transform.position = spawnPosition;
            spawnedObject.transform.rotation = Quaternion.identity;

            spawnedObject.AddComponent<ItemCollectable>();

            // Thêm vào tracker
            SpawnedItemTracker tracker = new SpawnedItemTracker(spawnedObject, item, spawnLocation);
            spawnedItems.Add(tracker);

            // Phát hiệu ứng
            spawnEffects.PlaySpawnEffect(spawnPosition, item);

            // Gọi event
            OnItemSpawned?.Invoke(spawnedObject, item);

            if (showDebugLogs)
            {
              //  Debug.Log($"Spawned item '{item.itemName}' at position {spawnPosition}");
            }

            return spawnedObject;
        }

        // Auto spawn
        private void AutoSpawn()
        {
            if (spawnedItems.Count >= maxActiveItems) return;

            SpawnRandomWeightedItem();
        }

        // Tìm spawn location hợp lệ
        private SpawnLocation GetValidSpawnLocation(Item item)
        {
            List<SpawnLocation> validLocations = spawnLocations
                .Where(loc => loc.IsValidForSpawning(item))
                .ToList();
            Debug.Log($"Found {validLocations.Count} valid spawn locations for item '{item.itemName}'");
            if (validLocations.Count == 0) return null;

            return validLocations[UnityEngine.Random.Range(0, validLocations.Count)];
        }

        // Despawn item
        public void DespawnItem(GameObject itemObject)
        {
            SpawnedItemTracker tracker = spawnedItems.FirstOrDefault(t => t.gameObject == itemObject);
            if (tracker == null)
            {
                Debug.LogWarning("Trying to despawn item that's not tracked");
                return;
            }

            // Return to pool
            itemPool.ReturnToPool(itemObject, tracker.item.itemID);

            // Remove from tracker
            spawnedItems.Remove(tracker);

            // Gọi event
            OnItemDespawned?.Invoke(itemObject, tracker.item);

            if (showDebugLogs)
            {
                Debug.Log($"Despawned item '{tracker.item.itemName}'");
            }
        }

        // Xoá tất cả item đang spawn
        public void ClearAllSpawnedItems()
        {
            for (int i = spawnedItems.Count - 1; i >= 0; i--)
            {
                if (spawnedItems[i].gameObject != null)
                {
                    DespawnItem(spawnedItems[i].gameObject);
                }
            }

            spawnedItems.Clear();

            if (showDebugLogs)
            {
                Debug.Log("Cleared all spawned items");
            }
        }

        // Getters
        public int GetActiveItemCount() => spawnedItems.Count;
        public List<SpawnedItemTracker> GetSpawnedItems() => new List<SpawnedItemTracker>(spawnedItems);
        public ChapterSpawnFilter GetChapterFilter() => chapterFilter;

        // Public methods for external control
        public void SetCurrentChapter(Item.ItemChapter chapter)
        {
            chapterFilter.CurrentChapter = chapter;
        }

        public void SetAutoSpawn(bool enabled)
        {
            enableAutoSpawn = enabled;
        }

        public void SetMaxActiveItems(int max)
        {
            maxActiveItems = Mathf.Max(0, max);
        }

        void OnDestroy()
        {
            itemPool.ClearPool();
        }
    }

    // Editor Tool cho ItemSpawnManager
    // [CustomEditor(typeof(ItemSpawnManager))]
    // public class ItemSpawnManagerEditor : Editor
    // {
    //     public override void OnInspectorGUI()
    //     {
    //         DrawDefaultInspector();

    //         ItemSpawnManager manager = (ItemSpawnManager)target;

    //         if (!Application.isPlaying) return;

    //         EditorGUILayout.Space();
    //         EditorGUILayout.LabelField("Runtime Info", EditorStyles.boldLabel);
    //         EditorGUILayout.LabelField($"Active Items: {manager.GetActiveItemCount()}/{manager.maxActiveItems}");

    //         EditorGUILayout.Space();
    //         EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);

    //         if (GUILayout.Button("Spawn Random Item"))
    //         {
    //             manager.SpawnRandomWeightedItem();
    //         }

    //         if (GUILayout.Button("Clear All Items"))
    //         {
    //             manager.ClearAllSpawnedItems();
    //         }

    //         EditorGUILayout.Space();

    //         // Chapter control
    //         EditorGUILayout.LabelField("Chapter Control", EditorStyles.boldLabel);
    //         Item.ItemChapter newChapter = (Item.ItemChapter)EditorGUILayout.EnumPopup("Current Chapter", manager.GetChapterFilter().CurrentChapter);
    //         if (newChapter != manager.GetChapterFilter().CurrentChapter)
    //         {
    //             manager.SetCurrentChapter(newChapter);
    //         }
    //     }
    // }
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
namespace InventoryPlus
{
    // 1. ItemDatabase - ScriptableObject chứa danh sách tất cả các Item
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "InventoryPlus/ItemDatabase", order = 0)]
    public class ItemDatabase : ScriptableObject
    {
        [SerializeField] private List<Item> allItems = new List<Item>();

        public List<Item> AllItems => allItems;

        public Item GetItemByID(string itemID)
        {
            return allItems.FirstOrDefault(item => item.itemID == itemID);
        }

        public List<Item> GetItemsByCategory(string category)
        {
            return allItems.Where(item => item.itemCategory == category).ToList();
        }

        public List<Item> GetItemsByRarity(int rarity)
        {
            return allItems.Where(item => item.itemRarity == rarity).ToList();
        }

        public List<Item> GetItemsByChapter(Item.ItemChapter chapter)
        {
            return allItems.Where(item => item.itemChapter == chapter).ToList();
        }

        public void AddItem(Item item)
        {
            if (!allItems.Contains(item))
            {
                allItems.Add(item);
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        public void RemoveItem(Item item)
        {
            if (allItems.Remove(item))
            {
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Auto Load All Items")]
        public void AutoLoadAllItems()
        {
            string[] guids = AssetDatabase.FindAssets("t:Item");
            allItems.Clear();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Item item = AssetDatabase.LoadAssetAtPath<Item>(path);
                if (item != null)
                {
                    allItems.Add(item);
                }
            }

            EditorUtility.SetDirty(this);
            Debug.Log($"Auto loaded {allItems.Count} items into database");
        }
#endif
    }

    // 3. ItemPool - Object pooling
    [System.Serializable]
    public class ItemPool
    {
        private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();
        private Transform poolParent;

        public void Initialize(Transform parent)
        {
            poolParent = parent;
        }

        public GameObject GetPooledItem(Item item)
        {
            if (item == null || item.itemPrefab == null)
            {
                Debug.LogWarning("Item or itemPrefab is null");
                return null;
            }

            string key = item.itemID;

            if (!poolDictionary.ContainsKey(key))
            {
                poolDictionary[key] = new Queue<GameObject>();
            }

            if (poolDictionary[key].Count > 0)
            {
                GameObject pooledItem = poolDictionary[key].Dequeue();
                pooledItem.SetActive(true);
                return pooledItem;
            }
            else
            {
                GameObject newItem = GameObject.Instantiate(item.itemPrefab);
                newItem.name = $"{item.itemName} (Pooled)";
                return newItem;
            }
        }

        public void ReturnToPool(GameObject item, string itemID)
        {
            if (item == null) return;

            item.SetActive(false);
            item.transform.SetParent(poolParent);

            if (!poolDictionary.ContainsKey(itemID))
            {
                poolDictionary[itemID] = new Queue<GameObject>();
            }

            poolDictionary[itemID].Enqueue(item);
        }

        public void ClearPool()
        {
            foreach (var queue in poolDictionary.Values)
            {
                while (queue.Count > 0)
                {
                    GameObject item = queue.Dequeue();
                    if (item != null)
                    {
                        GameObject.DestroyImmediate(item);
                    }
                }
            }
            poolDictionary.Clear();
        }
    }

    // 4. WeightedItemSpawner - Spawn ngẫu nhiên theo trọng số
    [System.Serializable]
    public class WeightedItemSpawner
    {
        public static Item GetRandomWeightedItem(List<Item> items)
        {
            if (items == null || items.Count == 0) return null;

            // Tính tổng trọng số (rarity càng cao càng hiếm)
            float totalWeight = 0f;
            foreach (Item item in items)
            {
                // Rarity cao = weight thấp (hiếm hơn)
                float weight = 1f / Mathf.Max(1f, item.itemRarity);
                totalWeight += weight;
            }

            float randomValue = UnityEngine.Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            foreach (Item item in items)
            {
                float weight = 1f / Mathf.Max(1f, item.itemRarity);
                currentWeight += weight;

                if (randomValue <= currentWeight)
                {
                    return item;
                }
            }

            return items[items.Count - 1]; // Fallback
        }
    }

    // 5. ChapterSpawnFilter - Lọc theo Chapter
    [System.Serializable]
    public class ChapterSpawnFilter
    {
        [SerializeField] private Item.ItemChapter currentChapter;
        [SerializeField] private bool allowAllChapters = false;

        public Item.ItemChapter CurrentChapter
        {
            get => currentChapter;
            set => currentChapter = value;
        }

        public bool AllowAllChapters
        {
            get => allowAllChapters;
            set => allowAllChapters = value;
        }

        public bool CanSpawnItem(Item item)
        {
            if (item == null) return false;
            return allowAllChapters || item.itemChapter == currentChapter;
        }

        public List<Item> FilterItems(List<Item> items)
        {
            if (allowAllChapters) return new List<Item>(items);
            return items.Where(item => item.itemChapter == currentChapter).ToList();
        }
    }

    // 6. SpawnEffects - Hiệu ứng khi spawn
    [System.Serializable]
    public class SpawnEffects
    {
        [SerializeField] private GameObject defaultSpawnVFX;
        [SerializeField] private AudioClip defaultSpawnSFX;
        [SerializeField] private Dictionary<int, GameObject> rarityVFX = new Dictionary<int, GameObject>();

        public void PlaySpawnEffect(Vector3 position, Item item)
        {
            // VFX
            GameObject vfxPrefab = defaultSpawnVFX;
            if (rarityVFX.ContainsKey(item.itemRarity))
            {
                vfxPrefab = rarityVFX[item.itemRarity];
            }

            if (vfxPrefab != null)
            {
                GameObject vfx = GameObject.Instantiate(vfxPrefab, position, Quaternion.identity);
                GameObject.Destroy(vfx, 3f); // Auto destroy after 3 seconds
            }

            // SFX
            AudioClip sfx = item.useAudio ?? defaultSpawnSFX;
            if (sfx != null)
            {
                AudioSource.PlayClipAtPoint(sfx, position);
            }
        }
    }

    // 7. SpawnedItemTracker - Theo dõi item đã spawn
    [System.Serializable]
    public class SpawnedItemTracker
    {
        public GameObject gameObject;
        public Item item;
        public float spawnTime;
        public SpawnLocation spawnLocation;

        public SpawnedItemTracker(GameObject go, Item itm, SpawnLocation loc)
        {
            gameObject = go;
            item = itm;
            spawnLocation = loc;
            spawnTime = Time.time;
        }
    }
}
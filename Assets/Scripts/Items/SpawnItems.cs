using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private ItemDatabase itemDatabase; // Tham chiếu đến ItemDatabase

    void Start()
    {
        for (int i = 0; i < 5; i++)
        {
            SpawnItem("flowers", new Vector3(-24 + i, 2+ i, 8+ i));            
        }
    }


    // Hàm spawn item tại vị trí xác định dựa trên tên item
    public GameObject SpawnItem(string itemName, Vector3 position)
    {
        // Lấy item từ ItemDatabase
        Item item = itemDatabase.GetItem(itemName);
        if (item == null)
        {
            Debug.LogWarning($"Item with name {itemName} not found in ItemDatabase!");
            return null;
        }

        // Kiểm tra xem item có prefab không
        if (item.itemPrefab == null)
        {
            Debug.LogWarning($"Item {itemName} does not have a valid prefab assigned!");
            return null;
        }

        // Tạo instance của itemPrefab tại vị trí chỉ định
        GameObject spawnedItem = Instantiate(item.itemPrefab, position, Quaternion.identity);

        // Gắn thông tin item vào GameObject
        ItemInstance itemInstance = spawnedItem.GetComponent<ItemInstance>();
        if (itemInstance == null)
        {
            itemInstance = spawnedItem.AddComponent<ItemInstance>();
        }
        itemInstance.Initialize(item);

        // Cập nhật sprite (nếu cần)
        // SpriteRenderer spriteRenderer = spawnedItem.GetComponent<SpriteRenderer>();
        // if (spriteRenderer != null && item.icon != null)
        // {
        //     spriteRenderer.sprite = item.icon;
        // }

        return spawnedItem;
    }

    // Hàm spawn item ngẫu nhiên dựa trên vùng, loại item và độ hiếm
    public GameObject SpawnRandomItem(MapRegion region, ItemType itemType, ItemRarity rarity, Vector3 position)
    {
        // Lấy danh sách item theo vùng và loại
        Item[] items = itemDatabase.GetItemsByRegion(region, itemType);
        if (items == null || items.Length == 0)
        {
            Debug.LogWarning($"No items found for region {region} and type {itemType}!");
            return null;
        }

        // Lọc item theo độ hiếm
        Item[] filteredItems = System.Array.FindAll(items, item => item.rarity == rarity);
        if (filteredItems.Length == 0)
        {
            Debug.LogWarning($"No items found for region {region}, type {itemType}, and rarity {rarity}!");
            return null;
        }

        // Chọn ngẫu nhiên một item từ danh sách đã lọc
        Item randomItem = filteredItems[Random.Range(0, filteredItems.Length)];
        if (randomItem == null)
        {
            Debug.LogWarning("Selected item is null!");
            return null;
        }

        // Spawn item đã chọn
        return SpawnItem(randomItem.itemName, position);
    }
}

// Component để lưu trữ thông tin item trên GameObject được spawn
public class ItemInstance : MonoBehaviour
{
    private Item item;

    public Item Item => item;

    public void Initialize(Item item)
    {
        this.item = item;
        gameObject.name = $"Item_{item.itemName}";
    }
}   
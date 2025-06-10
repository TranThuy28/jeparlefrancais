using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Item Database", menuName = "Game/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [Header("Mountain Region Items")]
    public Item[] mountainMaterials;
    public CraftableItem[] mountainCraftables;
    public Item[] mountainQuestItems;
    
    [Header("Snow Region Items")]
    public Item[] snowMaterials;
    public CraftableItem[] snowCraftables;
    public Item[] snowQuestItems;
    
    [Header("Tropical Sea Items")]
    public Item[] tropicalMaterials;
    public CraftableItem[] tropicalCraftables;
    public Item[] tropicalQuestItems;
    
    private Dictionary<string, Item> itemLookup;
    
    private void OnEnable()
    {
        BuildItemLookup();
    }

    private void BuildItemLookup()
    {
        itemLookup = new Dictionary<string, Item>();

        AddItemsToLookup(mountainMaterials);
        AddItemsToLookup(mountainCraftables);
        AddItemsToLookup(mountainQuestItems);
        AddItemsToLookup(snowMaterials);
        AddItemsToLookup(snowCraftables);
        AddItemsToLookup(snowQuestItems);
        AddItemsToLookup(tropicalMaterials);
        AddItemsToLookup(tropicalCraftables);
        AddItemsToLookup(tropicalQuestItems);
        // Log tất cả các item trong itemLookup
    }
    
    private void AddItemsToLookup(Item[] items)
    {
        foreach (var item in items)
        {
            if (item != null && !itemLookup.ContainsKey(item.name))
            {
                itemLookup[item.name] = item;
            }
        }
    }
    
    public Item GetItem(string itemName)
    {
        if (itemLookup == null) BuildItemLookup();
        itemLookup.TryGetValue(itemName, out Item item);
        return item;
    }
    
    public Item[] GetItemsByRegion(MapRegion region, ItemType itemType)
    {
        switch (region)
        {
            case MapRegion.Mountain:
                return itemType switch
                {
                    ItemType.Material => mountainMaterials,
                    ItemType.Craftable => System.Array.ConvertAll(mountainCraftables, x => (Item)x),
                    ItemType.Quest => mountainQuestItems,
                    _ => new Item[0]
                };
            case MapRegion.SnowRegion:
                return itemType switch
                {
                    ItemType.Material => snowMaterials,
                    ItemType.Craftable => System.Array.ConvertAll(snowCraftables, x => (Item)x),
                    ItemType.Quest => snowQuestItems,
                    _ => new Item[0]
                };
            case MapRegion.TropicalSea:
                return itemType switch
                {
                    ItemType.Material => tropicalMaterials,
                    ItemType.Craftable => System.Array.ConvertAll(tropicalCraftables, x => (Item)x),
                    ItemType.Quest => tropicalQuestItems,
                    _ => new Item[0]
                };
            default:
                return new Item[0];
        }
    }
}
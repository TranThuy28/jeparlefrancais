using UnityEngine;

[System.Serializable]
public enum ItemType
{
    Material,
    Craftable,
    Quest
}

[System.Serializable]
public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

[System.Serializable]
public enum MapRegion
{
    Mountain = 1,
    SnowRegion = 2,
    TropicalSea = 3
}

[CreateAssetMenu(fileName = "New Item", menuName = "Game/Item")]
public class Item : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public string itemNameEnglish;
    public string description;
    public ItemType itemType;
    public ItemRarity rarity;
    public MapRegion region;
    public GameObject itemPrefab;
    [HideInInspector]
    public Sprite icon;
    [Header("Properties")]
    public int maxStackSize = 99;
    public bool isConsumable = false;
    public int value = 0;

     void Start()
    {
        SpriteRenderer sr = itemPrefab.GetComponent<SpriteRenderer>();
        Sprite itemSprite = sr.sprite;

        icon = itemSprite;
    }
    
    [Header("Stats (for equipment)")]
    public int defense = 0;
    public int coldResistance = 0;
    public int luck = 0;
    public int charm = 0;
    public int hpRestore = 0;
    public int manaRestore = 0;
}

[CreateAssetMenu(fileName = "New Craftable Item", menuName = "Game/Craftable Item")]
public class CraftableItem : Item
{
    [Header("Crafting")]
    public CraftingRecipe recipe;
}

[System.Serializable]
public struct CraftingIngredient
{
    public Item item;
    public int quantity;
}

[CreateAssetMenu(fileName = "New Recipe", menuName = "Game/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    public CraftingIngredient[] ingredients;
    public Item resultItem;
    public int resultQuantity = 1;
    public float craftingTime = 1f;
}
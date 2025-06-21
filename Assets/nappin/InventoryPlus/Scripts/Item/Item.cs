using System;
using UnityEngine;


namespace InventoryPlus
{
    [CreateAssetMenu(fileName = "(Itm)Item", menuName = "InventoryPlus/Item", order = 1)]
    public class Item : ScriptableObject
    {
        public enum ItemChapter
        {
            Mountain,
            Frozen,
            Island
        }
        [SerializeField] public Sprite itemSprite;
        [SerializeField] public GameObject itemPrefab;
        [SerializeField] public ItemChapter itemChapter;
        [SerializeField] public string itemName;
        [SerializeField] public string itemID;
        [SerializeField] public string itemCategory;

        [SerializeField] public bool isStackable = true;
        [SerializeField] public int stackSize = 9999;

        [SerializeField] public bool isDurable = false;
        [SerializeField] public int maxDurability = 100;
        [SerializeField] public int usageCost = 1;
        [SerializeField] public bool hasDamagedSprites = false;
        [SerializeField] public Sprite[] damagedSprites;

        [SerializeField] public string itemAttribute;
        [SerializeField] public string itemDescription;
        [SerializeField] public int itemRarity;

        [SerializeField] public AudioClip useAudio;
        [SerializeField] public AudioClip dropAudio;
        [SerializeField] public AudioClip equipAudio;
    }
}
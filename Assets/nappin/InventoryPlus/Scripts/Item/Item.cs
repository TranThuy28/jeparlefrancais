using System;
using UnityEngine;


namespace InventoryPlus
{
    [CreateAssetMenu(fileName = "(Itm)Item", menuName = "InventoryPlus/Item", order = 1)]
    public class Item : ScriptableObject
    {
        [SerializeField] public Sprite itemSprite;

        [SerializeField] public string itemName;
        [SerializeField] public string itemID;
        [SerializeField] public string itemCategory;

        [SerializeField] public bool isStackable;
        [SerializeField] public int stackSize;

        [SerializeField] public bool isDurable;
        [SerializeField] public int maxDurability;
        [SerializeField] public int usageCost;
        [SerializeField] public bool hasDamagedSprites;
        [SerializeField] public Sprite[] damagedSprites;

        [SerializeField] public string itemAttribute;
        [SerializeField] public string itemDescription;
        [SerializeField] public int itemRarity;

        [SerializeField] public AudioClip useAudio;
        [SerializeField] public AudioClip dropAudio;
        [SerializeField] public AudioClip equipAudio;
    }
}
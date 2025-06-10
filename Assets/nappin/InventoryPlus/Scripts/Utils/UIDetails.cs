using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace InventoryPlus
{
    public class UIDetails : MonoBehaviour
    {
        [Header("UI Elements")]
        public TextMeshProUGUI itemTitle;
        public TextMeshProUGUI itemAttribute;
        public TextMeshProUGUI itemDescription;
        public TextMeshProUGUI itemNum;
        public Image itemImage;

        [Space(15)]
        public GameObject[] rarityTags;

        [Space(15)]
        [Header("References")]
        public Inventory inventory;
        public InputReader inputReader;
        public Animator anim;


        /**/


        #region UpdateContent

        public void UpdateDetails(UISlot _UIInventorySlot, bool _triggerAnimation)
        {
            if (_UIInventorySlot != null)
            {
                ItemSlot tmpSlot = inventory.GetInventorySlot(_UIInventorySlot);
                if (_triggerAnimation) anim.SetTrigger("changeItem");

                SetImage(tmpSlot);
                SetContent(tmpSlot);
                SetRarity(tmpSlot);
            }
        }


        private void SetContent(ItemSlot _inventorySlot)
        {
            if (_inventorySlot != null && _inventorySlot.GetItemType() != null)
            {
                itemTitle.text = _inventorySlot.GetItemType().itemName;
                itemAttribute.text = _inventorySlot.GetItemType().itemAttribute;
                itemDescription.text = _inventorySlot.GetItemType().itemDescription;

                if (_inventorySlot.GetItemType().isDurable) itemNum.text = _inventorySlot.GetItemDurability().ToString() + "/" + _inventorySlot.GetItemType().maxDurability.ToString();
                else itemNum.text = _inventorySlot.GetItemNum().ToString();
            }
            else
            {
                itemTitle.text = "";
                itemAttribute.text = "";
                itemDescription.text = "";
                itemNum.text = "";
            }
        }


        private void SetRarity(ItemSlot _inventorySlot)
        {
            if (_inventorySlot != null && _inventorySlot.GetItemType() != null)
            {
                for (int i = 0; i < rarityTags.Length; i++)
                {
                    if (i == _inventorySlot.GetItemType().itemRarity) rarityTags[i].SetActive(true);
                    else rarityTags[i].SetActive(false);
                }
            }
            else for (int i = 0; i < rarityTags.Length; i++) rarityTags[i].SetActive(false);
        }


        private void SetImage(ItemSlot _inventorySlot)
        {
            if (_inventorySlot != null && _inventorySlot.GetItemType() != null)
            {
                itemImage.gameObject.SetActive(true);
                
                if (!_inventorySlot.GetItemType().hasDamagedSprites || !_inventorySlot.GetItemType().isDurable) itemImage.sprite = _inventorySlot.GetItemType().itemSprite;
                else
                {
                    int spriteIndex = (int)Mathf.Ceil((_inventorySlot.GetItemType().damagedSprites.Length + 1) * _inventorySlot.GetItemDurability() / _inventorySlot.GetItemType().maxDurability);

                    if (spriteIndex > _inventorySlot.GetItemType().damagedSprites.Length) itemImage.sprite = _inventorySlot.GetItemType().itemSprite;
                    else itemImage.sprite = _inventorySlot.GetItemType().damagedSprites[Mathf.Abs(_inventorySlot.GetItemType().damagedSprites.Length - spriteIndex)];
                }
            }
            else itemImage.gameObject.SetActive(false);
        }

        #endregion
    }
}
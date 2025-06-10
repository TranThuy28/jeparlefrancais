using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace InventoryPlus
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class PickUp : MonoBehaviour
    {
        [Header("Inventory Changes")]
        public List<ItemSlot> addToInventory;
        public List<ItemSlot> removeFromInventory;

        [Space(15)]
        [Header("Audio")]
        public bool playAudioOnEvent;
        public AudioSource eventAudio;

        [Space(15)]
        [Header("References")]
        public Sprite bagSprite;
        public SpriteRenderer itemSprite;
        public string playerTag = "Player";
        public Animator anim;

        [Space(15)]
        public bool destroyWhenCalled = true;
        public bool enableDebug = true;


        private Inventory inventory;
        private BoxCollider2D col;

        private bool hasExitedTrigger = true;


        /**/


        #region Setup

        private void Start()
        {
            col = this.GetComponent<BoxCollider2D>();

            if (addToInventory.Count == 1)
            {
                if (!addToInventory[0].GetItemType().hasDamagedSprites || !addToInventory[0].GetItemType().isDurable) itemSprite.sprite = addToInventory[0].GetItemType().itemSprite;
                else
                {
                    int spriteIndex = (int)Mathf.Ceil((addToInventory[0].GetItemType().damagedSprites.Length + 1) * addToInventory[0].GetItemDurability() / addToInventory[0].GetItemType().maxDurability);

                    if (spriteIndex > addToInventory[0].GetItemType().damagedSprites.Length) itemSprite.sprite = addToInventory[0].GetItemType().itemSprite;
                    else itemSprite.sprite = addToInventory[0].GetItemType().damagedSprites[Mathf.Abs(addToInventory[0].GetItemType().damagedSprites.Length - spriteIndex)];
                }
            }
            else if (addToInventory.Count > 1)
            {
                if (IsSameType(addToInventory)) itemSprite.sprite = addToInventory[0].GetItemType().itemSprite;
                else itemSprite.sprite = bagSprite;
            }
        }

        #endregion


        #region Drop

        public void ManageDrop(List<ItemSlot> _itemSlots, bool _triggerExit)
        {
            if (_itemSlots.Count == 1)
            {
                if (!_itemSlots[0].GetItemType().hasDamagedSprites || !_itemSlots[0].GetItemType().isDurable) itemSprite.sprite = _itemSlots[0].GetItemType().itemSprite;
                else
                {
                    int spriteIndex = (int)Mathf.Ceil((_itemSlots[0].GetItemType().damagedSprites.Length + 1) * _itemSlots[0].GetItemDurability() / _itemSlots[0].GetItemType().maxDurability);

                    if (spriteIndex > _itemSlots[0].GetItemType().damagedSprites.Length) itemSprite.sprite = _itemSlots[0].GetItemType().itemSprite;
                    else itemSprite.sprite = _itemSlots[0].GetItemType().damagedSprites[Mathf.Abs(_itemSlots[0].GetItemType().damagedSprites.Length - spriteIndex)];
                }

                addToInventory.Add(new ItemSlot(_itemSlots[0].GetItemType(), _itemSlots[0].GetItemNum(), _itemSlots[0].GetItemDurability()));
            }
            else if (_itemSlots.Count > 1)
            {
                if (IsSameType(_itemSlots)) itemSprite.sprite = _itemSlots[0].GetItemType().itemSprite;
                else itemSprite.sprite = bagSprite;

                for(int i = 0; i < _itemSlots.Count; i++) addToInventory.Add(new ItemSlot(_itemSlots[i].GetItemType(), _itemSlots[i].GetItemNum(), _itemSlots[i].GetItemDurability()));
            }

            hasExitedTrigger = _triggerExit;
        }


        private bool IsSameType(List<ItemSlot> _itemSlots)
        {
            Item refItemType = _itemSlots[0].GetItemType();
            bool sameType = true;

            for (int i = 1; i < _itemSlots.Count; i++)
            {
                if (_itemSlots[i].GetItemType() != refItemType)
                {
                    sameType = false;
                    break;
                }
            }

            return sameType;
        }

        #endregion


        #region Interaction

        public void TriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag(playerTag) && hasExitedTrigger)
            {
                inventory = collision.GetComponent<PlayerController>().inventory;
                UpdateInventory();
            }
        }


        public void TriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag(playerTag))
            {
                hasExitedTrigger = true;
            }
        }


        public void TriggerEnter3D(Collider collision)
        {
            if (collision.CompareTag(playerTag) && hasExitedTrigger)
            {
                inventory = collision.GetComponent<PlayerController>().inventory;
                UpdateInventory();
            }
        }


        public void TriggerExit3D(Collider collision)
        {
            if (collision.CompareTag(playerTag))
            {
                hasExitedTrigger = true;
            }
        }

        #endregion


        #region InventoryUpdate

        public void UpdateInventory()
        {
            if (inventory != null)
            {
                RemoveInventory();
                AddInventory();

                if (destroyWhenCalled)
                {
                    this.enabled = false;
                    anim.SetTrigger("Despawn");
                }

                if (playAudioOnEvent) eventAudio.Play();
            }

            else Debug.LogError("Can't update Inventory because player has no Inventory reference");
        }


        private void AddInventory()
        {
            if(addToInventory.Count > 0)
            {
                for (int i = 0; i < addToInventory.Count; i++)
                {
                    inventory.AddInventory(addToInventory[i].GetItemType(), addToInventory[i].GetItemNum(), addToInventory[i].GetItemDurability(), false);
                }

                inventory.DropExtra();
            }
        }


        private void RemoveInventory()
        {
            if (removeFromInventory.Count > 0)
            {
                for (int i = 0; i < removeFromInventory.Count; i++)
                {
                    inventory.RemoveInventory(removeFromInventory[i].GetItemType(), removeFromInventory[i].GetItemNum());
                }
            }
        }

        #endregion


        #region Utils

        private void OnDrawGizmos()
        {
            if (enableDebug)
            {
                if (col == null) col = this.GetComponent<BoxCollider2D>();

                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(transform.position + (Vector3)col.offset, col.size);
            }
        }

        #endregion
    }
}
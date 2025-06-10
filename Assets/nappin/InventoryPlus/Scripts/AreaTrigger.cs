using UnityEngine;
using UnityEngine.Events;


namespace InventoryPlus
{
    
    public class AreaTrigger : MonoBehaviour
    {
        [Header("Events")]
        public UnityEvent inventoryUpdates;

        [Space(15)]
        [Header("Audio")]
        public bool playAudioOnEvent;
        public AudioSource eventAudio;

        [Space(15)]
        [Header("References")]
        public string playerTag = "Player";

        [Space(15)]
        public bool enableDebug = true;


        private Inventory inventory;
        private BoxCollider2D col;


        /**/


        #region Setup

        private void Start()
        {
            col = this.GetComponent<BoxCollider2D>();
        }

        #endregion


        #region Trigger

        public void TriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag(playerTag))
            {
                inventory = collision.GetComponent<PlayerController>().inventory;
                UpdateInventory();
            }
        }


        public void TriggerEnter3D(Collider collision)
        {
            if (collision.CompareTag(playerTag))
            {
                inventory = collision.GetComponent<PlayerController>().inventory;
                UpdateInventory();
            }
        }


        public void UpdateInventory()
        {
            if (inventory != null)
            {
                inventoryUpdates.Invoke();
                if (playAudioOnEvent) eventAudio.Play();
            }

            else Debug.LogError("Can't update Inventory because player has no Inventory reference");
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


        public void _RemoveCategory(string _category) { inventory.RemoveCategory(_category); }
        public void _RemoveID(string _ID) { inventory.RemoveID(_ID); }
        public void _ClearInventory() { inventory.ClearInventory(); }
        public void _Sort() { inventory.Sort(); }

        #endregion
    }
}
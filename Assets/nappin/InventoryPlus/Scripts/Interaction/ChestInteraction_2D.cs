using UnityEngine;


namespace InventoryPlus
{
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(Chest))]
    public class ChestInteraction_2D : MonoBehaviour
    {
        private Chest chest;


        /**/


        void Start()
        {
            chest = this.GetComponent<Chest>();
        }


        private void OnTriggerEnter2D(Collider2D collision)
        {
            chest.TriggerEnter2D(collision);
        }


        private void OnTriggerExit2D(Collider2D collision)
        {
            chest.TriggerExit2D(collision);
        }
    }
}
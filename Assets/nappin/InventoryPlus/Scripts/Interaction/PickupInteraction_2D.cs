using UnityEngine;


namespace InventoryPlus
{
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(PickUp))]
    public class PickupInteraction_2D : MonoBehaviour
    {
        private PickUp pickUp;


        /**/


        void Start()
        {
            pickUp = this.GetComponent<PickUp>();
        }


        private void OnTriggerEnter2D(Collider2D collision)
        {
            pickUp.TriggerEnter2D(collision);
        }


        private void OnTriggerExit2D(Collider2D collision)
        {
            pickUp.TriggerExit2D(collision);
        }
    }
}
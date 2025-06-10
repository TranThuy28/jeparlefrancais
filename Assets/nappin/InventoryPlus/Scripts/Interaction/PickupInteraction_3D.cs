using UnityEngine;


namespace InventoryPlus
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(PickUp))]
    public class PickupInteraction_3D : MonoBehaviour
    {
        private PickUp pickUp;


        /**/


        void Start()
        {
            pickUp = this.GetComponent<PickUp>();
        }


        private void OnTriggerEnter(Collider collision)
        {
            pickUp.TriggerEnter3D(collision);
        }


        private void OnTriggerExit(Collider collision)
        {
            pickUp.TriggerExit3D(collision);
        }
    }
}
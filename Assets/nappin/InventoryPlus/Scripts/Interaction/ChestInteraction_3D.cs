using UnityEngine;


namespace InventoryPlus
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(Chest))]
    public class ChestInteraction_3D : MonoBehaviour
    {
        private Chest chest;


        /**/


        void Start()
        {
            chest = this.GetComponent<Chest>();
        }


        private void OnTriggerEnter(Collider collision)
        {
            chest.TriggerEnter3D(collision);
        }


        private void OnTriggerExit(Collider collision)
        {
            chest.TriggerExit3D(collision);
        }
    }
}
using UnityEngine;


namespace InventoryPlus
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(AreaTrigger))]
    public class AreaTriggerInteraction_3D : MonoBehaviour
    {
        private AreaTrigger areaTrigger;


        /**/


        void Start()
        {
            areaTrigger = this.GetComponent<AreaTrigger>();
        }


        private void OnTriggerEnter(Collider collision)
        {
            areaTrigger.TriggerEnter3D(collision);
        }
    }
}
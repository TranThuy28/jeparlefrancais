using UnityEngine;


namespace InventoryPlus
{
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(AreaTrigger))]
    public class AreaTriggerInteraction_2D : MonoBehaviour
    {
        private AreaTrigger areaTrigger;


        /**/


        void Start()
        {
            areaTrigger = this.GetComponent<AreaTrigger>();
        }


        private void OnTriggerEnter2D(Collider2D collision)
        {
            areaTrigger.TriggerEnter2D(collision);
        }
    }
}
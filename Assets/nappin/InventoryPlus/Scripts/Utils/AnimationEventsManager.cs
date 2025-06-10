using UnityEngine;


namespace InventoryPlus
{
    public class AnimationEventsManager : MonoBehaviour
    {
        [Header("References")]
        public GameObject parentObj;


        /**/

        
        public void DestroyParent() { GameObject.Destroy(parentObj); }
    }
}
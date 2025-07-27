using UnityEngine;


namespace InventoryPlus
{
    public class DontDestroy : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroy[] playerParents = FindObjectsByType<DontDestroy>(FindObjectsSortMode.None);
            
            //destroy duplicates if they exist, keep this
            if (playerParents.Length != 1) GameObject.Destroy(playerParents[1].gameObject);
            else GameObject.DontDestroyOnLoad(this);
        }
    }
}
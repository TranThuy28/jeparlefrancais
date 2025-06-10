using UnityEngine.SceneManagement;
using UnityEngine;


namespace InventoryPlus
{
    public class ChangeScene : MonoBehaviour
    {
        [Header("Refernces")]
        [Min(0)] public int sceneIndex;
        public string playerTag = "Player";
        
        
        private SaveSystem saveSystem;


        /**/


        private void Awake()
        {
            saveSystem = this.GetComponent<SaveSystem>();    
        }


        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag(playerTag))
            {
                if(saveSystem != null) saveSystem.SaveData();
                SceneManager.LoadScene(sceneIndex);
            }
        }
    }
}
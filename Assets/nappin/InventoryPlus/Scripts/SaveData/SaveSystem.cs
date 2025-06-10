using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace InventoryPlus
{
    [Serializable] public class SaveData { public ChestDataList chestDataList = new ChestDataList(); public PickUpDataList pickUpDataList = new PickUpDataList(); }
    [Serializable] public class ChestDataList { public List<ChestData> chestData = new List<ChestData>(); }
    [Serializable] public class PickUpDataList { public List<PickUpData> pickUpData = new List<PickUpData>(); }


    public class SaveSystem : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject chestPrefab;
        public GameObject pickUpPrefab;

        [Space(15)]
        public bool ignoreSaves = false;


        private SaveData saveData = new SaveData();
        private InventoryData inventoryData;

        private string pathInteractable;
        private string pathInventory;


        /**/


        #region Setup

        private void Awake()
        {
            pathInteractable = Application.persistentDataPath + "/" + SceneManager.GetActiveScene().name + "_Interactable.json";
            pathInventory = Application.persistentDataPath + "/" + "_Inventory.json";

            Load();
        }

        #endregion


        #region Save

        public void SaveData()
        {
            if (!ignoreSaves)
            {
                //chests
                Chest[] chests = GameObject.FindObjectsOfType<Chest>();
                for (int i = 0; i < chests.Length; i++)
                {
                    List<ItemSlot> chestList = chests[i].GetSlots();
                    chestList.RemoveAll(item => item == null);

                    ChestData chestData = new ChestData(chests[i].name, chests[i].transform.position.x, chests[i].transform.position.y, chestList);
                    saveData.chestDataList.chestData.Add(chestData);
                }

                //pickups
                PickUp[] pickUps = GameObject.FindObjectsOfType<PickUp>();
                for (int i = 0; i < pickUps.Length; i++)
                {
                    PickUpData inventoryTriggerData = new PickUpData(pickUps[i].name, pickUps[i].transform.position.x, pickUps[i].transform.position.y, pickUps[i].addToInventory, pickUps[i].removeFromInventory);
                    saveData.pickUpDataList.pickUpData.Add(inventoryTriggerData);
                }

                //inventory
                Inventory inventory = GameObject.FindObjectOfType<Inventory>();
                List<ItemSlot> inventoryList = inventory.GetSlots();
                //inventoryList.RemoveAll(item => item == null);

                inventoryData = new InventoryData(inventoryList);

                Save();
            }
        }


        public void Save()
        {
            //write interactable
            StreamWriter writerInteractable = new StreamWriter(pathInteractable);
            string jsonDataInteractable = JsonUtility.ToJson(saveData, true);
            writerInteractable.Write(jsonDataInteractable);
            writerInteractable.Close();

            //write inventory
            StreamWriter writerInventory = new StreamWriter(pathInventory);
            string jsonDataInventory = JsonUtility.ToJson(inventoryData, true);
            writerInventory.Write(jsonDataInventory);
            writerInventory.Close();
        }

        #endregion


        #region Load

        public void Load()
        {
            if(!ignoreSaves)
            {
                //chests and pickups
                if (File.Exists(pathInteractable))
                {
                    string jsonDataInteractable = File.ReadAllText(pathInteractable);
                    SaveData saveDataInteractable = JsonUtility.FromJson<SaveData>(jsonDataInteractable);

                    foreach (ChestData chestData in saveDataInteractable.chestDataList.chestData)
                    {
                        GameObject chestObject = GameObject.Find(chestData.instanceName);
                        if (chestObject != null) GameObject.Destroy(chestObject);

                        GameObject loadInstance = GameObject.Instantiate(chestPrefab, new Vector2(chestData.positionX, chestData.positionY), Quaternion.identity);
                        loadInstance.name = chestData.instanceName;

                        Chest chest = loadInstance.GetComponent<Chest>();
                        chest.chestItems = chestData.items;
                    }

                    PickUp[] pickUps = GameObject.FindObjectsOfType<PickUp>();
                    for (int i = 0; i < pickUps.Length; i++) GameObject.Destroy(pickUps[i].gameObject);

                    foreach (PickUpData pickUpData in saveDataInteractable.pickUpDataList.pickUpData)
                    {
                        GameObject loadInstance = GameObject.Instantiate(pickUpPrefab, new Vector2(pickUpData.positionX, pickUpData.positionY), Quaternion.identity);
                        loadInstance.name = pickUpData.instanceName;

                        PickUp pickUp = loadInstance.GetComponent<PickUp>();
                        pickUp.addToInventory = pickUpData.itemsAdd;
                        pickUp.removeFromInventory = pickUpData.itemsRemove;
                    }
                }

                //inventory
                if (File.Exists(pathInventory))
                {
                    string jsonDataInventory = File.ReadAllText(pathInventory);
                    inventoryData = JsonUtility.FromJson<InventoryData>(jsonDataInventory);

                    Inventory inventory = GameObject.FindObjectOfType<Inventory>();
                    inventory.LoadInventory(inventoryData.items);
                }
            }
        }

        #endregion
    }
}
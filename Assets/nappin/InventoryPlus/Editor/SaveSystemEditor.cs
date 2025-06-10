using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;


namespace InventoryPlus
{
    public class SaveSystemEditor : EditorWindow
    {
        [MenuItem("InventoryPlus/Rename Chest and PickUps")]
        static void RenameAll()
        {
            RenameChests();
            RenameInventoryTriggers();
        }


        static void RenameChests()
        {
            Chest[] chests = Object.FindObjectsOfType<Chest>();
            HashSet<int> usedIds = new HashSet<int>();

            foreach (Chest chest in chests)
            {
                int id = GetUniqueId(usedIds);
                chest.gameObject.name = "(Prb)Chest_" + id;

                usedIds.Add(id);
            }
        }


        static void RenameInventoryTriggers()
        {
            PickUp[] pickUps = Object.FindObjectsOfType<PickUp>();
            HashSet<int> usedIds = new HashSet<int>();

            foreach (PickUp pickUp in pickUps)
            {
                int id = GetUniqueId(usedIds);
                pickUp.gameObject.name = "(Prb)PickUp_" + id;

                usedIds.Add(id);
            }
        }


        [MenuItem("InventoryPlus/Delete Save Files")]
        static void DestroySaves()
        {
            string folderPath = Application.persistentDataPath;

            if (Directory.Exists(folderPath))
            {
                string[] files = Directory.GetFiles(folderPath);
                foreach (string file in files) File.Delete(file);
            }
        }


        static int GetUniqueId(HashSet<int> usedIds)
        {
            int id = Random.Range(0, int.MaxValue);
            while (usedIds.Contains(id)) id = Random.Range(0, int.MaxValue);
            
            return id;
        }
    }
}
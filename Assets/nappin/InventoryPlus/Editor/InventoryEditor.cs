using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace InventoryPlus
{
    public static class BackgroundStyle
    {
        private static GUIStyle style = new GUIStyle();
        private static Texture2D texture = new Texture2D(1, 1);


        public static GUIStyle Get(Color color)
        {
            if (texture == null) texture = new Texture2D(1, 1);
            if (style == null) style = new GUIStyle();

            texture.SetPixel(0, 0, color);
            texture.Apply();

            style.normal.background = texture;
            return style;
        }
    }


    [CustomEditor(typeof(Inventory))]
    public class InventoryEditor : Editor
    {
        #region EditorSpecificVars

        private GUIStyle alignGUILeft;
        private GUIStyle alignGUIRight;

        #endregion

        #region SerializedVars

        public SerializedProperty _inventoryItems;
        public SerializedProperty _inventoryUISlots;
        public SerializedProperty _hasHotbar;
        public SerializedProperty _hotbarUISlots;

        public SerializedProperty _displayNotificationOnNewItems;
        public SerializedProperty _showDurabilityValues;
        public SerializedProperty _fillReservedFirst;

        public SerializedProperty _instanciatePickuppableOnDrop;
        public SerializedProperty _pickupPrefab;
        public SerializedProperty _player;

        public SerializedProperty _anim;

        public SerializedProperty _itemsAudio;
        public SerializedProperty _sortAudio;
        public SerializedProperty _swapAudio;

        public SerializedProperty _enableDebug;
        public SerializedProperty _enableMouseDrag;

        #endregion


        private float cellSize = 90f;
        private float cellOutOffset = 10f;
        private float cellInOffset = 8f;


        private Inventory script;
        private bool showInventory = true;


        /**/


        private void OnEnable()
        {
            _inventoryItems = serializedObject.FindProperty("inventoryItems");

            _inventoryUISlots = serializedObject.FindProperty("inventoryUISlots");
            _hasHotbar = serializedObject.FindProperty("hasHotbar");
            _hotbarUISlots = serializedObject.FindProperty("hotbarUISlots");

            _displayNotificationOnNewItems = serializedObject.FindProperty("displayNotificationOnNewItems");
            _showDurabilityValues = serializedObject.FindProperty("showDurabilityValues");
            _fillReservedFirst = serializedObject.FindProperty("fillReservedFirst");

            _instanciatePickuppableOnDrop = serializedObject.FindProperty("instanciatePickuppableOnDrop");
            _pickupPrefab = serializedObject.FindProperty("pickupPrefab");
            _player = serializedObject.FindProperty("player");

            _anim = serializedObject.FindProperty("anim");

            _itemsAudio = serializedObject.FindProperty("itemsAudio");
            _sortAudio = serializedObject.FindProperty("sortAudio");
            _swapAudio = serializedObject.FindProperty("swapAudio");

            _enableDebug = serializedObject.FindProperty("enableDebug");
            _enableMouseDrag = serializedObject.FindProperty("enableMouseDrag");

            script = (Inventory)target;
        }


        public override void OnInspectorGUI()
        {
            //styles
            alignGUILeft = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontSize = 11 };
            alignGUIRight = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight, fontSize = 11 };


            serializedObject.Update();
            EditorGUILayout.BeginVertical();


            //inventory content
            EditorGUILayout.LabelField("Starting Content", EditorStyles.boldLabel);

            #region StartingInventory

            EditorGUILayout.PropertyField(_inventoryItems, true);

            #endregion

            #region UIInventory

            EditorGUILayout.PropertyField(_inventoryUISlots, true);

            #endregion

            #region Hotbar

            _hasHotbar.boolValue = EditorGUILayout.Toggle("Has hotbar", script.hasHotbar);

            if (_hasHotbar.boolValue)
            {
                EditorGUILayout.PropertyField(_hotbarUISlots, true);
            }

            #endregion


            //inventory behaviour
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("UI", EditorStyles.boldLabel);

            #region FillReservedFirst

            _fillReservedFirst.boolValue = EditorGUILayout.Toggle("Fill Reserved First", script.fillReservedFirst);

            #endregion

            #region DisplayNotification

            _displayNotificationOnNewItems.boolValue = EditorGUILayout.Toggle("Display New Notification", script.displayNotificationOnNewItems);

            #endregion

            #region ShowDurabilityValues

            _showDurabilityValues.boolValue = EditorGUILayout.Toggle("Show Durability Values", script.showDurabilityValues);

            #endregion

            
            //pickup
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("Pickup Behaviour", EditorStyles.boldLabel);

            #region InstanciatePickuppable

            _instanciatePickuppableOnDrop.boolValue = EditorGUILayout.Toggle("Instanciate Pickuppable On Drop", script.instanciatePickuppableOnDrop);

            if (_instanciatePickuppableOnDrop.boolValue)
            {
                EditorGUILayout.PropertyField(_pickupPrefab, true);

                EditorGUILayout.PropertyField(_player, true);
            }

            #endregion


            //audio
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("Audio", EditorStyles.boldLabel);

            #region ItemAudio

            EditorGUILayout.PropertyField(_itemsAudio, true);

            #endregion

            #region AudioOnSort

            EditorGUILayout.PropertyField(_sortAudio, true);

            #endregion

            #region AudioOnSwap

            EditorGUILayout.PropertyField(_swapAudio, true);

            #endregion


            //audio
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("References", EditorStyles.boldLabel);

            #region AnimReference

            EditorGUILayout.PropertyField(_anim, true);

            #endregion            


            //debug
            EditorGUILayout.Space(15);

            #region Debug

            _enableDebug.boolValue = EditorGUILayout.Toggle("Enable debug", script.enableDebug);

            #endregion

            #region MouseDrag

            _enableMouseDrag.boolValue = EditorGUILayout.Toggle("Enable mouse drag", script.enableMouseDrag);

            #endregion

            //inventory
            EditorGUILayout.Space(15);

            if (showInventory)
            {
                if (GUILayout.Button("Hide Inventory", GUILayout.Height(26))) showInventory = !showInventory;

                EditorGUILayout.Space(5);
                DisplayGrid();

            }
            else if (GUILayout.Button("Show Inventory", GUILayout.Height(26))) showInventory = !showInventory;


            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }


        
        private void DisplayGrid()
        {
            float width = Screen.width - cellOutOffset * 2;
            List<ItemSlot> tmpInv = script.GetInventoryContent();
            List<int> positions = tmpInv.Select((item, index) => new { Item = item, Index = index }).Where(x => x.Item != null).Select(x => x.Index).ToList();


            if (width > (cellSize + cellOutOffset) && positions.Count > 0)
            {
                float gridX = Mathf.Floor(width / (cellSize + cellOutOffset));
                float gridY = Mathf.Ceil(positions.Count / gridX);
                int currentSlot = 0;

                GUILayout.BeginVertical();

                for (int y = 0; y < (int)gridY; y++)
                {
                    GUILayout.BeginHorizontal();

                    for (int x = 0; x < (int)gridX; x++)
                    {
                        if (tmpInv[positions[currentSlot]] != null) FillCell(tmpInv[positions[currentSlot]]);

                        if (currentSlot < positions.Count - 1) currentSlot++;
                        else break;
                    }

                    GUILayout.EndHorizontal();

                    GUILayout.Space(cellOutOffset);
                }

                GUILayout.EndVertical();
            }
        }


        private void FillCell(ItemSlot _inventorySlot)
        {
            //background
            GUILayout.BeginHorizontal(BackgroundStyle.Get(new Color(0f, 0f, 0f, 0.2f)), GUILayout.Width(cellSize), GUILayout.Height(cellSize));
            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();


            //image
            GUILayout.Label(_inventorySlot.GetItemType().itemSprite.texture, GUILayout.Width(cellSize - cellInOffset), GUILayout.Height(cellSize - cellInOffset));

            //text content
            GUILayout.BeginHorizontal();

            float labelWidth = (cellSize - cellInOffset * 2) / 2f - 0.5f;

            GUILayout.Label(_inventorySlot.GetItemNum().ToString(), alignGUILeft, GUILayout.Width(labelWidth));

            if (_inventorySlot.GetItemType().isDurable) GUILayout.Label((100f * _inventorySlot.GetItemDurability() / _inventorySlot.GetItemType().maxDurability).ToString() + "%", alignGUIRight, GUILayout.Width(labelWidth));
            else GUILayout.Label("--.--", alignGUIRight, GUILayout.Width(labelWidth));

            GUILayout.EndHorizontal();


            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(cellOutOffset);
        }
    }
}
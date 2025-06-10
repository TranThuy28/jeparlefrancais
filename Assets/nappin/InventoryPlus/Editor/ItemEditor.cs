using UnityEditor;
using UnityEngine;


namespace InventoryPlus
{
    [CustomEditor(typeof(Item))]
    public class ItemEditor : Editor
    {
        #region EditorSpecificVars

        private GUIStyle alignGUILeft;
        private GUIStyle alignGUIRight;

        #endregion


        #region SerializedVars

        //fixed
        public SerializedProperty _itemSprite;
        public SerializedProperty _itemName;
        public SerializedProperty _itemID;
        public SerializedProperty _itemCategory;

        //stackable
        public SerializedProperty _isStackable;
        public SerializedProperty _stackSize;

        //if not stackable
        public SerializedProperty _isDurable;
        public SerializedProperty _maxDurability;
        public SerializedProperty _usageCost;
        public SerializedProperty _hasDamagedSprites;
        public SerializedProperty _damagedSprites;

        //details
        public SerializedProperty _itemAttribute;
        public SerializedProperty _itemDescription;
        public SerializedProperty _itemRarity;

        //audio
        public SerializedProperty _useAudio;
        public SerializedProperty _dropAudio;
        public SerializedProperty _equipAudio;

        #endregion


        private Item script;
        private bool showDetails = false;


        /**/


        private void OnEnable()
        {
            //fixed
            _itemSprite = serializedObject.FindProperty("itemSprite");
            _itemName = serializedObject.FindProperty("itemName");
            _itemID = serializedObject.FindProperty("itemID");
            _itemCategory = serializedObject.FindProperty("itemCategory");

            //stackable
            _isStackable = serializedObject.FindProperty("isStackable");
            _stackSize = serializedObject.FindProperty("stackSize");

            //if not stackable
            _isDurable = serializedObject.FindProperty("isDurable");
            _maxDurability = serializedObject.FindProperty("maxDurability");
            _usageCost = serializedObject.FindProperty("usageCost");
            _hasDamagedSprites = serializedObject.FindProperty("hasDamagedSprites");
            _damagedSprites = serializedObject.FindProperty("damagedSprites");

            //details
            _itemAttribute = serializedObject.FindProperty("itemAttribute");
            _itemDescription = serializedObject.FindProperty("itemDescription");
            _itemRarity = serializedObject.FindProperty("itemRarity");

            //audio
            _useAudio = serializedObject.FindProperty("useAudio");
            _dropAudio = serializedObject.FindProperty("dropAudio");
            _equipAudio = serializedObject.FindProperty("equipAudio");


            script = (Item)target;
        }


        public override void OnInspectorGUI()
        {
            //styles
            EditorStyles.textField.wordWrap = true;
            alignGUILeft = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft };
            alignGUIRight = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight };


            serializedObject.Update();


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(15);


            //fixed
            EditorGUILayout.BeginVertical();

            #region ItemSprite

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            _itemSprite.objectReferenceValue = (Sprite)EditorGUILayout.ObjectField(script.itemSprite, typeof(Sprite), allowSceneObjects: true, GUILayout.Height(200f), GUILayout.Width(200f));

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            #endregion

            EditorGUILayout.Space(20);

            #region ItemName

            EditorGUILayout.LabelField("Name", EditorStyles.boldLabel);
            _itemName.stringValue = EditorGUILayout.TextField(script.itemName);
            if (string.IsNullOrEmpty(_itemName.stringValue)) _itemName.stringValue = script.itemName = "item name";

            #endregion

            EditorGUILayout.Space(5);

            #region ItemID

            EditorGUILayout.LabelField("ID", EditorStyles.boldLabel);
            _itemID.stringValue = EditorGUILayout.TextField(script.itemID);
            if (string.IsNullOrEmpty(_itemID.stringValue)) _itemID.stringValue = script.itemID = "item ID";

            #endregion

            EditorGUILayout.Space(5);

            #region ItemCategory

            EditorGUILayout.LabelField("Category", EditorStyles.boldLabel);
            _itemCategory.stringValue = EditorGUILayout.TextField(script.itemCategory);
            if (string.IsNullOrEmpty(_itemCategory.stringValue)) _itemCategory.stringValue = script.itemCategory = "item category";

            #endregion

            EditorGUILayout.Space(5);

            if (showDetails)
            {
                if (GUILayout.Button("Hide Details", GUILayout.Height(26))) showDetails = !showDetails;

                //description
                EditorGUILayout.Space(5);

                #region ItemAttribute

                EditorGUILayout.LabelField("Item Attribute", EditorStyles.boldLabel);
                _itemAttribute.stringValue = EditorGUILayout.TextArea(script.itemAttribute);
                if (string.IsNullOrEmpty(_itemAttribute.stringValue)) _itemAttribute.stringValue = script.itemAttribute = "item attribute";

                #endregion

                EditorGUILayout.Space(5);

                #region ItemDescription

                EditorGUILayout.LabelField("Item Description", EditorStyles.boldLabel);
                _itemDescription.stringValue = EditorGUILayout.TextArea(script.itemDescription);
                if (string.IsNullOrEmpty(_itemDescription.stringValue)) _itemDescription.stringValue = script.itemDescription = "item description";

                #endregion

                EditorGUILayout.Space(5);

                #region ItemRarity

                EditorGUILayout.LabelField("Item Rarity", EditorStyles.boldLabel);
                _itemRarity.intValue = EditorGUILayout.IntSlider(script.itemRarity, 0, 2);

                #endregion
            }
            else if (GUILayout.Button("Show Details", GUILayout.Height(26))) showDetails = !showDetails;

            EditorGUILayout.EndVertical();


            //separator
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space(20);


            //stackable
            EditorGUILayout.LabelField("Item Properties", EditorStyles.boldLabel);
            
            EditorGUILayout.Space(5);

            #region IsStackable

            _isStackable.boolValue = EditorGUILayout.Toggle("Stackable", script.isStackable);

            #endregion

            if (_isStackable.boolValue)
            {
                EditorGUILayout.Space(1);

                #region StackSize

                _stackSize.intValue = EditorGUILayout.IntField("Stack Size", script.stackSize);
                if (_stackSize.intValue <= 0) _stackSize.intValue = script.stackSize = 1;

                #endregion
            }
            else
            {
                //if not stackable
                EditorGUILayout.Space(1);

                #region IsDurable

                _isDurable.boolValue = EditorGUILayout.Toggle("Has Durability", script.isDurable);

                #endregion

                if (_isDurable.boolValue)
                {
                    EditorGUILayout.Space(1);

                    #region MaximumDurability

                    _maxDurability.intValue = EditorGUILayout.IntField("Maximum Durability", script.maxDurability);
                    if (_maxDurability.intValue <= 0) _maxDurability.intValue = script.maxDurability = 1;

                    #endregion

                    EditorGUILayout.Space(1);

                    #region UsageCost

                    _usageCost.intValue = EditorGUILayout.IntField("Usage cost", script.usageCost);
                    if (_usageCost.intValue > _maxDurability.intValue) _usageCost.intValue = script.usageCost = _maxDurability.intValue;

                    #endregion

                    EditorGUILayout.Space(1);

                    #region HasDamagedSprites

                    _hasDamagedSprites.boolValue = EditorGUILayout.Toggle("Has Damaged Sprites", script.hasDamagedSprites);

                    #endregion

                    if (_hasDamagedSprites.boolValue)
                    {
                        EditorGUILayout.Space(1);

                        #region DamagedSprites

                        EditorGUILayout.PropertyField(_damagedSprites, true);

                        #endregion
                    }
                }
            }


            //separator
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space(20);


            //stackable
            EditorGUILayout.LabelField("Item Audio Properties", EditorStyles.boldLabel);

            EditorGUILayout.Space(5);

            #region UseAudio

            EditorGUILayout.PropertyField(_useAudio, true);

            #endregion

            #region DropAudio

            EditorGUILayout.PropertyField(_dropAudio, true);

            #endregion

            #region EquipAudio

            EditorGUILayout.PropertyField(_equipAudio, true);

            #endregion


            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(15);
            EditorGUILayout.EndHorizontal();


            serializedObject.ApplyModifiedProperties();
        }
    }
}
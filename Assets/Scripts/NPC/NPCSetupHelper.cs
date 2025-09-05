#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class NPCDialogueData
{
    public string npcName;
    [TextArea(2, 5)]
    public string[] dialogueLines;

    [Header("Animation Settings")]
    [Tooltip("Có sử dụng animation system không")]
    public bool useAnimations = true;
    public string idleAnimation = "HumanoidIdle";
    public string talkingAnimation = "Talking";
    public string greetingAnimation = "greeting";
    [Tooltip("Animation đặc biệt cho từng dòng thoại")]
    public DialogueAnimationSetup[] customAnimations;
    public float interactionRange = 4f;
    public float uiHeightOffset = 2.5f;
}
[System.Serializable]
public class DialogueAnimationSetup
{
    [Tooltip("Dòng thoại thứ mấy (bắt đầu từ 0)")]
    public int dialogueLineIndex;
    
    [Tooltip("Tên animation")]
    public string animationName;
    
    [Tooltip("Delay trước khi play")]
    public float delay = 0f;
    
    [Tooltip("Thời gian kéo dài (-1 = vô hạn)")]
    public float duration = -1f;
    
    [Tooltip("Quay về idle sau khi xong")]
    public bool returnToIdle = true;
    
    [Tooltip("Sound effect")]
    public AudioClip soundEffect;
}

public class NPCSetupHelper : MonoBehaviour
{
    [Header("Prefab References")]
    public GameObject interactUIPrefab;
    public DialogueManager dialogueManager;

    [Header("NPC Data")]
    public NPCDialogueData[] npcDataList;

    [Header("Auto Setup")]
    public bool autoSetupOnStart = true;
    public bool createInteractUIIfMissing = true;
    public bool setupAnimationController = true;
    public bool addMissingComponents = true;
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupAllNPCs();
        }
    }

    [ContextMenu("Setup All NPCs")]
    public void SetupAllNPCs()
    {
        DialogueTrigger[] allTriggers = FindObjectsOfType<DialogueTrigger>();

        foreach (DialogueTrigger trigger in allTriggers)
        {
            SetupSingleNPC(trigger);
        }

        Debug.Log("Đã setup " + allTriggers.Length + " NPC(s)");
    }

    void SetupSingleNPC(DialogueTrigger trigger)
    {
        NPCDialogueData npcData = FindNPCData(trigger.gameObject.name);

        if (npcData == null)
        {
            Debug.LogWarning("Không tìm thấy dialogue data cho: " + trigger.gameObject.name);
            return;
        }

        if (trigger.dialogueManager == null)
        {
            trigger.dialogueManager = dialogueManager;
        }

        trigger.npcName = npcData.npcName;
        trigger.dialogueLines = npcData.dialogueLines;
        trigger.interactionRange = npcData.interactionRange;
        trigger.uiHeightOffset = npcData.uiHeightOffset;
        trigger.useAnimations = npcData.useAnimations;
        // 3. Add missing components
        if (addMissingComponents)
        {
            AddMissingComponents(trigger.gameObject);
        }

        // 4. Setup animation controller
        if (setupAnimationController && npcData.useAnimations)
        {
            SetupAnimationController(trigger.gameObject, npcData);
        }
        CreateInteractUI(trigger);

        Debug.Log("Đã setup NPC: " + npcData.npcName);
    }

    void AddMissingComponents(GameObject npcObject)
    {
        bool addedComponents = false;

        // 1. Add Collider nếu chưa có
        Collider col = npcObject.GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sphereCol = npcObject.AddComponent<SphereCollider>();
            sphereCol.isTrigger = true;
            sphereCol.radius = 2f;
            Debug.Log($"➕ Đã thêm SphereCollider cho {npcObject.name}");
            addedComponents = true;
        }
        else if (!col.isTrigger)
        {
            col.isTrigger = true;
            Debug.Log($"🔧 Đã set Collider.isTrigger = true cho {npcObject.name}");
        }

        // 2. Add Animator nếu chưa có
        Animator animator = npcObject.GetComponent<Animator>();
        if (animator == null)
        {
            animator = npcObject.AddComponent<Animator>();
            Debug.Log($"➕ Đã thêm Animator cho {npcObject.name}");
            addedComponents = true;
        }

        // 3. Add NPCAnimationController nếu chưa có
        NPCAnimationController animController = npcObject.GetComponent<NPCAnimationController>();
        if (animController == null)
        {
            animController = npcObject.AddComponent<NPCAnimationController>();
            animController.animator = animator;
            Debug.Log($"➕ Đã thêm NPCAnimationController cho {npcObject.name}");
            addedComponents = true;
        }
        
        // 4. Add AudioSource cho animation sounds
        AudioSource audioSource = npcObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = npcObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
            animController.audioSource = audioSource;
            Debug.Log($"➕ Đã thêm AudioSource cho {npcObject.name}");
            addedComponents = true;
        }

        if (!addedComponents)
        {
            Debug.Log($"✅ {npcObject.name} đã có đầy đủ components.");
        }
    }
    void SetupAnimationController(GameObject npcObject, NPCDialogueData npcData)
    {
        NPCAnimationController animController = npcObject.GetComponent<NPCAnimationController>();
        if (animController == null)
        {
            Debug.LogWarning($"Không tìm thấy NPCAnimationController trên {npcObject.name}");
            return;
        }

        // Setup basic animations
        animController.idleAnimationName = npcData.idleAnimation;
        animController.talkingAnimationName = npcData.talkingAnimation;  
        animController.greetingAnimationName = npcData.greetingAnimation;

        // Setup custom animations cho từng dòng thoại
        if (npcData.customAnimations != null && npcData.customAnimations.Length > 0)
        {
            DialogueAnimation[] dialogueAnimations = new DialogueAnimation[npcData.customAnimations.Length];
            
            for (int i = 0; i < npcData.customAnimations.Length; i++)
            {
                DialogueAnimationSetup setup = npcData.customAnimations[i];
                dialogueAnimations[i] = new DialogueAnimation
                {
                    dialogueLineIndex = setup.dialogueLineIndex,
                    animationName = setup.animationName,
                    delay = setup.delay,
                    duration = setup.duration,
                    returnToIdle = setup.returnToIdle,
                    playSound = setup.soundEffect != null,
                    soundEffect = setup.soundEffect
                };
            }

            animController.dialogueAnimations = dialogueAnimations;
            Debug.Log($"🎭 Đã setup {dialogueAnimations.Length} custom animations cho {npcObject.name}");
        }

        Debug.Log($"✅ Đã setup animation controller cho {npcObject.name}");
    }
    NPCDialogueData FindNPCData(string gameObjectName)
    {
        foreach (NPCDialogueData data in npcDataList)
        {
            if (gameObjectName.ToLower().Contains(data.npcName.ToLower()) ||
                data.npcName.ToLower().Contains(gameObjectName.ToLower()))
            {
                return data;
            }
        }
        return null;
    }

    void CreateInteractUI(DialogueTrigger trigger)
    {
        if (trigger.interactUI != null)
        {
            if (Application.isPlaying)
                Destroy(trigger.interactUI);
            else
                DestroyImmediate(trigger.interactUI);
        }
        GameObject uiObject;

        if (interactUIPrefab != null)
        {
            uiObject = Instantiate(interactUIPrefab);
        }
        else
        {
            uiObject = CreateSimpleInteractUI();
        }

        // Đặt UI làm con của trigger (NPC)
        uiObject.transform.SetParent(trigger.transform, false);

        // Set vị trí offset UI trên đầu NPC
        Vector3 offset = new Vector3(0, trigger.uiHeightOffset, 0);
        uiObject.transform.localPosition = offset;

        // Nếu là Canvas World Space, nên scale nhỏ lại
        Canvas canvas = uiObject.GetComponentInChildren<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
        {
            uiObject.transform.localScale = Vector3.one * 0.01f;
            canvas.worldCamera = Camera.main;
            canvas.sortingOrder = 100;
        }

        // Gán vào trigger
        trigger.interactUI = uiObject;

        Debug.Log("Đã tạo InteractUI cho: " + trigger.npcName);
        Debug.Log("InteractUI parent: " + uiObject.transform.parent.name);
}


    GameObject CreateSimpleInteractUI()
    {
        GameObject uiObject = new GameObject("InteractButton");

        uiObject.AddComponent<CanvasRenderer>();
        Image image = uiObject.AddComponent<Image>();

        Texture2D buttonTexture = CreateButtonTexture();
        Sprite buttonSprite = Sprite.Create(buttonTexture, new Rect(0, 0, buttonTexture.width, buttonTexture.height), Vector2.one * 0.5f);
        image.sprite = buttonSprite;

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(uiObject.transform, false);

        Text text = textObject.AddComponent<Text>();
        text.text = "E";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;

        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return uiObject;
    }

    Texture2D CreateButtonTexture()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size);
        Color[] colors = new Color[size * size];

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 2;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);

                if (distance <= radius)
                    colors[y * size + x] = new Color(0, 0, 0, 0.7f);
                else if (distance <= radius + 2)
                    colors[y * size + x] = Color.white;
                else
                    colors[y * size + x] = Color.clear;
            }
        }

        texture.SetPixels(colors);
        texture.Apply();

        return texture;
    }

#if UNITY_EDITOR
    [ContextMenu("Add Current Selected NPC")]
    public void AddSelectedNPC()
    {
        if (Selection.activeGameObject != null)
        {
            DialogueTrigger trigger = Selection.activeGameObject.GetComponent<DialogueTrigger>();
            if (trigger != null)
            {
                AddNPCToDataList(trigger.gameObject);
            }
            else
            {
                Debug.LogWarning("Selected object không có DialogueTrigger component");
            }
        }
        else
        {
            Debug.LogWarning("Chưa chọn GameObject nào");
        }
    }

    void AddNPCToDataList(GameObject npcObject)
    {
        string npcName = npcObject.name;

        if (FindNPCData(npcName) != null)
        {
            Debug.LogWarning($"NPC {npcName} đã có trong danh sách");
            return;
        }
        
        NPCDialogueData[] newArray = new NPCDialogueData[npcDataList.Length + 1];
        System.Array.Copy(npcDataList, newArray, npcDataList.Length);
        
        newArray[npcDataList.Length] = new NPCDialogueData
        {
            npcName = npcName,
            dialogueLines = new string[] { $"Xin chào! Tôi là {npcName}" },
            interactionRange = 2f,
            uiHeightOffset = 2.5f,
            useAnimations = true,
            idleAnimation = "Idle",
            talkingAnimation = "Talking"
        };

        npcDataList = newArray;
        
        EditorUtility.SetDirty(this);

        Debug.Log($"✅ Đã thêm NPC: {npcName} vào danh sách");
    }

    [ContextMenu("Remove Missing NPCs")]
    public void RemoveMissingNPCs()
    {
        if (npcDataList == null || npcDataList.Length == 0) return;

        System.Collections.Generic.List<NPCDialogueData> validNPCs = new System.Collections.Generic.List<NPCDialogueData>();
        int removedCount = 0;

        foreach (NPCDialogueData npcData in npcDataList)
        {
            GameObject npcObject = GameObject.Find(npcData.npcName);
            bool foundInScene = false;

            if (npcObject == null)
            {
                DialogueTrigger[] allTriggers = FindObjectsOfType<DialogueTrigger>();
                foreach (DialogueTrigger trigger in allTriggers)
                {
                    if (trigger.gameObject.name.ToLower().Contains(npcData.npcName.ToLower()))
                    {
                        foundInScene = true;
                        break;
                    }
                }
            }
            else
            {
                foundInScene = true;
            }

            if (foundInScene)
            {
                validNPCs.Add(npcData);
            }
            else
            {
                Debug.Log($"Đã loại bỏ NPC không tồn tại: {npcData.npcName}");
                removedCount++;
            }
        }

        npcDataList = validNPCs.ToArray();
        EditorUtility.SetDirty(this);

        Debug.Log($"Đã loại bỏ {removedCount} NPC không tồn tại khỏi danh sách");
    }

    [CustomEditor(typeof(NPCSetupHelper))]
    public class NPCSetupHelperEditor : Editor
    {
        private bool showNPCList = true;
        private bool showAdvancedOptions = false;

        public override void OnInspectorGUI()
        {
            NPCSetupHelper helper = (NPCSetupHelper)target;

            EditorGUILayout.LabelField("NPC Setup Helper", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            DrawBasicSetup(helper);
            EditorGUILayout.Space();
            
            DrawNPCList(helper);
            EditorGUILayout.Space();
            
            DrawAdvancedOptions(helper);
            EditorGUILayout.Space();
            
            DrawActionButtons(helper);
            EditorGUILayout.Space();
            
            DrawHelpSection();
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(helper);
            }
        }

        void DrawBasicSetup(NPCSetupHelper helper)
        {
            EditorGUILayout.LabelField("Cài đặt cơ bản", EditorStyles.boldLabel);

            helper.dialogueManager = (DialogueManager)EditorGUILayout.ObjectField(
                "Dialogue Manager", helper.dialogueManager, typeof(DialogueManager), true);

            helper.interactUIPrefab = (GameObject)EditorGUILayout.ObjectField(
                "Interact UI Prefab", helper.interactUIPrefab, typeof(GameObject), false);

            helper.autoSetupOnStart = EditorGUILayout.Toggle("Auto Setup On Start", helper.autoSetupOnStart);
        }

        void DrawNPCList(NPCSetupHelper helper)
        {
            showNPCList = EditorGUILayout.Foldout(showNPCList, $"Danh sách NPC ({helper.npcDataList?.Length ?? 0})");

            if (showNPCList)
            {
                // This will draw only the npcDataList field, which is what we want to show in the foldout.
                // Using a SerializedObject is the proper way to handle inspector fields.
                serializedObject.Update();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("npcDataList"), true);
                serializedObject.ApplyModifiedProperties();
            }
        }

        void DrawAdvancedOptions(NPCSetupHelper helper)
        {
            showAdvancedOptions = EditorGUILayout.Foldout(showAdvancedOptions, "Tùy chọn nâng cao");

            if (showAdvancedOptions)
            {
                EditorGUI.indentLevel++;

                helper.createInteractUIIfMissing = EditorGUILayout.Toggle(
                    "Tạo UI nếu thiếu", helper.createInteractUIIfMissing);

                helper.setupAnimationController = EditorGUILayout.Toggle(
                    "Setup Animation Controller", helper.setupAnimationController);

                helper.addMissingComponents = EditorGUILayout.Toggle(
                    "Thêm Component thiếu", helper.addMissingComponents);

                EditorGUI.indentLevel--;
            }
        }

        void DrawActionButtons(NPCSetupHelper helper)
        {
            EditorGUILayout.LabelField("Hành động", EditorStyles.boldLabel);

            if (GUILayout.Button("Setup All NPCs", GUILayout.Height(30)))
            {
                helper.SetupAllNPCs();
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add Selected NPC"))
            {
                helper.AddSelectedNPC();
            }

            if (GUILayout.Button("Remove Missing"))
            {
                helper.RemoveMissingNPCs();
            }

            EditorGUILayout.EndHorizontal();
        }

        void DrawHelpSection()
        {
            EditorGUILayout.HelpBox(
                "Hướng dẫn sử dụng:\n\n" +
                "1. Gán DialogueManager vào helper\n" +
                "2. Tạo các NPC với DialogueTrigger component\n" +
                "3. Chọn NPC và nhấn 'Add Selected NPC'\n" +
                "4. Cấu hình dialogue lines cho từng NPC\n" +
                "5. Nhấn 'Setup All NPCs' để tự động setup\n\n" +
                "Tips:\n" +
                "• Auto Setup On Start sẽ tự động setup khi chạy game\n" +
                "• Có thể tạo Interact UI Prefab để sử dụng chung\n" +
                "• Animation settings chỉ áp dụng khi có NPCAnimationController",
                MessageType.Info
            );
        }
    }
#endif
}
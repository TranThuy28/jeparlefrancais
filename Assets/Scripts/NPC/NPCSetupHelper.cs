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
    public float interactionRange = 2f;
    public float uiHeightOffset = 2.5f;
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
        CreateInteractUI(trigger);

        Debug.Log("Đã setup NPC: " + npcData.npcName);
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
                string npcName = trigger.gameObject.name;
                if (FindNPCData(npcName) == null)
                {
                    NPCDialogueData[] newArray = new NPCDialogueData[npcDataList.Length + 1];
                    System.Array.Copy(npcDataList, newArray, npcDataList.Length);

                    newArray[npcDataList.Length] = new NPCDialogueData
                    {
                        npcName = npcName,
                        dialogueLines = new string[] { "Xin chào! Tôi là " + npcName },
                        interactionRange = 2f,
                        uiHeightOffset = 2.5f
                    };

                    npcDataList = newArray;

                    Debug.Log("Đã thêm NPC: " + npcName + " vào danh sách");
                }
            }
        }
    }

    [CustomEditor(typeof(NPCSetupHelper))]
    public class NPCSetupHelperEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            NPCSetupHelper helper = (NPCSetupHelper)target;

            GUILayout.Space(10);

            if (GUILayout.Button("Setup All NPCs"))
            {
                helper.SetupAllNPCs();
            }

            if (GUILayout.Button("Add Selected NPC"))
            {
                helper.AddSelectedNPC();
            }

            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Cách sử dụng:\n" +
                "1. Tạo NPCSetupHelper trong scene\n" +
                "2. Gán DialogueManager vào helper\n" +
                "3. Thêm DialogueTrigger vào các NPC\n" +
                "4. Nhấn 'Setup All NPCs' để tự động setup\n" +
                "5. Hoặc chọn từng NPC và nhấn 'Add Selected NPC'",
                MessageType.Info
            );
        }
    }
#endif
}

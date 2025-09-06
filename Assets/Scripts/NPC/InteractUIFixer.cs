using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class InteractUIFixer : MonoBehaviour
{
    [Header("Fix Settings")]
    public bool fixOnStart = false;
    public bool addAnimationIfMissing = true;
    public bool fixCanvasSettings = true;
    public bool recreateUIIfBroken = false;
    
    [Header("Animation Settings")]
    public float pulseSpeed = 2f;
    public float minScale = 0.9f;
    public float maxScale = 1.1f;
    
    [Header("UI Recreation Settings")]
    public string displayText = "E";
    public Color backgroundColor = new Color(0, 0, 0, 0.7f);
    public Color textColor = Color.white;
    public int fontSize = 24;
    public Vector2 backgroundSize = new Vector2(80, 80);
    
    void Start()
    {
        if (fixOnStart)
        {
            FixInteractUI();
        }
    }
    
    [ContextMenu("Fix Interact UI")]
    public void FixInteractUI()
    {
        Debug.Log("Bắt đầu sửa chữa InteractUI cho: " + gameObject.name);
        
        // Kiểm tra và sửa Canvas settings
        if (fixCanvasSettings)
        {
            FixCanvasConfiguration();
        }
        
        // Kiểm tra animation
        if (addAnimationIfMissing)
        {
            EnsureAnimationExists();
        }
        
        // Recreate UI nếu cần
        if (recreateUIIfBroken && IsUIBroken())
        {
            RecreateInteractUI();
        }
        
        Debug.Log("Đã hoàn thành sửa chữa InteractUI!");
    }
    
    void FixCanvasConfiguration()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.Log("Thêm Canvas component...");
            canvas = gameObject.AddComponent<Canvas>();
        }
        
        // Setup Canvas cho WorldSpace
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        if (canvas.worldCamera == null)
        {
            canvas.worldCamera = FindObjectOfType<Camera>();
        }
        canvas.sortingOrder = 100;
        
        // Setup CanvasScaler
        CanvasScaler canvasScaler = GetComponent<CanvasScaler>();
        if (canvasScaler == null)
        {
            Debug.Log("Thêm CanvasScaler component...");
            canvasScaler = gameObject.AddComponent<CanvasScaler>();
        }
        
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        canvasScaler.dynamicPixelsPerUnit = 10f;
        
        // Setup RectTransform
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(2, 1);
            rectTransform.localScale = Vector3.one * 0.01f;
        }
        
        Debug.Log("Đã sửa Canvas configuration");
    }
    
    void EnsureAnimationExists()
    {
        // Tìm object có thể animate (background hoặc chính object này)
        GameObject targetForAnimation = FindAnimationTarget();
        
        if (targetForAnimation == null)
        {
            Debug.LogWarning("Không tìm thấy target để animate!");
            return;
        }
        
        // Kiểm tra xem đã có animation component chưa
        InteractUIAnimation existingAnimation = targetForAnimation.GetComponent<InteractUIAnimation>();
        if (existingAnimation != null)
        {
            Debug.Log("Animation đã tồn tại, cập nhật settings...");
            existingAnimation.scaleSpeed = pulseSpeed;
            existingAnimation.minScale = minScale;
            existingAnimation.maxScale = maxScale;
        }
        else
        {
            Debug.Log("Thêm animation component...");
            InteractUIAnimation newAnimation = targetForAnimation.AddComponent<InteractUIAnimation>();
            newAnimation.scaleSpeed = pulseSpeed;
            newAnimation.minScale = minScale;
            newAnimation.maxScale = maxScale;
        }
    }
    
    GameObject FindAnimationTarget()
    {
        // Tìm background object
        Transform backgroundTransform = transform.Find("Background");
        if (backgroundTransform != null)
        {
            return backgroundTransform.gameObject;
        }
        
        // Tìm object có Image component
        Image[] images = GetComponentsInChildren<Image>();
        if (images.Length > 0)
        {
            return images[0].gameObject;
        }
        
        // Fallback: sử dụng chính object này
        return gameObject;
    }
    
    bool IsUIBroken()
    {
        // Kiểm tra xem UI có hoạt động bình thường không
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null) return true;
        
        // Kiểm tra có Text hoặc Image component không
        Text[] texts = GetComponentsInChildren<Text>();
        Image[] images = GetComponentsInChildren<Image>();
        
        return texts.Length == 0 && images.Length == 0;
    }
    
    void RecreateInteractUI()
    {
        Debug.Log("Recreating InteractUI từ đầu...");
        
        // Xóa các component cũ (trừ Transform và script này)
        Component[] components = GetComponents<Component>();
        foreach (Component comp in components)
        {
            if (comp != null && comp != transform && comp != this)
            {
                DestroyImmediate(comp);
            }
        }
        
        // Xóa tất cả children
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        
        // Tạo lại UI hoàn toàn mới
        CreateCompleteUI();
    }
    
    void CreateCompleteUI()
    {
        // Tạo Canvas
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        if (canvas.worldCamera == null)
        {
            canvas.worldCamera = FindObjectOfType<Camera>();
        }
        canvas.sortingOrder = 100;
        
        // Tạo CanvasScaler
        CanvasScaler canvasScaler = gameObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        canvasScaler.dynamicPixelsPerUnit = 10f;
        
        // Setup RectTransform
        RectTransform canvasRect = GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(2, 1);
        canvasRect.localScale = Vector3.one * 0.01f;
        
        // Tạo background
        GameObject backgroundObj = CreateBackground();
        
        // Tạo text
        CreateText(backgroundObj);
        
        // Thêm animation
        InteractUIAnimation animation = backgroundObj.AddComponent<InteractUIAnimation>();
        animation.scaleSpeed = pulseSpeed;
        animation.minScale = minScale;
        animation.maxScale = maxScale;
    }
    
    GameObject CreateBackground()
    {
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(transform, false);
        
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.sprite = CreateCircleSprite();
        bgImage.color = backgroundColor;
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.pivot = new Vector2(0.5f, 0.5f);
        bgRect.anchoredPosition = Vector2.zero;
        bgRect.sizeDelta = backgroundSize;
        
        return bgObj;
    }
    
    void CreateText(GameObject parent)
    {
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(parent.transform, false);
        
        Text textComponent = textObj.AddComponent<Text>();
        textComponent.text = displayText;
        textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComponent.fontSize = fontSize;
        textComponent.color = textColor;
        textComponent.alignment = TextAnchor.MiddleCenter;
        textComponent.fontStyle = FontStyle.Bold;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
    
    Sprite CreateCircleSprite()
    {
        int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] colors = new Color[size * size];
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 4;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                
                if (distance <= radius)
                {
                    float alpha = Mathf.Lerp(0.9f, 0.6f, distance / radius);
                    colors[y * size + x] = new Color(0.1f, 0.1f, 0.1f, alpha);
                }
                else if (distance <= radius + 3)
                {
                    colors[y * size + x] = new Color(1f, 1f, 1f, 0.8f);
                }
                else
                {
                    colors[y * size + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, 100f);
    }
    
    [ContextMenu("Remove Converter Script")]
    public void RemoveConverterScript()
    {
        TextMeshToUIConverter converter = GetComponent<TextMeshToUIConverter>();
        if (converter != null)
        {
            Debug.Log("Xóa TextMeshToUIConverter script...");
            DestroyImmediate(converter);
        }
    }
    
    [ContextMenu("Diagnose UI State")]
    public void DiagnoseUIState()
    {
        Debug.Log("=== CHẨN ĐOÁN UI STATE ===");
        Debug.Log("GameObject: " + gameObject.name);
        
        // Kiểm tra Canvas
        Canvas canvas = GetComponent<Canvas>();
        Debug.Log("Canvas: " + (canvas != null ? "✓ Có" : "✗ Không có"));
        if (canvas != null)
        {
            Debug.Log("- Render Mode: " + canvas.renderMode);
            Debug.Log("- World Camera: " + (canvas.worldCamera != null ? canvas.worldCamera.name : "Null"));
        }
        
        // Kiểm tra Animation
        InteractUIAnimation[] animations = GetComponentsInChildren<InteractUIAnimation>();
        Debug.Log("Animation Components: " + animations.Length);
        
        // Kiểm tra UI Elements
        Image[] images = GetComponentsInChildren<Image>();
        Text[] texts = GetComponentsInChildren<Text>();
        Debug.Log("Images: " + images.Length + ", Texts: " + texts.Length);
        
        // Kiểm tra Children
        Debug.Log("Child Objects: " + transform.childCount);
        for (int i = 0; i < transform.childCount; i++)
        {
            Debug.Log("- Child " + i + ": " + transform.GetChild(i).name);
        }
        
        Debug.Log("=== KET THÚC CHẨN ĐOÁN ===");
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(InteractUIFixer))]
public class InteractUIFixerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        InteractUIFixer fixer = (InteractUIFixer)target;
        
        GUILayout.Space(10);
        
        EditorGUILayout.LabelField("Quick Fix Tools", EditorStyles.boldLabel);
        
        if (GUILayout.Button("🔧 Fix Interact UI", GUILayout.Height(30)))
        {
            fixer.FixInteractUI();
        }
        
        GUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🔍 Diagnose"))
        {
            fixer.DiagnoseUIState();
        }
        
        if (GUILayout.Button("🗑️ Remove Converter"))
        {
            fixer.RemoveConverterScript();
        }
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "Cách sử dụng:\n" +
            "1. Attach script này vào InteractUI bị lỗi\n" +
            "2. Nhấn 'Diagnose' để xem tình trạng\n" +
            "3. Nhấn 'Fix Interact UI' để sửa\n" +
            "4. Check 'Recreate UI If Broken' nếu UI hỏng hoàn toàn\n" +
            "5. Nhấn 'Remove Converter' để xóa script cũ",
            MessageType.Info
        );
        
        // Hiển thị trạng thái hiện tại
        Canvas canvas = fixer.GetComponent<Canvas>();
        InteractUIAnimation[] animations = fixer.GetComponentsInChildren<InteractUIAnimation>();
        
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Current State:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Canvas: " + (canvas != null ? "✓" : "✗"));
        EditorGUILayout.LabelField("Animation: " + (animations.Length > 0 ? "✓" : "✗"));
        EditorGUILayout.LabelField("Children: " + fixer.transform.childCount);
    }
}
#endif
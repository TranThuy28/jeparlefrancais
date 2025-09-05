#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextMeshToUIConverter : MonoBehaviour
{
    [Header("Conversion Settings")]
    public bool convertOnStart = false;
    public bool addBackground = true;
    public bool addAnimation = true;
    
    [Header("UI Styling")]
    public Color backgroundColor = new Color(0, 0, 0, 0.7f);
    public Color textColor = Color.white;
    public int fontSize = 24;
    public Vector2 backgroundSize = new Vector2(80, 80);
    
    [Header("Animation Settings")]
    public float pulseSpeed = 2f;
    public float minScale = 0.9f;
    public float maxScale = 1.1f;
    
    void Start()
    {
        if (convertOnStart)
        {
            ConvertToUI();
        }
    }
    
    [ContextMenu("Debug Components")]
    public void DebugComponents()
    {
        Debug.Log("=== DEBUGGING COMPONENTS ===");
        
        if (gameObject == null)
        {
            Debug.LogError("GameObject is null!");
            return;
        }
        
        Debug.Log("GameObject: " + gameObject.name);
        
        // Kiểm tra tất cả components trong children
        try
        {
            Component[] allComponents = GetComponentsInChildren<Component>();
            if (allComponents != null && allComponents.Length > 0)
            {
                foreach (Component comp in allComponents)
                {
                    if (comp != null && comp.gameObject != null)
                    {
                        Debug.Log("Found component: " + comp.GetType().Name + " on GameObject: " + comp.gameObject.name);
                    }
                }
            }
            else
            {
                Debug.Log("No components found in children");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error getting components: " + e.Message);
        }
        
        // Kiểm tra cụ thể các loại text component
        try
        {
            TextMesh textMesh = GetComponentInChildren<TextMesh>();
            Text uiText = GetComponentInChildren<Text>();
            TextMeshPro tmp = GetComponentInChildren<TextMeshPro>();
            TextMeshProUGUI tmpUGUI = GetComponentInChildren<TextMeshProUGUI>();
            
            Debug.Log("TextMesh found: " + (textMesh != null && textMesh.gameObject != null ? textMesh.gameObject.name : "None"));
            Debug.Log("UI Text found: " + (uiText != null && uiText.gameObject != null ? uiText.gameObject.name : "None"));
            Debug.Log("TextMeshPro found: " + (tmp != null && tmp.gameObject != null ? tmp.gameObject.name : "None"));
            Debug.Log("TextMeshProUGUI found: " + (tmpUGUI != null && tmpUGUI.gameObject != null ? tmpUGUI.gameObject.name : "None"));
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error checking text components: " + e.Message);
        }
        
        // Kiểm tra hierarchy structure
        Debug.Log("=== HIERARCHY STRUCTURE ===");
        LogHierarchy(transform, 0);
    }
    
    void LogHierarchy(Transform parent, int depth)
    {
        if (parent == null) return;
        
        string indent = new string(' ', depth * 2);
        string componentsList = "";
        
        Component[] components = parent.GetComponents<Component>();
        if (components != null)
        {
            foreach (Component comp in components)
            {
                if (comp != null)
                {
                    componentsList += comp.GetType().Name + " ";
                }
            }
        }
        
        Debug.Log(indent + parent.name + " - Components: [" + componentsList + "]");
        
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child != null)
            {
                LogHierarchy(child, depth + 1);
            }
        }
    }
    
    [ContextMenu("Convert TextMesh to UI")]
    public void ConvertToUI()
    {
        string originalText = "";
        GameObject textSourceObject = null;
        
        // Tìm kiếm tất cả các loại text component có thể có
        TextMesh textMesh = GetComponentInChildren<TextMesh>();
        Text uiText = GetComponentInChildren<Text>();
        TextMeshPro tmp = GetComponentInChildren<TextMeshPro>();
        TextMeshProUGUI tmpUGUI = GetComponentInChildren<TextMeshProUGUI>();
        
        // Xác định loại component và lấy text
        if (textMesh != null)
        {
            originalText = textMesh.text;
            textSourceObject = textMesh.gameObject;
            Debug.Log("Found TextMesh component");
            
            // Xóa TextMesh và MeshRenderer
            DestroyImmediate(textMesh);
            MeshRenderer meshRenderer = textSourceObject.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                DestroyImmediate(meshRenderer);
            }
        }
        else if (tmp != null)
        {
            originalText = tmp.text;
            textSourceObject = tmp.gameObject;
            Debug.Log("Found TextMeshPro component");
            
            // Xóa TextMeshPro và MeshRenderer
            DestroyImmediate(tmp);
            MeshRenderer meshRenderer = textSourceObject.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                DestroyImmediate(meshRenderer);
            }
        }
        else if (tmpUGUI != null)
        {
            originalText = tmpUGUI.text;
            textSourceObject = tmpUGUI.gameObject;
            Debug.Log("Found TextMeshProUGUI component");
            
            // Xóa TextMeshProUGUI và các UI components cũ
            DestroyImmediate(tmpUGUI);
            
            // Xóa CanvasRenderer nếu có
            CanvasRenderer canvasRenderer = textSourceObject.GetComponent<CanvasRenderer>();
            if (canvasRenderer != null)
            {
                DestroyImmediate(canvasRenderer);
            }
        }
        else if (uiText != null)
        {
            originalText = uiText.text;
            textSourceObject = uiText.gameObject;
            Debug.Log("Found UI Text component");
            
            // Xóa UI Text
            DestroyImmediate(uiText);
        }
        else
        {
            Debug.LogError("Không tìm thấy bất kỳ text component nào trong " + gameObject.name + " hoặc children của nó!");
            Debug.LogError("Hãy chạy 'Debug Components' để xem có component gì.");
            return;
        }
        
        // Tạo Canvas cho WorldSpace UI
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        canvas.sortingOrder = 100;
        
        // Setup Canvas Scaler
        CanvasScaler canvasScaler = gameObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        canvasScaler.dynamicPixelsPerUnit = 10f;
        
        // Setup RectTransform của Canvas
        RectTransform canvasRect = GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(2, 1);
        canvasRect.localScale = Vector3.one * 0.01f;
        
        // Tạo background nếu cần
        GameObject backgroundObj = null;
        if (addBackground)
        {
            backgroundObj = CreateBackground();
        }
        
        // Sử dụng lại GameObject cũ thay vì tạo mới
        GameObject textObj = ConvertExistingTextObject(textSourceObject, originalText, backgroundObj);
        
        // Thêm animation nếu cần
        if (addAnimation)
        {
            AddPulseAnimation(backgroundObj != null ? backgroundObj : textObj);
        }
        
        Debug.Log("Đã convert text component thành UI cho: " + gameObject.name);
    }
    
    GameObject ConvertExistingTextObject(GameObject existingTextObj, string text, GameObject parent)
    {
        if (existingTextObj == null)
        {
            // Fallback: tạo mới nếu không có GameObject cũ
            return CreateTextUI(text, parent);
        }
        
        // Di chuyển GameObject cũ thành con của parent mới (nếu có background)
        if (parent != null)
        {
            existingTextObj.transform.SetParent(parent.transform, false);
        }
        else
        {
            existingTextObj.transform.SetParent(transform, false);
        }
        
        // Thêm Text component mới
        Text textComponent = existingTextObj.AddComponent<Text>();
        textComponent.text = text;
        textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComponent.fontSize = fontSize;
        textComponent.color = textColor;
        textComponent.alignment = TextAnchor.MiddleCenter;
        textComponent.fontStyle = FontStyle.Bold;
        
        // Setup RectTransform (đã có sẵn)
        RectTransform textRect = existingTextObj.GetComponent<RectTransform>();
        if (textRect != null)
        {
            if (parent != null)
            {
                // Nếu có parent, fill toàn bộ parent
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
                textRect.anchoredPosition = Vector2.zero;
            }
            else
            {
                // Nếu không có parent, giữ nguyên size hiện tại
                textRect.anchoredPosition = Vector2.zero;
            }
        }
        
        Debug.Log("Converted existing GameObject: " + existingTextObj.name);
        return existingTextObj;
    }
    
    GameObject CreateBackground()
    {
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(transform, false);
        
        // Thêm Image component
        Image bgImage = bgObj.AddComponent<Image>();
        
        // Tạo sprite hình tròn
        Sprite circleSprite = CreateCircleSprite();
        bgImage.sprite = circleSprite;
        bgImage.color = backgroundColor;
        bgImage.type = Image.Type.Simple;
        
        // Setup RectTransform
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.pivot = new Vector2(0.5f, 0.5f);
        bgRect.anchoredPosition = Vector2.zero;
        bgRect.sizeDelta = backgroundSize;
        
        return bgObj;
    }
    
    GameObject CreateTextUI(string text, GameObject parent)
    {
        GameObject textObj = new GameObject("Text");
        Transform parentTransform = parent != null ? parent.transform : transform;
        textObj.transform.SetParent(parentTransform, false);
        
        // Thêm Text component
        Text textComponent = textObj.AddComponent<Text>();
        textComponent.text = text;
        textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComponent.fontSize = fontSize;
        textComponent.color = textColor;
        textComponent.alignment = TextAnchor.MiddleCenter;
        textComponent.fontStyle = FontStyle.Bold;
        
        // Setup RectTransform
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        return textObj;
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
                    // Trong vòng tròn
                    float alpha = Mathf.Lerp(0.9f, 0.6f, distance / radius);
                    colors[y * size + x] = new Color(0.1f, 0.1f, 0.1f, alpha);
                }
                else if (distance <= radius + 3)
                {
                    // Viền
                    colors[y * size + x] = new Color(1f, 1f, 1f, 0.8f);
                }
                else
                {
                    // Ngoài viền
                    colors[y * size + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, 100f);
    }
    
    void AddPulseAnimation(GameObject target)
    {
        InteractUIAnimation animation = target.AddComponent<InteractUIAnimation>();
        animation.scaleSpeed = pulseSpeed;
        animation.minScale = minScale;
        animation.maxScale = maxScale;
    }
}

// Script animation đơn giản
public class InteractUIAnimation : MonoBehaviour
{
    [Header("Pulse Animation")]
    public float scaleSpeed = 2f;
    public float minScale = 0.9f;
    public float maxScale = 1.1f;
    
    [Header("Look At Camera")]
    public bool lookAtCamera = true;
    
    private Vector3 originalScale;
    private Camera mainCamera;
    
    void Start()
    {
        originalScale = transform.localScale;
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
    }
    
    void Update()
    {
        // Pulse animation
        float scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(Time.time * scaleSpeed) + 1) / 2);
        transform.localScale = originalScale * scale;
        
        // Look at camera
        if (lookAtCamera && mainCamera != null)
        {
            Vector3 directionToCamera = mainCamera.transform.position - transform.position;
            if (directionToCamera != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(-directionToCamera);
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TextMeshToUIConverter))]
public class TextMeshToUIConverterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        TextMeshToUIConverter converter = (TextMeshToUIConverter)target;
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("🔍 Debug Components", GUILayout.Height(25)))
        {
            converter.DebugComponents();
        }
        
        GUILayout.Space(5);
        
        if (GUILayout.Button("Convert TextMesh to UI", GUILayout.Height(30)))
        {
            converter.ConvertToUI();
        }
        
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "Cách sử dụng:\n" +
            "1. Nhấn 'Debug Components' để xem có text component gì\n" +
            "2. Script sẽ tự động tìm TextMesh, TextMeshPro, hoặc UI Text\n" +
            "3. Nhấn 'Convert TextMesh to UI' để convert\n" +
            "4. Kiểm tra Console để xem kết quả",
            MessageType.Info
        );
        
        // Kiểm tra và hiển thị trạng thái
        TextMesh textMesh = converter.GetComponentInChildren<TextMesh>();
        Text uiText = converter.GetComponentInChildren<Text>();
        TextMeshPro tmp = converter.GetComponentInChildren<TextMeshPro>();
        TextMeshProUGUI tmpUGUI = converter.GetComponentInChildren<TextMeshProUGUI>();
        Canvas canvas = converter.GetComponent<Canvas>();
        
        if (canvas != null)
        {
            EditorGUILayout.HelpBox("✅ Đã convert thành công!", MessageType.Info);
        }
        else if (textMesh != null || uiText != null || tmp != null || tmpUGUI != null)
        {
            string foundComponents = "";
            if (textMesh != null) foundComponents += "TextMesh ";
            if (uiText != null) foundComponents += "UI Text ";
            if (tmp != null) foundComponents += "TextMeshPro ";
            if (tmpUGUI != null) foundComponents += "TextMeshProUGUI ";
            
            EditorGUILayout.HelpBox("📝 Tìm thấy: " + foundComponents + "- sẵn sàng convert!", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("⚠️ Không tìm thấy text component nào! Hãy nhấn 'Debug Components'", MessageType.Warning);
        }
    }
}
#endif
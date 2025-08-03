using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

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

    [ContextMenu("Convert TextMesh or TMP to UI")]
    public void ConvertToUI()
    {
        string originalText = "";

        // Try get TextMesh or TMP_Text
        TextMesh textMesh = GetComponent<TextMesh>();
        TMP_Text tmpText = GetComponent<TMP_Text>();

        if (textMesh != null)
        {
            originalText = textMesh.text;
            DestroyImmediate(textMesh);
            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr != null) DestroyImmediate(mr);
        }
        else if (tmpText != null)
        {
            originalText = tmpText.text;
            DestroyImmediate(tmpText);
        }
        else
        {
            Debug.LogError("Không tìm thấy TextMesh hoặc TextMeshPro component!");
            return;
        }

        // Add Canvas
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        canvas.sortingOrder = 100;

        CanvasScaler canvasScaler = gameObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        canvasScaler.dynamicPixelsPerUnit = 10f;

        RectTransform canvasRect = GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(2, 1);
        canvasRect.localScale = Vector3.one * 0.01f;

        GameObject backgroundObj = null;
        if (addBackground)
        {
            backgroundObj = CreateBackground();
        }

        GameObject textObj = CreateTextUI(originalText, backgroundObj);

        if (addAnimation)
        {
            AddPulseAnimation(backgroundObj != null ? backgroundObj : textObj);
        }

        Debug.Log("Đã convert Text thành WorldSpace UI cho: " + gameObject.name);
    }

    GameObject CreateBackground()
    {
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(transform, false);

        Image bgImage = bgObj.AddComponent<Image>();
        Sprite circleSprite = CreateCircleSprite();
        bgImage.sprite = circleSprite;
        bgImage.color = backgroundColor;
        bgImage.type = Image.Type.Simple;

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

        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.color = textColor;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.fontStyle = FontStyles.Bold;

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

    void AddPulseAnimation(GameObject target)
    {
        InteractUIAnimation animation = target.AddComponent<InteractUIAnimation>();
        animation.scaleSpeed = pulseSpeed;
        animation.minScale = minScale;
        animation.maxScale = maxScale;
    }
}

public class InteractUIAnimation : MonoBehaviour
{
    public float scaleSpeed = 2f;
    public float minScale = 0.9f;
    public float maxScale = 1.1f;

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
        float scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(Time.time * scaleSpeed) + 1) / 2);
        transform.localScale = originalScale * scale;

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

        if (GUILayout.Button("Convert TextMesh to UI", GUILayout.Height(30)))
        {
            converter.ConvertToUI();
        }

        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "Cách sử dụng:\n" +
            "1. Gán script vào GameObject có TextMesh hoặc TextMeshPro\n" +
            "2. Nhấn 'Convert TextMesh to UI'\n" +
            "3. Text sẽ được chuyển thành WorldSpace UI",
            MessageType.Info
        );

        if (converter.GetComponent<Canvas>() != null)
        {
            EditorGUILayout.HelpBox("✅ Đã convert thành công!", MessageType.Info);
        }
    }
}
#endif

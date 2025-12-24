using UnityEngine;

public class FloodController : MonoBehaviour
{
    [Header("Water Levels Setup")]
    [Tooltip("Height values for each flood level. Element 0 will be captured from current height, others auto-filled (+1 each).")]
    public float[] waterLevels = new float[12];

    [Header("Tween Settings")]
    public float tweenDuration = 5f;
    public LeanTweenType easeType = LeanTweenType.easeInOutSine;

    /// <summary>
    /// Capture the current Y position as level 0 and auto-fill the rest
    /// (each next level = previous level + 1.0f).
    /// </summary>
    [ContextMenu("Capture Current Height as Level 0")]
    private void CaptureCurrentHeightAsLevel0()
    {
        if (waterLevels == null || waterLevels.Length == 0)
        {
            waterLevels = new float[12];
        }

        float baseHeight = transform.position.y;
        waterLevels[0] = baseHeight;

        for (int i = 1; i < waterLevels.Length; i++)
        {
            waterLevels[i] = waterLevels[i - 1] + 1.0f;
        }

        Debug.Log($"[FloodController] Captured current height {baseHeight} as Level 0 and auto-filled {waterLevels.Length} levels.");
    }

    /// <summary>
    /// Tween the water plane to the requested level height.
    /// Accepts both 1-based (1..N) and 0-based (0..N-1) indices.
    /// </summary>
    public void SetFloodLevel(int levelIndex)
    {
        if (waterLevels == null || waterLevels.Length == 0)
        {
            Debug.LogError("[FloodController] waterLevels is not configured. Use the context menu to capture heights.");
            return;
        }

        int arrayIndex = levelIndex;
        // Support 1-based level indices (Level 1..N)
        if (levelIndex >= 1 && levelIndex <= waterLevels.Length)
        {
            arrayIndex = levelIndex - 1;
        }

        if (arrayIndex < 0 || arrayIndex >= waterLevels.Length)
        {
            Debug.LogWarning($"[FloodController] Invalid level index {levelIndex}. Valid: 1..{waterLevels.Length} or 0..{waterLevels.Length - 1}.");
            return;
        }

        float currentY = transform.position.y;
        float targetY = waterLevels[arrayIndex];

        // Cancel any existing tweens on this GameObject
        LeanTween.cancel(gameObject);

        Debug.Log($"Moving Water from {currentY} to {targetY}");

        // Move the object in world-space Y using LeanTween
        LeanTween.moveY(gameObject, targetY, tweenDuration)
                 .setEase(easeType);
    }
}
using UnityEngine;

/// <summary>
/// Simple buoyancy/bobbing controller.
/// Attach this to the Floating School parent object.
/// Requires a reference to the WaterPlane transform.
/// </summary>
public class SimpleBuoyancy : MonoBehaviour
{
    [Header("Water Reference")]
    [Tooltip("Transform of the WaterPlane object.")]
    public Transform waterPlane;

    [Header("Vertical Offset")]
    [Tooltip("Base offset above the water surface.")]
    public float heightOffset = 1.0f;

    [Header("Bobbing (Vertical Sin Wave)")]
    [Tooltip("Amplitude of vertical bobbing in units.")]
    public float bobAmplitude = 0.25f;
    [Tooltip("Frequency of vertical bobbing in Hz.")]
    public float bobFrequency = 0.5f;

    [Header("Tilt (Z Rotation Sin Wave)")]
    [Tooltip("Maximum tilt angle on Z axis in degrees.")]
    public float tiltAmplitude = 3f;
    [Tooltip("Frequency of tilt in Hz.")]
    public float tiltFrequency = 0.4f;

    [Header("Position Locking")]
    [Tooltip("If true, keeps the X/Z position fixed at the initial world position to avoid drifting/flying away.")]
    public bool lockXZToInitial = true;

    private Vector3 initialLocalEulerAngles;
    private Vector3 initialWorldPosition;

    private void Awake()
    {
        initialLocalEulerAngles = transform.localEulerAngles;
        initialWorldPosition = transform.position;
    }

    private void Update()
    {
        if (waterPlane == null)
        {
            return;
        }

        // Safety: avoid feedback loop if the water is a child of this object
        if (waterPlane.IsChildOf(transform))
        {
            Debug.LogWarning("[SimpleBuoyancy] waterPlane is a child of this object. This can cause flying/feedback-loop behaviour. Please separate them.");
            return;
        }

        float time = Time.time;

        // Base Y = water height + constant offset
        float baseY = waterPlane.position.y + heightOffset;

        // Add bobbing using a sine wave
        float bobOffset = bobAmplitude * Mathf.Sin(time * Mathf.PI * 2f * bobFrequency);
        float targetY = baseY + bobOffset;

        Vector3 position = transform.position;
        if (lockXZToInitial)
        {
            position.x = initialWorldPosition.x;
            position.z = initialWorldPosition.z;
        }
        position.y = targetY;
        transform.position = position;

        // Add slight rocking/tilt on Z axis using another sine wave
        float tiltOffset = tiltAmplitude * Mathf.Sin(time * Mathf.PI * 2f * tiltFrequency);
        Vector3 euler = initialLocalEulerAngles;
        euler.z += tiltOffset;
        transform.localEulerAngles = euler;
    }
}



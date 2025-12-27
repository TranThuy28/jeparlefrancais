using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ManualCharacterController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 4f;
    public float rotationSpeed = 10f;

    [Header("Jump Settings")]
    public float jumpHeight = 1.2f;
    public float gravity = -15.0f;

    [Header("Swimming Settings")]
    [Tooltip("Reference to the water plane (FloodController Transform).")]
    public Transform waterPlane;
    
    [Tooltip("Chest height offset from water surface to determine if player is swimming.")]
    public float swimLevelOffset = 1.2f;
    
    [Tooltip("Movement speed multiplier when swimming (typically slower than ground movement).")]
    public float swimSpeedMultiplier = 0.7f;

    [Header("References")]
    public Transform cameraTransform;
    public Animator animator;

    [Header("Cutscene Control")]
    [Tooltip("Set to false during cutscenes to disable gravity")]
    public bool enableGravity = true;
    
    private CharacterController controller;
    private Vector3 verticalVelocity = Vector3.zero;
    private bool isSwimming = false;
    private Vector3 horizontalVelocity = Vector3.zero; // Store horizontal velocity for forward momentum preservation
    
    /// <summary>
    /// Public method to check if the player is currently swimming.
    /// Used by other scripts like BreathManager.
    /// </summary>
    public bool IsSwimming()
    {
        return isSwimming;
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        // Auto-find waterPlane if not assigned (look for FloodController)
        if (waterPlane == null)
        {
            FloodController floodController = FindObjectOfType<FloodController>();
            if (floodController != null)
            {
                waterPlane = floodController.transform;
            }
        }
    }

    void Update()
    {
        if (controller == null || !controller.enabled) return;

        // Get input
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(h, 0, v).normalized;

        // Calculate move direction based on camera
        Vector3 moveDir = Vector3.zero;
        if (inputDir.magnitude > 0.1f && cameraTransform != null)
        {
            // Tính hướng theo camera
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            moveDir = (camForward * v + camRight * h).normalized;

            // Xoay mượt nhân vật (only rotate when not swimming or when moving)
            if (!isSwimming || inputDir.magnitude > 0.1f)
            {
                float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
                float smoothAngle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, rotationSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
            }
        }

        // Check if player is in water (SWIMMING STATE DETECTION)
        bool wasSwimming = isSwimming;
        if (waterPlane != null && enableGravity)
        {
            // Enter Swim State if player position is at or below water level minus offset
            isSwimming = transform.position.y <= waterPlane.position.y - swimLevelOffset;
        }
        else
        {
            isSwimming = false;
        }

        // Update Animator for swimming state
        if (animator != null)
        {
            animator.SetBool("isSwimming", isSwimming);
            // Set movement blend for swimming (0 = treading water/idle, 1 = swimming/moving)
            float inputMagnitude = inputDir.magnitude;
            animator.SetFloat("swimBlend", isSwimming ? inputMagnitude : 0f);
        }

        // Handle Jumping (only when grounded and not swimming)
        if (enableGravity && !isSwimming)
        {
            if (controller.isGrounded)
            {
                // Reset vertical velocity when grounded
                verticalVelocity.y = -2f;
                
                // Check for jump input
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    // Calculate jump velocity using physics formula: v = sqrt(2 * h * -g)
                    verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                }
            }
            else
            {
                // Apply gravity when airborne
                verticalVelocity.y += gravity * Time.deltaTime;
            }
        }

        // Handle Swimming Physics
        if (isSwimming && enableGravity)
        {
            // Disable gravity and apply buoyancy
            if (waterPlane != null)
            {
                float targetY = waterPlane.position.y - swimLevelOffset;
                float currentY = transform.position.y;
                
                // Smoothly move player to water level (buoyancy effect)
                if (Mathf.Abs(currentY - targetY) > 0.1f)
                {
                    verticalVelocity.y = (targetY - currentY) * 5f; // Buoyancy force multiplier
                    verticalVelocity.y = Mathf.Clamp(verticalVelocity.y, -5f, 5f); // Limit vertical velocity
                }
                else
                {
                    verticalVelocity.y = 0f;
                    // Force position to stay at water level
                    Vector3 pos = transform.position;
                    pos.y = targetY;
                    transform.position = pos;
                }
            }
        }

        // Calculate horizontal movement velocity (PRESERVE FORWARD MOMENTUM)
        float currentMoveSpeed = isSwimming ? moveSpeed * swimSpeedMultiplier : moveSpeed;
        Vector3 horizontalMove = moveDir * currentMoveSpeed;
        
        // Preserve horizontal velocity when jumping (forward momentum)
        if (!controller.isGrounded && !isSwimming && horizontalMove.magnitude > 0.01f)
        {
            // Update horizontal velocity with input while airborne
            horizontalVelocity = horizontalMove;
        }
        else if (controller.isGrounded || isSwimming)
        {
            // Update horizontal velocity normally when grounded or swimming
            horizontalVelocity = horizontalMove;
        }
        // Else: keep previous horizontalVelocity to preserve momentum in air

        // Combine horizontal and vertical movement
        Vector3 finalMove = horizontalVelocity + Vector3.up * verticalVelocity.y;
        controller.Move(finalMove * Time.deltaTime);

        // Update Animator for walking/running
        if (animator != null && !isSwimming)
        {
            float moveMagnitude = horizontalMove.magnitude;
            animator.SetBool("isWalking", moveMagnitude > 0.1f);
        }
    }

    // PUBLIC METHODS cho CutsceneManager
    public void DisableGravity()
    {
        enableGravity = false;
        verticalVelocity.y = controller.isGrounded ? -2f : 0f;
        Debug.Log("Gravity disabled for cutscene");
    }
    
    public void EnableGravity()
    {
        enableGravity = true;
        verticalVelocity.y = controller.isGrounded ? -2f : 0f;
        Debug.Log("Gravity enabled after cutscene");
    }
    
    public void ResetVerticalVelocity()
    {
        verticalVelocity.y = -2f;
        Debug.Log("Vertical velocity reset");
    }
    
    private Vector3 AdjustVelocityToSlope(Vector3 velocity)
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 0.2f))
        {
            Quaternion slopeRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            Vector3 adjustedVelocity = slopeRotation * velocity;

            if (adjustedVelocity.y < 0) // nếu hướng bị nghiêng xuống
                return adjustedVelocity;
        }

        return velocity; // nếu không có slope, giữ nguyên
    }
}

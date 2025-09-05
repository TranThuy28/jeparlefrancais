using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ManualCharacterController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 4f;
    public float rotationSpeed = 10f;

    [Header("References")]
    public Transform cameraTransform;
    public Animator animator;

    [Header("Cutscene Control")]
    [Tooltip("Set to false during cutscenes to disable gravity")]
    public bool enableGravity = true;
    
    private CharacterController controller;
    private float gravity = -9.81f;
    private float verticalVelocity = 0f;

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
    }

    void Update()
    {
        if (controller == null || !controller.enabled) return;
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(h, 0, v).normalized;

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

            // Xoay mượt nhân vật
            float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
            float smoothAngle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
        }

        // FIX: Chỉ xử lý trọng lực khi enableGravity = true
        if (enableGravity)
        {
            if (controller.isGrounded)
            {
                verticalVelocity = -2f;
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }
        }
        else
        {
            // Trong cutscene: giữ vertical velocity ở mức tối thiểu để dính mặt đất
            verticalVelocity = controller.isGrounded ? -2f : 0f;
        }

        // Tổng vector di chuyển
        Vector3 finalMove = moveDir * moveSpeed + Vector3.up * verticalVelocity;
        controller.Move(finalMove * Time.deltaTime);


    }
    // PUBLIC METHODS cho CutsceneManager
    public void DisableGravity()
    {
        enableGravity = false;
        verticalVelocity = controller.isGrounded ? -2f : 0f;
        Debug.Log("Gravity disabled for cutscene");
    }
    
    public void EnableGravity()
    {
        enableGravity = true;
        verticalVelocity = controller.isGrounded ? -2f : 0f;
        Debug.Log("Gravity enabled after cutscene");
    }
    
    public void ResetVerticalVelocity()
    {
        verticalVelocity = -2f;
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

using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ManualCharacterController : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float rotationSpeed = 10f;
    public Transform cameraTransform;

    private CharacterController controller;
    public Animator animator;
    private float gravity = -9.81f;
    private float verticalVelocity = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        
    }

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(h, 0, v).normalized;

        if (inputDir.magnitude > 0.1f)
        {
            // Xoay hướng input theo hướng camera
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDir = camForward * v + camRight * h;
            moveDir.Normalize();

            // Xoay nhân vật mượt hơn
            float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
            float smoothAngle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);

            // Thêm trọng lực
            if (controller.isGrounded)
            {
                verticalVelocity = -1f; // nhẹ để giữ dính mặt đất
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }

            // Di chuyển
            Vector3 finalMove = moveDir * moveSpeed + Vector3.up * verticalVelocity;
            controller.Move(finalMove * Time.deltaTime);
        }
        else
        {
            // Nếu không di chuyển: chỉ áp dụng trọng lực
            if (!controller.isGrounded)
            {
                verticalVelocity += gravity * Time.deltaTime;
                controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
            }
        }
    }
}

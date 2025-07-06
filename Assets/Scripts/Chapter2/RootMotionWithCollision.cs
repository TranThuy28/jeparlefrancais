using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class RootMotionWithCollision : MonoBehaviour
{
    private CharacterController controller;
    private Animator animator;
    private Vector3 inputDirection;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (Camera.main == null) return;
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
        
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        inputDirection = (camForward * v + camRight * h).normalized;
        //inputDirection = new Vector3(h, 0f, v).normalized;

        // Cập nhật animation blend
        animator.SetBool("isWalking", inputDirection.magnitude > 0.1f);
        //animator.SetBool("isRunning", inputDirection.magnitude > 0.1f);

        // Nếu có input, xoay hướng nhân vật theo input
        if (inputDirection.magnitude > 0.1f)
        {
            // Xoay hướng di chuyển tương ứng với input theo camera (nếu cần)
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5.0f);
        }
    }

   void OnAnimatorMove()
{
    if (animator == null || controller == null) return;

    if (inputDirection.magnitude > 0.01f)
    {
        // Xoay deltaPosition theo hướng nhân vật
        Vector3 rotatedDelta = transform.rotation * animator.deltaPosition;
        controller.Move(rotatedDelta);
    }
        else
        {
            // Khi không có input, chỉ giữ nhân vật dính đất
            Vector3 lockPos = transform.position;
            lockPos.x = Mathf.Round(lockPos.x * 1000f) / 1000f;
            lockPos.z = Mathf.Round(lockPos.z * 1000f) / 1000f;
            transform.position = lockPos; // Gravity nhỏ giữ nhân vật bám đất
        }
}

}

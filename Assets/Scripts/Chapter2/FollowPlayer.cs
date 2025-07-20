using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform cameraTarget;                     // NÊN gán vào GameObject nhân vật GỐC, không phải head/hip
    public Vector3 offset = new Vector3(0, 2f, -5);
    public float mouseSensitivity = 80f;
    public float rotationSmoothTime = 0.05f;

    private float yaw = 0f;
    private float pitch = 10f;
    private float pitchMin = -20f;
    private float pitchMax = 60f;
    private Vector3 currentVelocity;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        // Lấy input chuột để xoay camera
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        // Tính góc xoay thành quaternion
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        // Tính vị trí target của camera theo offset
        Vector3 targetPos = cameraTarget.position + Quaternion.Euler(pitch, yaw, 0) * offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, 0.05f);



        // Làm mượt vị trí
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, rotationSmoothTime);

        // Camera luôn nhìn về một điểm cố định (gốc nhân vật + một chút chiều cao)
        transform.LookAt(cameraTarget.position);
    }
}


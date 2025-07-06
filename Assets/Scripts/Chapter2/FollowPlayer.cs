using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0, 1.5f, -5);
    public float mouseSentivity = 80f;
    public float rotationSmoothTime = 0.1f;
    private float yaw = 0f;
    private float pitch = 2f;
    private float pitchMin = -10f;
    private float pitchMax = 40f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true; // ẩn con trỏ
    }

    void LateUpdate() // Sử dụng LateUpdate để đảm bảo player đã di chuyển xong trước khi camera cập nhật
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSentivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSentivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        // Cập nhật vị trí camera
        transform.position = player.position + rotation * offset;

        transform.LookAt(player.position + Vector3.up * 1.5f);

        // Xoay camera theo hướng nhìn của nhân vật
        // transform.rotation = Quaternion.Euler(0, player.eulerAngles.y, 0);
    }
}
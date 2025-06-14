using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;

    void LateUpdate() // Sử dụng LateUpdate để đảm bảo player đã di chuyển xong trước khi camera cập nhật
    {
        // Cập nhật vị trí camera
        transform.position = player.position + player.rotation * offset;

        // Xoay camera theo hướng nhìn của nhân vật
        transform.rotation = Quaternion.Euler(0, player.eulerAngles.y, 0);
    }
}

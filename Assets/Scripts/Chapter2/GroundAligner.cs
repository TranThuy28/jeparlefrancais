using UnityEngine;

public class GroundAligner : MonoBehaviour
{
    public float raycastDistance = 1.5f;
    public LayerMask groundLayer;

    void LateUpdate()
    {
        RaycastHit hit;

        // Bắn tia xuống từ chân nhân vật
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, raycastDistance, groundLayer))
        {
            Vector3 pos = transform.position;
            pos.y = hit.point.y; // Điều chỉnh Y theo mặt đất
            transform.position = pos;
        }
    }
}

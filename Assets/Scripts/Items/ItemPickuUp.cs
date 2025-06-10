using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Data")]
    public Item item;
    public int quantity = 1;
    
    [Header("Visual")]
    public float bobSpeed = 2f;
    public float bobHeight = 0.5f;
    
    private Vector3 startPosition;
    
    private void Start()
    {
        startPosition = transform.position;
    }
    
    private void Update()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
    
    public void OnItemCollected()
    {
        Destroy(gameObject);
    }
}
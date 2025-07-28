using UnityEngine;

public class InteractUIController : MonoBehaviour
{
    public static InteractUIController Instance;

    public GameObject interactUI;
    public Canvas worldCanvas;
    public Vector3 offset = Vector3.up * 2.5f;

    private Camera mainCam;
    private Transform target;

    void Awake()
    {
        Instance = this;
        mainCam = Camera.main;
        interactUI.SetActive(false);
    }

    void LateUpdate()
    {
        if (target != null)
        {
            worldCanvas.transform.position = target.position + offset;
            worldCanvas.transform.LookAt(mainCam.transform);
        }
    }

    public void ShowAt(Transform npcTransform)
    {
        target = npcTransform;
        interactUI.SetActive(true);
    }

    public void Hide()
    {
        target = null;
        interactUI.SetActive(false);
    }
}

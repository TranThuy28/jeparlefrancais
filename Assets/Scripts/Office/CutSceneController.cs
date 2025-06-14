using UnityEngine;

 // Start is called once before the first execution of Update after the MonoBehaviour is created
using UnityEngine;
using UnityEngine.Playables;

public class CutSceneController : MonoBehaviour
{
    public CanvasGroup group;

    public void Show()
    {
        Debug.Log("SHOW CALLED");
        group.alpha = 1;
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    public void Hide()
    {
        Debug.Log("HIDE CALLED");
        group.alpha = 0;
        group.interactable = false;
        group.blocksRaycasts = false;
    }
}

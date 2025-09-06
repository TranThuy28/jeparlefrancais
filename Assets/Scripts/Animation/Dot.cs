using UnityEngine;
using DG.Tweening;

public class DOTweenCutscene : MonoBehaviour
{
    public Camera mainCamera;
    public Transform cutscenePosition;
    public float duration = 2f;
    
    private Vector3 originalPos;
    private Quaternion originalRot;
    
    void Start()
    {
        originalPos = mainCamera.transform.position;
        originalRot = mainCamera.transform.rotation;
    }
    
    public void StartCutscene()
    {
        Sequence cutscene = DOTween.Sequence();
        
        // Di chuyển đến vị trí cutscene
        cutscene.Append(mainCamera.transform.DOMove(cutscenePosition.position, duration));
        cutscene.Join(mainCamera.transform.DORotateQuaternion(cutscenePosition.rotation, duration));
        
        // Dừng lại
        cutscene.AppendInterval(3f);
        
        // Quay về
        cutscene.Append(mainCamera.transform.DOMove(originalPos, duration));
        cutscene.Join(mainCamera.transform.DORotateQuaternion(originalRot, duration));
        
        // Callback khi kết thúc
        cutscene.OnComplete(() => {
            Debug.Log("Cutscene finished!");
        });
    }
}
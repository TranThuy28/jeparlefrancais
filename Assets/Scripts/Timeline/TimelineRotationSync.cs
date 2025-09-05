using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections;

// Fix Timeline không sync với hierarchy rotation
public class TimelineRotationSync : MonoBehaviour
{
    [Header("Timeline References")]
    public PlayableDirector timelineDirector;
    public GameObject targetCharacter;
    
    [Header("Rotation Control")]
    [Tooltip("Force Timeline to use hierarchy rotation")]
    public bool syncRotationWithHierarchy = true;
    
    [Tooltip("Set specific rotation for cutscene")]
    public bool useCustomRotation = false;
    public Vector3 customRotationEuler = Vector3.zero;
    
    private Quaternion hierarchyRotation;
    private Quaternion originalTimelineRotation;
    
    void Start()
    {
        if (timelineDirector)
        {
            timelineDirector.played += OnTimelineStarted;
            timelineDirector.stopped += OnTimelineEnded;
        }
    }
    
    void OnTimelineStarted(PlayableDirector director)
    {
        if (targetCharacter && syncRotationWithHierarchy)
        {
            // Lưu rotation hiện tại từ hierarchy
            hierarchyRotation = targetCharacter.transform.rotation;
            Debug.Log($"Saved hierarchy rotation: {hierarchyRotation.eulerAngles}");
            
            // Force sync Timeline với hierarchy rotation
            StartCoroutine(SyncRotationWithTimeline());
        }
    }
    
    IEnumerator SyncRotationWithTimeline()
    {
        // Chờ 1 frame để Timeline khởi tạo
        yield return null;
        
        // Force rotation theo hierarchy hoặc custom
        Quaternion targetRotation = useCustomRotation ? 
            Quaternion.Euler(customRotationEuler) : 
            hierarchyRotation;
            
        Debug.Log($"Setting Timeline rotation to: {targetRotation.eulerAngles}");
        
        // Force rotation trong suốt Timeline
        while (timelineDirector.state == PlayState.Playing)
        {
            if (targetCharacter)
            {
                targetCharacter.transform.rotation = targetRotation;
            }
            yield return null;
        }
    }
    
    void OnTimelineEnded(PlayableDirector director)
    {
        if (targetCharacter)
        {
            // Giữ rotation cuối cùng
            Quaternion finalRotation = useCustomRotation ? 
                Quaternion.Euler(customRotationEuler) : 
                hierarchyRotation;
                
            targetCharacter.transform.rotation = finalRotation;
            Debug.Log($"Timeline ended, final rotation: {finalRotation.eulerAngles}");
        }
    }
    
    // Method để set rotation từ bên ngoài
    public void SetCustomRotation(Vector3 eulerAngles)
    {
        customRotationEuler = eulerAngles;
        useCustomRotation = true;
    }
    
    public void SetCustomRotation(float yRotation)
    {
        customRotationEuler = new Vector3(0, yRotation, 0);
        useCustomRotation = true;
    }
    
    // Sync ngay lập tức (không chờ Timeline)
    public void ForceSyncRotation()
    {
        if (targetCharacter)
        {
            Quaternion targetRotation = useCustomRotation ? 
                Quaternion.Euler(customRotationEuler) : 
                hierarchyRotation;
                
            targetCharacter.transform.rotation = targetRotation;
        }
    }
}
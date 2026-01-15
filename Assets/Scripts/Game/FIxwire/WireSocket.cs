// using UnityEngine;
// using UnityEngine.EventSystems;

// public class WireSocket : MonoBehaviour, IPointerDownHandler
// {
//     public Wire wire;

//     public void OnPointerDown(PointerEventData eventData)
//     {
//         wire.StartDrag(this);
//     }
//     public void OnPointerUp(PointerEventData eventData)
//     {
//         wire.StopDrag(GetComponent<WireSocket>());
//     }
// }
// using UnityEngine;
// using UnityEngine.EventSystems;

// public class Wire : MonoBehaviour
// {
//     public LineRenderer line;
//     public WireSocket startSocket;
//     public WireSocket correctEndSocket;

//     private bool dragging;

//     void Update()
//     {
//         if (!dragging) return;

//         Vector3 mouseWorld =
//             Camera.main.ScreenToWorldPoint(Input.mousePosition);
//         mouseWorld.z = 0;

//         line.SetPosition(1, mouseWorld);
//     }

//     public void StartDrag(WireSocket socket)
//     {
//         startSocket = socket;
//         dragging = true;

//         line.positionCount = 2;
//         line.SetPosition(0, socket.transform.position);
//         line.SetPosition(1, socket.transform.position);
//     }

//     public void StopDrag(WireSocket endSocket)
//     {
//         dragging = false;

//         if (endSocket == correctEndSocket)
//         {
//             // Snap
//             line.SetPosition(1, endSocket.transform.position);
//         }
//         else
//         {
//             // Reset
//             line.SetPosition(1, startSocket.transform.position);
//         }
//     }
// }

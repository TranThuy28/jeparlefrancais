using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RopeManagement : MonoBehaviour
{
    // This class is currently empty and serves as a placeholder for future rope management logic.
    public List<RopeComponent> ropeComponents;
    public Button endButton;
    public Text Win;
    public static RopeManagement Instance { get; private set; }
    public Dictionary<RopeType, RopeState[]> winStates = new Dictionary<RopeType, RopeState[]>
    {
        { RopeType.Horizontal, new RopeState[] { RopeState.D90, RopeState.D270 } },
        { RopeType.Vertical, new RopeState[] { RopeState.D0, RopeState.D180 } },
        { RopeType.Corner, new RopeState[] { RopeState.D270 } },
        { RopeType.TShape, new RopeState[] { RopeState.D0 } },
        { RopeType.Cross, new RopeState[] { RopeState.D0, RopeState.D90, RopeState.D180, RopeState.D270 } },
    };
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        endButton.gameObject.SetActive(false);
        Win.gameObject.SetActive(false);
        endButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
        });
    }


    void Start()
    {
        System.Random rand = new System.Random();
        foreach (var rope in ropeComponents)
        {
            int randomAngleIndex = rand.Next(0, 4); // 0, 1, 2, 3
            RopeState randomState = (RopeState)randomAngleIndex;
            rope.transform.eulerAngles = new Vector3(0, 0, randomAngleIndex * 90);
            // Cập nhật trạng thái cho RopeComponent
            var stateField = rope.GetType().GetField("currentState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (stateField != null)
                stateField.SetValue(rope, randomState);
        }
        CheckWin();
    }

    public void CheckWin()
    {
        foreach (var rope in ropeComponents)
        {
            var stateField = rope.GetType().GetField("currentState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            RopeState state = RopeState.D0;
            if (stateField != null)
                state = (RopeState)stateField.GetValue(rope);
            bool found = false;
            foreach (var winState in winStates[rope.ropeType])
            {
                if (winState == state)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                return;
            }
        }
        Debug.Log("Win!");
        endButton.gameObject.SetActive(true);
        Win.gameObject.SetActive(true);
    }
}
public enum RopeState
{
    D0,
    D90,
    D180,
    D270
}

public enum RopeType
{
    Horizontal,
    Vertical,
    Corner,
    TShape,
    Cross
}
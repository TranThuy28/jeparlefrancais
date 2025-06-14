using UnityEngine;
using UnityEngine.UI;

public class SnowGameUI : MonoBehaviour
{
    [Header("Instructions")]
    public Text instructionText;
    
    void Start()
    {
        if (instructionText != null)
        {
            instructionText.text = "Use WASD or Arrow Keys to move the shovel!\nDrag with mouse to move!\nCollect snow before it melts!";
        }
    }
}
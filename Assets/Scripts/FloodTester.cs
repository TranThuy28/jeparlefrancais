using UnityEngine;

public class FloodTester : MonoBehaviour
{
    public FloodController flood;   // Kéo WaterPlane (có FloodController) vào đây
    public int levelToTest = 11;     // Level muốn test

    void Start()
    {
        if (flood != null)
        {
            flood.SetFloodLevel(levelToTest);
        }
    }
}
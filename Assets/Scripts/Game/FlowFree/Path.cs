// UIPath.cs
using UnityEngine;
using UnityEngine.UI;

public class UIPath : MonoBehaviour
{
    private Color pathColor;
    private int pairId;
    
    public Color PathColor => pathColor;
    public int PairId => pairId;
    
    public void Initialize(Color color, int pairId)
    {
        this.pathColor = color;
        this.pairId = pairId;
        
        // Set màu cho parent cell
        transform.parent.GetComponent<Image>().color = new Color(color.r, color.g, color.b, 0.7f);
    }
}
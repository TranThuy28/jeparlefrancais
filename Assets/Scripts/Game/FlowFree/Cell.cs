// 1. CellUI.cs - Quản lý từng ô trong grid
using UnityEngine;
using UnityEngine.UI;

public enum CellState
{
    Empty,
    Endpoint,
    Path
}

public class CellUI : MonoBehaviour
{
    [Header("Cell Properties")]
    public Image cellImage;
    public int row;
    public int col;
    public CellState cellState = CellState.Empty;
    public Color cellColor = Color.white;
    public int colorID = -1; // ID màu cho endpoint và đường đi

    [Header("Default Colors")]
    public Color emptyColor = Color.white;
    public Color pathColor = Color.gray;

    private void Awake()
    {
        if (cellImage == null)
            cellImage = GetComponent<Image>();
    }

    public void SetAsEmpty()
    {
        cellState = CellState.Empty;
        cellColor = emptyColor;
        colorID = -1;
        UpdateVisual();
    }

    public void SetAsEndpoint(Color color, int id)
    {
        cellState = CellState.Endpoint;
        cellColor = color;
        colorID = id;
        UpdateVisual();
    }

    public void SetAsPath(Color color, int id)
    {
        cellState = CellState.Path;
        cellColor = color;
        colorID = id;
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        if (cellImage != null)
        {
            cellImage.color = cellColor;
        }
    }

    public bool CanDrawPath(int newColorID)
    {
        // Có thể vẽ nếu ô trống hoặc là endpoint cùng màu
        return cellState == CellState.Empty || 
               (cellState == CellState.Endpoint && colorID == newColorID);
    }

    public bool IsEndpoint()
    {
        return cellState == CellState.Endpoint;
    }

    public bool IsEmpty()
    {
        return cellState == CellState.Empty;
    }
}
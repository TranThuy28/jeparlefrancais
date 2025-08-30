// 2. GridManagerUI.cs - Quản lý grid và tạo layout
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GridManagerUI : MonoBehaviour
{
    [Header("Grid Settings")]
    public int rows = 4;
    public int cols = 7;
    public GameObject cellPrefab;
    public GridLayoutGroup gridLayout;
    
    [Header("Endpoint Colors")]
    public Color[] endpointColors = { Color.red, Color.blue, Color.green, Color.yellow, Color.magenta };
    
    public CellUI[,] cells;
    
    private void Start()
    {
        CreateGrid();
        SetupEndpoints();
    }

private void CreateGrid()
{
    cells = new CellUI[rows, cols];
    
    if (gridLayout == null)
        gridLayout = GetComponent<GridLayoutGroup>();
    
//    gridLayout.GetComponent<Image>().raycastTarget = false; // Đảm bảo GridLayout không chặn raycast
        
    gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
    gridLayout.constraintCount = cols;
    
    for (int row = 0; row < rows; row++)
    {
        for (int col = 0; col < cols; col++)
        {
            GameObject cellObj = Instantiate(cellPrefab, transform);
            cellObj.transform.SetParent(gridLayout.transform, false);
            
            CellUI cell = cellObj.GetComponent<CellUI>();
            if (cell == null)
                cell = cellObj.AddComponent<CellUI>();
            
            cell.row = row;
            cell.col = col;
            cell.SetAsEmpty();
            
            cells[row, col] = cell;
        }
    }
    
    // Log position sau khi layout hoàn thành
    StartCoroutine(LogPositionsAfterLayout());
}

    private IEnumerator LogPositionsAfterLayout()
    {
        yield return new WaitForEndOfFrame();

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                RectTransform rectTransform = cells[row, col].GetComponent<RectTransform>();
            }
        }
    }

private void SetupEndpoints()
{
    // Đỏ - cùng hàng
    cells[0, 0].SetAsEndpoint(endpointColors[0], 0);
    cells[2, 4].SetAsEndpoint(endpointColors[0], 0);

    // Xanh dương - cùng cột
    cells[0, 1].SetAsEndpoint(endpointColors[1], 1);
    cells[3, 4].SetAsEndpoint(endpointColors[1], 1);

    // Xanh lá - diagonal easy
    cells[4, 1].SetAsEndpoint(endpointColors[2], 2);
    cells[4, 4].SetAsEndpoint(endpointColors[2], 2);

    // Vàng - corner to center
    cells[0, 5].SetAsEndpoint(endpointColors[3], 3);
    cells[2, 2].SetAsEndpoint(endpointColors[3], 3);

    // Tím - edge pattern
    cells[2, 3].SetAsEndpoint(endpointColors[4], 4);
    cells[1, 5].SetAsEndpoint(endpointColors[4], 4);
}

    public CellUI GetCell(int row, int col)
    {
        if (row >= 0 && row < rows && col >= 0 && col < cols)
            return cells[row, col];
        return null;
    }

    public bool IsValidPosition(int row, int col)
    {
        return row >= 0 && row < rows && col >= 0 && col < cols;
    }

    public CellUI[] GetEndpointsOfColor(int colorID)
    {
        var endpoints = new System.Collections.Generic.List<CellUI>();
        
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (cells[row, col].IsEndpoint() && cells[row, col].colorID == colorID)
                {
                    endpoints.Add(cells[row, col]);
                }
            }
        }
        
        return endpoints.ToArray();
    }
}
// 3. PipeDrawerUI.cs - Fixed version
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class PipeDrawerUI : MonoBehaviour
{
    [Header("References")]
    public GridManagerUI gridManager;
    public GraphicRaycaster graphicRaycaster; // Kéo Canvas vào đây trong Inspector
    
    private bool isDrawing = false;
    private int currentColorID = -1;
    private CellUI startCell = null;
    private List<CellUI> currentPath = new List<CellUI>();

    private void Start()
    {
        // Tự động tìm GraphicRaycaster nếu chưa được gán
        if (graphicRaycaster == null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
            }
        }
}
    
    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartDrawing();
        }
        else if (Input.GetMouseButton(0) && isDrawing)
        {
            ContinueDrawing();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            FinishDrawing();
        }
    }

    private void StartDrawing()
    {
        CellUI cell = GetCellUnderMouse();
        if (cell != null && cell.IsEndpoint())
        {
            isDrawing = true;
            currentColorID = cell.colorID;
            startCell = cell;
            currentPath.Clear();
            currentPath.Add(cell);

            // Xóa đường cũ của màu này
            ClearPathsOfColor(currentColorID);
        }
    }

    private void ContinueDrawing()
    {
        CellUI cell = GetCellUnderMouse();
        if (cell != null && cell != currentPath[currentPath.Count - 1])
        {
            // Kiểm tra xem có thể vẽ không
            if (CanDrawToCell(cell))
            {
                // Kiểm tra xem có phải đang quay lại không
                if (currentPath.Count > 1 && cell == currentPath[currentPath.Count - 2])
                {
                    // Quay lại - xóa cell cuối
                    CellUI lastCell = currentPath[currentPath.Count - 1];
                    if (!lastCell.IsEndpoint())
                    {
                        lastCell.SetAsEmpty();
                    }
                    currentPath.RemoveAt(currentPath.Count - 1);
                }
                else if (IsAdjacent(currentPath[currentPath.Count - 1], cell))
                {
                    // Vẽ tiếp
                    currentPath.Add(cell);
                    if (!cell.IsEndpoint())
                    {
                        cell.SetAsPath(gridManager.endpointColors[currentColorID], currentColorID);
                    }
                }
            }
        }
    }

    private void FinishDrawing()
    {
        if (isDrawing)
        {
            // Kiểm tra xem đường có hoàn thành không (kết thúc tại endpoint cùng màu)
            if (!IsValidPath())
            {
                // Xóa đường không hoàn thành
                ClearCurrentPath();
            }
        }
        
        isDrawing = false;
        currentColorID = -1;
        startCell = null;
        currentPath.Clear();
    }

    private bool IsValidPath()
    {
        if (currentPath.Count < 2) return false;
        
        CellUI endCell = currentPath[currentPath.Count - 1];
        return endCell.IsEndpoint() && endCell.colorID == currentColorID && endCell != startCell;
    }

    private void ClearCurrentPath()
    {
        for (int i = 0; i < currentPath.Count; i++)
        {
            if (!currentPath[i].IsEndpoint())
            {
                currentPath[i].SetAsEmpty();
            }
        }
    }

    private bool CanDrawToCell(CellUI cell)
    {
        return cell != null && cell.CanDrawPath(currentColorID);
    }

    private bool IsAdjacent(CellUI cell1, CellUI cell2)
    {
        if (cell1 == null || cell2 == null) return false;
        
        int rowDiff = Mathf.Abs(cell1.row - cell2.row);
        int colDiff = Mathf.Abs(cell1.col - cell2.col);
        
        return (rowDiff == 1 && colDiff == 0) || (rowDiff == 0 && colDiff == 1);
    }

    private CellUI GetCellUnderMouse()
    {
        // Kiểm tra null trước khi sử dụng
        if (EventSystem.current == null || graphicRaycaster == null)
        {
            Debug.LogWarning("EventSystem or GraphicRaycaster is null!");
            return null;
        }
        
        Vector2 mousePos = Input.mousePosition;
        
        // Sử dụng GraphicRaycaster để tìm UI element
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = mousePos;
        
        List<RaycastResult> results = new List<RaycastResult>();

        graphicRaycaster.Raycast(pointerData, results);
        
        foreach (RaycastResult result in results)
        {
            CellUI cell = result.gameObject.GetComponent<CellUI>();
            if (cell != null)
            {
                return cell;
            }
        }
        
        return null;
    }

    private void ClearPathsOfColor(int colorID)
    {
        if (gridManager == null || gridManager.cells == null) return;
        
        for (int row = 0; row < gridManager.rows; row++)
        {
            for (int col = 0; col < gridManager.cols; col++)
            {
                CellUI cell = gridManager.cells[row, col];
                if (cell != null && cell.cellState == CellState.Path && cell.colorID == colorID)
                {
                    cell.SetAsEmpty();
                }
            }
        }
    }
}
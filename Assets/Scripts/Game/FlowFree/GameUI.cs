// 4. GameManagerUI.cs - Quản lý trạng thái game
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameManagerUI : MonoBehaviour
{
    [Header("References")]
    public GridManagerUI gridManager;
    public Text winText;
    
    [Header("Game Settings")]
    public int totalColors = 4; // Số màu cần nối
    
    private void Start()
    {
        if (winText != null)
            winText.gameObject.SetActive(false);
    }

    private void Update()
    {
        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        if (IsGameWon())
        {
            ShowWinMessage();
        }
    }

    private bool IsGameWon()
    {
        // Kiểm tra tất cả endpoint có được nối không
        for (int colorID = 0; colorID < totalColors; colorID++)
        {
            if (!IsColorCompleted(colorID))
            {  
                return false;
            }
        }
        
        // Kiểm tra tất cả ô có được lấp đầy không
        return IsGridFull();
    }

    private bool IsColorCompleted(int colorID)
    {
        CellUI[] endpoints = gridManager.GetEndpointsOfColor(colorID);
        if (endpoints.Length != 2) return false;
        // Tìm đường đi từ endpoint này đến endpoint kia
        return HasValidPath(endpoints[0], endpoints[1], colorID);
    }

    private bool HasValidPath(CellUI start, CellUI end, int colorID)
    {
        HashSet<CellUI> visited = new HashSet<CellUI>();
        Queue<CellUI> queue = new Queue<CellUI>();
        
        queue.Enqueue(start);
        visited.Add(start);
        
        while (queue.Count > 0)
        {
            CellUI current = queue.Dequeue();
            
            if (current == end)
                return true;
            
            // Kiểm tra các ô liền kề
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (Mathf.Abs(dr) + Mathf.Abs(dc) != 1) continue; // Chỉ kiểm tra 4 hướng
                    
                    int newRow = current.row + dr;
                    int newCol = current.col + dc;
                    
                    if (gridManager.IsValidPosition(newRow, newCol))
                    {
                        CellUI neighbor = gridManager.GetCell(newRow, newCol);
                        
                        if (!visited.Contains(neighbor) && 
                            (neighbor.colorID == colorID || neighbor == end))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }
        }
        
        return false;
    }

    private bool IsGridFull()
    {
        for (int row = 0; row < gridManager.rows; row++)
        {
            for (int col = 0; col < gridManager.cols; col++)
            {
                if (gridManager.cells[row, col].cellState == CellState.Empty)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void ShowWinMessage()
    {
        if (winText != null && !winText.gameObject.activeInHierarchy)
        {
            winText.gameObject.SetActive(true);
            winText.text = "YOU WIN!\nCongratulations!";
        }
    }

    public void RestartGame()
    {
        // Reset tất cả cells về trạng thái ban đầu
        for (int row = 0; row < gridManager.rows; row++)
        {
            for (int col = 0; col < gridManager.cols; col++)
            {
                CellUI cell = gridManager.cells[row, col];
                if (cell.cellState == CellState.Path)
                {
                    cell.SetAsEmpty();
                }
            }
        }
        
        if (winText != null)
            winText.gameObject.SetActive(false);
    }
}
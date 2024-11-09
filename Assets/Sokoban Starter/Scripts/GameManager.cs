using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Dictionary to store grid positions and the objects in each cell
    private Dictionary<Vector2Int, GameObject> grid = new Dictionary<Vector2Int, GameObject>();
    public float moveDelay = 0.2f; // Delay to control movement speed

    public GameObject player;
    private Vector2Int playerPosition;

    private GridObject gridObject;
    private void Start()
    {
        gridObject = GetComponent<GridObject>();
        InitializeGrid();

    }

    private void InitializeGrid()
    {
        // Initialize grid with all block positions
        foreach (GridObject obj in FindObjectsOfType<GridObject>())
        {
            Vector2Int pos = obj.gridPosition;
            if (!grid.ContainsKey(pos))
            {
                grid[pos] = obj.gameObject;
            }

            if (obj.gameObject.CompareTag("Player"))
            {
                player = obj.gameObject;
                playerPosition = pos;
            }
        }
    }

    private void Update()
    {
        HandlePlayerInput();
    }

    private void HandlePlayerInput()
    {
        // Get dimensions
        int maxX = (int)GridMaker.reference.dimensions.x;
        int maxY = (int)GridMaker.reference.dimensions.y;



        if (Input.GetKeyDown(KeyCode.S))
        {
            gridObject.gridPosition.y += 1;
            if (gridObject.gridPosition.y > maxY)
                gridObject.gridPosition.y = maxY;  // Bottom
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            gridObject.gridPosition.y -= 1;
            if (gridObject.gridPosition.y < 1)
                gridObject.gridPosition.y = 1;  // UP
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            gridObject.gridPosition.x -= 1;
            if (gridObject.gridPosition.x < 1)
                gridObject.gridPosition.x = 1;  // Left
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            gridObject.gridPosition.x += 1;
            if (gridObject.gridPosition.x > maxX)
                gridObject.gridPosition.x = maxX;  // Right
        }
    }

     private void TryMove(Vector2Int direction)
    {
        Vector2Int targetPos = playerPosition + direction;
        if (IsWithinGridBounds(targetPos) && CanMoveTo(targetPos))
        {
            MoveBlock(playerPosition, targetPos, direction, player);
        }
    }

    private bool CanMoveTo(Vector2Int targetPos)
    {
        if (grid.ContainsKey(targetPos))
        {
            GameObject targetBlock = grid[targetPos];
            if (targetBlock.CompareTag("Wall"))
            {
                return false; // Walls block all movement
            }
            else if (targetBlock.CompareTag("Smooth"))
            {
                // Push Smooth block
                Vector2Int nextPos = targetPos + (targetPos - playerPosition);
                if (IsWithinGridBounds(nextPos) && !grid.ContainsKey(nextPos))
                {
                    MoveBlock(targetPos, nextPos, targetPos - playerPosition, targetBlock);
                    return true;
                }
                return false;
            }
            else if (targetBlock.CompareTag("Clingy"))
            {
                return false; // Clingy can only be pulled, not pushed
            }
            else if (targetBlock.CompareTag("Sticky"))
            {
                Vector2Int nextPos = targetPos + (targetPos - playerPosition);
                if (IsWithinGridBounds(nextPos) && !grid.ContainsKey(nextPos))
                {
                    MoveBlock(targetPos, nextPos, targetPos - playerPosition, targetBlock);
                    return true;
                }
                return false;
            }
        }
        return true;
    }

    private void MoveBlock(Vector2Int startPos, Vector2Int targetPos, Vector2Int direction, GameObject mover)
    {
        if (!grid.ContainsKey(targetPos))
        {
            grid.Remove(startPos);
            grid[targetPos] = mover;

            GridObject gridObj = mover.GetComponent<GridObject>();
            gridObj.gridPosition = targetPos;

            if (mover.CompareTag("Player"))
            {
                playerPosition = targetPos;
            }

            HandleSpecialBlocks(startPos, targetPos, direction);
        }
    }

    private void HandleSpecialBlocks(Vector2Int startPos, Vector2Int targetPos, Vector2Int direction)
    {
        GameObject block = grid[targetPos];
        
        if (block.CompareTag("Sticky"))
        {
            MoveStickyBlock(targetPos, direction);
        }
        else if (block.CompareTag("Clingy"))
        {
            PullClingyBlock(startPos, direction);
        }
    }

    private void MoveStickyBlock(Vector2Int stickyPos, Vector2Int direction)
    {
        Vector2Int nextPos = stickyPos + direction;
        if (IsWithinGridBounds(nextPos) && !grid.ContainsKey(nextPos))
        {
            GameObject sticky = grid[stickyPos];
            grid.Remove(stickyPos);
            grid[nextPos] = sticky;
            sticky.GetComponent<GridObject>().gridPosition = nextPos;
        }
    }

    private void PullClingyBlock(Vector2Int clingyPos, Vector2Int direction)
    {
        Vector2Int playerPos = clingyPos - direction;
        if (grid.ContainsKey(playerPos) && grid[playerPos].CompareTag("Player"))
        {
            grid.Remove(clingyPos);
            Vector2Int newClingyPos = clingyPos - direction;
            grid[newClingyPos] = grid[clingyPos];
            grid[clingyPos].GetComponent<GridObject>().gridPosition = newClingyPos;
        }
    }

    private bool IsWithinGridBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 &&
               pos.x < (int)GridMaker.reference.dimensions.x &&
               pos.y < (int)GridMaker.reference.dimensions.y;
    }
}
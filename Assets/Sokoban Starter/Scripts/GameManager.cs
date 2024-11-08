using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Dictionary to store grid positions and the objects in each cell
    private Dictionary<Vector2Int, GameObject> grid = new Dictionary<Vector2Int, GameObject>();
    public float moveDelay = 0.2f; // Delay to control movement speed

    public GameObject player;
    private Vector2Int playerPosition;

    private void Start()
    {
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
        Vector2Int direction = Vector2Int.zero;

        // Get WASD input and set direction
        if (Input.GetKeyDown(KeyCode.W)) direction = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.S)) direction = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.A)) direction = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D)) direction = Vector2Int.right;

        if (direction != Vector2Int.zero)
        {
            Vector2Int targetPos = playerPosition + direction;
            MoveBlock(playerPosition, targetPos, direction, player);
        }
    }

    private void MoveBlock(Vector2Int startPos, Vector2Int targetPos, Vector2Int direction, GameObject mover)
    {
        if (!grid.ContainsKey(targetPos) && IsWithinGridBounds(targetPos))
        {
            grid.Remove(startPos);
            grid[targetPos] = mover;

            // Update position in the GridObject component
            GridObject gridObj = mover.GetComponent<GridObject>();
            gridObj.gridPosition = targetPos;

            if (mover.CompareTag("Player"))
            {
                playerPosition = targetPos;
            }

            // Handle special cases for Sticky and Clingy blocks
            HandleSpecialBlocks(targetPos, direction);
        }
    }

    private void HandleSpecialBlocks(Vector2Int currentPos, Vector2Int direction)
    {
        foreach (var entry in grid)
        {
            GameObject obj = entry.Value;
            if (obj.CompareTag("Sticky"))
            {
                MoveStickyBlock(entry.Key, direction);
            }
            else if (obj.CompareTag("Clingy"))
            {
                PullClingyBlock(entry.Key, direction);
            }
        }
    }

    private void MoveStickyBlock(Vector2Int stickyPos, Vector2Int direction)
    {
        Vector2Int targetPos = stickyPos + direction;
        if (!grid.ContainsKey(targetPos) && IsWithinGridBounds(targetPos))
        {
            GameObject sticky = grid[stickyPos];
            grid.Remove(stickyPos);
            grid[targetPos] = sticky;
            sticky.GetComponent<GridObject>().gridPosition = targetPos;
        }
    }

    private void PullClingyBlock(Vector2Int clingyPos, Vector2Int direction)
    {
        Vector2Int targetPos = clingyPos - direction;
        if (grid.ContainsKey(targetPos) && grid[targetPos].CompareTag("Player"))
        {
            grid.Remove(clingyPos);
            grid[clingyPos - direction] = grid[clingyPos];
            grid[clingyPos].GetComponent<GridObject>().gridPosition = clingyPos - direction;
        }
    }

    private bool IsWithinGridBounds(Vector2Int pos)
    {
        // Check if position is within grid boundaries based on grid dimensions
        return pos.x >= 0 && pos.y >= 0 &&
               pos.x < (int)GridMaker.reference.dimensions.x &&
               pos.y < (int)GridMaker.reference.dimensions.y;
    }
}

using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GridObject player;  // Assign Player in Inspector
    public List<GridObject> walls, smoothBlocks, stickyBlocks, clingyBlocks;  // Assign blocks in Inspector

    private Dictionary<Vector2Int, GridObject> gridMap = new Dictionary<Vector2Int, GridObject>();

    void Start()
    {
        InitializeGridMap();
    }

    void Update()
    {
        HandlePlayerInput();
    }

    private void InitializeGridMap()
    {
        foreach (var wall in walls) gridMap[wall.gridPosition] = wall;
        foreach (var smooth in smoothBlocks) gridMap[smooth.gridPosition] = smooth;
        foreach (var sticky in stickyBlocks) gridMap[sticky.gridPosition] = sticky;
        foreach (var clingy in clingyBlocks) gridMap[clingy.gridPosition] = clingy;
        gridMap[player.gridPosition] = player;
    }

    private void HandlePlayerInput()
    {
        Vector2Int direction = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.W)) direction = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.S)) direction = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.A)) direction = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D)) direction = Vector2Int.right;

        if (direction != Vector2Int.zero)
            MoveBlock(player, direction);
    }

    private void MoveBlock(GridObject block, Vector2Int direction)
    {
        Vector2Int targetPos = block.gridPosition + direction;

        // Prevent moving beyond grid bounds
        if (!IsWithinBounds(targetPos))
            return;

        // Check if target position is occupied
        if (gridMap.TryGetValue(targetPos, out GridObject otherBlock))
        {
            // Handle interactions based on block types
            if (otherBlock == player) return;
            else if (walls.Contains(otherBlock)) return;  // Wall is immovable
            else if (smoothBlocks.Contains(otherBlock) || stickyBlocks.Contains(otherBlock))
            {
                MoveBlock(otherBlock, direction);  // Push Smooth or Sticky blocks
                if (gridMap.ContainsKey(targetPos)) return;  // Blocked, no movement
            }
            else if (clingyBlocks.Contains(otherBlock))
            {
                // Only move Clingy block if pulled
                if (block == player)
                    PullBlock(otherBlock, direction);
            }
        }

        // Update positions if not blocked
        UpdatePosition(block, targetPos);

        // Handle sticky block chaining
        if (stickyBlocks.Contains(block))
            MoveStickyAdjacentBlocks(block, direction);
    }

    private void PullBlock(GridObject clingy, Vector2Int direction)
    {
        Vector2Int pullPos = clingy.gridPosition - direction;
        if (gridMap.TryGetValue(pullPos, out GridObject puller) && puller == player)
            UpdatePosition(clingy, clingy.gridPosition + direction);
    }

    private void MoveStickyAdjacentBlocks(GridObject sticky, Vector2Int direction)
    {
        foreach (var stickyNeighbor in stickyBlocks)
        {
            Vector2Int neighborPos = stickyNeighbor.gridPosition - direction;
            if (neighborPos == sticky.gridPosition)
                MoveBlock(stickyNeighbor, direction);
        }
    }

    private bool IsWithinBounds(Vector2Int position)
    {
        return position.x >= 0 && position.x < GridMaker.reference.dimensions.x &&
               position.y >= 0 && position.y < GridMaker.reference.dimensions.y;
    }

    private void UpdatePosition(GridObject block, Vector2Int newPosition)
    {
        gridMap.Remove(block.gridPosition);
        block.gridPosition = newPosition;
        gridMap[newPosition] = block;
    }
}

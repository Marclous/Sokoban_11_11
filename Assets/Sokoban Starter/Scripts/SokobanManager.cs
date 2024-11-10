using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

public class SokobanManager : MonoBehaviour
{
    public GridObject player;
    private Vector2Int lastGridPosition;
    private Vector2Int lastDirection;
    private Dictionary<Vector2Int, GridObject> gridObjects = new Dictionary<Vector2Int, GridObject>();
    public List<GridObject> walls;
    public List<GridObject> smoothBlocks;
    public List<GridObject> stickyBlocks;
    public List<GridObject> clingyBlocks;

    private void Awake()
    {
        // Initialize grid objects dictionary by finding all GridObjects in the scene
        RegisterAllGridObjects();
        if (player != null)
        {
            lastGridPosition = player.gridPosition;
        }
    }
    private void Update()
    {
        if (player == null) return;

        // Calculate the player's movement direction based on their grid position change
        Vector2Int currentGridPosition = player.gridPosition;
        lastDirection = currentGridPosition - lastGridPosition;

        // Only proceed if the player has moved to a new grid position
        if (lastDirection != Vector2Int.zero)
        {
            // Check adjacent block in the direction of movement
            //CheckAdjacentBlockInDirection(lastDirection);

            // Update last position to the current position
            //lastGridPosition = currentGridPosition;
        }
        HandlePlayerMovement();
        HandleStickyBehavior();
    }
    private void RegisterAllGridObjects()
    {
        // Find all GridObject instances in the scene
        GridObject[] objects = FindObjectsOfType<GridObject>();

        // Register each GridObject with its grid position
        foreach (var obj in objects)
        {
            gridObjects[obj.gridPosition] = obj;
        }
    }


    void HandlePlayerMovement()
    {
        // Start with the current player position and a variable for the intended target position
        Vector2Int direction = Vector2Int.zero;

        // Determine the direction based on input without directly modifying targetPosition
        if (Input.GetKeyDown(KeyCode.W))
            direction = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.S))
            direction = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.A))
            direction = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D))
            direction = Vector2Int.right;
        // If no movement key is pressed, return early
        if (direction == Vector2Int.zero)
            return;

        // Calculate the target position based on the direction
        Vector2Int targetPosition = player.gridPosition + direction;
        Vector2Int backPosition = player.gridPosition - direction;
        Debug.Log("targetposition" + targetPosition);
        Debug.Log("backposition" + backPosition);
        GridObject adjacentBlock = GetBlockAtPosition(targetPosition);
        GridObject pullBlock = GetBlockAtPosition(backPosition);

        // Check if the target position is within the grid and not occupied by a wall
        if (IsPositionValid(targetPosition) && IsPositionEmpty(targetPosition, walls) && IsPositionEmpty(targetPosition, clingyBlocks))
        {

            if (adjacentBlock != null || pullBlock != null)
            {
                Vector2Int pushPosition = targetPosition + direction;

                if (smoothBlocks.Contains(adjacentBlock) && TryPushBlock(adjacentBlock, direction))
                {
                    MoveBlock(player, targetPosition);
                }
                else if (clingyBlocks.Contains(pullBlock))
                {
                    // If pulling, the Clingy block should move to the player’s previous position
                    MoveBlock(player, targetPosition);
                    MoveBlock(pullBlock, player.gridPosition - direction);

                }
                else if ( (smoothBlocks.Contains(pullBlock) || walls.Contains(pullBlock) || stickyBlocks.Contains(pullBlock)) && IsPositionEmpty(targetPosition,clingyBlocks) && IsPositionEmpty(targetPosition, stickyBlocks))
                {
                    MoveBlock(player, targetPosition);
                }

            }
            else
            {
                MoveBlock(player, targetPosition);
            }
        }

    }

    private bool TryPushBlock(GridObject block, Vector2Int direction)
    {
        Vector2Int targetPosition = block.gridPosition + direction;

        // Check if the position is within bounds and empty or has another Smooth block
        if (!IsPositionValid(targetPosition) || !IsPositionEmpty(targetPosition, walls))
            return false;

        GridObject nextBlock = GetBlockAtPosition(targetPosition);

        if (nextBlock != null)
        {
            if (smoothBlocks.Contains(nextBlock))
            {
                // Recursively attempt to push the next Smooth block
                if (!TryPushBlock(nextBlock, direction))
                    return false;
            }
            else
            {
                // Block is immovable, so we can't push
                return false;
            }
        }

        // Move the current block to the target position if it's free or movable
        MoveBlock(block, targetPosition);
        return true;
    }

    private void MoveBlock(GridObject block, Vector2Int newPosition)
    {
        // Remove the old position from gridObjects
        gridObjects.Remove(block.gridPosition);

        // Update the block's grid position and re-add it to gridObjects
        block.gridPosition = newPosition;
        gridObjects[newPosition] = block;
    }

    private void HandleStickyBehavior()
    {
        foreach (GridObject sticky in stickyBlocks)
        {
            // Check each movement direction for adjacent blocks that could be moving
            foreach (Vector2Int direction in new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int adjacentPosition = sticky.gridPosition + direction;
                GridObject adjacentBlock = GetBlockAtPosition(adjacentPosition);

                if (adjacentBlock != null && (adjacentBlock == player || smoothBlocks.Contains(adjacentBlock)))
                {
                    // Calculate the target position in the direction of the adjacent block's movement
                    Vector2Int stickyTargetPosition = sticky.gridPosition + direction;

                    // Ensure Sticky block can move in that direction (i.e., the target position is valid and unoccupied)
                    if (IsPositionValid(stickyTargetPosition) && IsPositionEmpty(stickyTargetPosition) && stickyTargetPosition != player.gridPosition)
                    {
                        MoveBlock(sticky, stickyTargetPosition);
                    }
                }
            }
        }
    }



    bool IsPositionValid(Vector2Int position)
    {
        int maxX = (int)GridMaker.reference.dimensions.x;
        int maxY = (int)GridMaker.reference.dimensions.y;
        return position.x > 0 && position.x <= maxX && position.y > 0 && position.y <= maxY;
    }

    bool IsPositionEmpty(Vector2Int position, List<GridObject> specificList = null)
    {
        // Check specified list if provided
        if (specificList != null)
        {
            foreach (GridObject obj in specificList)
                if (obj.gridPosition == position)
                    return false;
        }
        return true;
    }


    GridObject GetBlockAtPosition(Vector2Int position)
    {
        // Check each list of blocks for a block at the specified position
        foreach (GridObject wall in walls)
            if (wall.gridPosition == position)
                return wall;

        foreach (GridObject smooth in smoothBlocks)
            if (smooth.gridPosition == position)
                return smooth;

        foreach (GridObject sticky in stickyBlocks)
            if (sticky.gridPosition == position)
                return sticky;

        foreach (GridObject clingy in clingyBlocks)
            if (clingy.gridPosition == position)
                return clingy;

        // Return null if no block is found at the position
        return null;
    }
    GridObject FindBlockAtPosition(Vector2Int position, List<GridObject> list1, List<GridObject> list2 = null, List<GridObject> list3 = null, List<GridObject> list4 = null)
    {
        foreach (GridObject block in list1)
            if (block.gridPosition == position)
                return block;
        if (list2 != null)
        {
            foreach (GridObject block in list2)
                if (block.gridPosition == position)
                    return block;
        }
        return null;
    }

    bool IsAdjacentTo(Vector2Int pos1, Vector2Int pos2)
    {

        return (pos1 - pos2).sqrMagnitude == 1;
    }
}

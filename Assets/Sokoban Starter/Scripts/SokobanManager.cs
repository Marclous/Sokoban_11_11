using System.Collections.Generic;
using JetBrains.Annotations;
using NaughtyAttributes.Test;
using UnityEngine;
using UnityEngine.UIElements;

public class SokobanManager : MonoBehaviour
{
    public GridObject player;
    private Vector2Int lastGridPosition;
    private Vector2Int lastDirection;
    private Dictionary<Vector2Int, GridObject> gridObjects = new Dictionary<Vector2Int, GridObject>();
    private Dictionary<GridObject, Vector2Int> previousPositions = new Dictionary<GridObject, Vector2Int>();
    public List<GridObject> walls;
    public List<GridObject> smoothBlocks;
    public List<GridObject> stickyBlocks;
    private bool stickingObject;
    public List<GridObject> clingyBlocks;
    private List<GridObject> connectedObjects = new List<GridObject>();
    public bool shouldMove = false;
    private List<GridObject> stickyBlocksToMove = new List<GridObject>();
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

        Vector2Int currentGridPosition = player.gridPosition;
        lastDirection = currentGridPosition - lastGridPosition;

        // Only proceed if the player has moved to a new grid position
        if (lastDirection != Vector2Int.zero)
        {
            // Update last position to the current position
            lastGridPosition = currentGridPosition;
        }


        HandlePlayerMovement();
        /*HandleStickyBehavior();
        if (shouldMove)
        {
            MoveSticky();
        }*/

    }


    /*private void HandleStickyBehavior()
    {

        List<GridObject> connectedObjects = new List<GridObject>();
        foreach (var stickyBlock in stickyBlocks)
        {

            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            // Check only cardinal directions: up, down, left, and right
            foreach (var dir in directions)
            {
                Vector2Int adjacentPos = stickyBlock.gridPosition + dir;
                GridObject adjacentBlock = GetBlockAtPosition(adjacentPos);

                // Check if adjacent block is moving in the cardinal direction
                if (adjacentBlock != null && (player == adjacentBlock || smoothBlocks.Contains(adjacentBlock) || clingyBlocks.Contains(adjacentBlock)))
                {
                    // Set shouldMove to true if a valid adjacent block is moving
                    Debug.Log(adjacentBlock.name + "is around");
                    previousPositions[adjacentBlock] = adjacentBlock.gridPosition;
                    connectedObjects.Add(adjacentBlock);
                    stickyBlocksToMove.Add(stickyBlock);
                    shouldMove = true;
                }
            }

            // If Sticky block should move, attempt to move it in the same direction

        }

        // Move Sticky blocks after determining which can move

    }

    private void MoveSticky()
    {
        Debug.Log("Sticky should move");
        
        
        foreach (var adjacent in connectedObjects)
            {
                Vector2Int currentPos = adjacent.gridPosition;
                Debug.Log(currentPos);
                if (currentPos != previousPositions[adjacent])
                {
                    Vector2Int targetPosition = sticky.gridPosition + lastDirection;
                    Debug.Log("Moving Sticky");
                    
                    MoveBlock(sticky, targetPosition);

                }
            }
        

        // Ensure target position is valid and empty (not occupied by wall or another block)
        /*if (IsPositionValid(targetPosition) && IsPositionEmpty(targetPosition, walls))
        {
            stickyBlocksToMove.Add(stickyBlock);
        }
    }*/

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
                else if ((smoothBlocks.Contains(pullBlock) || walls.Contains(pullBlock) || stickyBlocks.Contains(pullBlock)) && IsPositionEmpty(targetPosition, clingyBlocks) && IsPositionEmpty(targetPosition, stickyBlocks))
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
        if (!IsPositionValid(targetPosition) || !IsPositionEmpty(targetPosition, walls) || !IsPositionEmpty(targetPosition, clingyBlocks))
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


    public GridObject GetBlockAtPosition(Vector2Int position)
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

        if (player.gridPosition == position) return player;
        // Return null if no block is found at the position
        return null;
    }
    bool IsAdjacentTo(Vector2Int pos1, Vector2Int pos2)
    {

        return (pos1 - pos2).sqrMagnitude == 1;
    }
}

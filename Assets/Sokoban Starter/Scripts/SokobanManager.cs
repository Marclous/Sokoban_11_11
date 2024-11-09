using System.Collections.Generic;
using UnityEngine;

public class SokobanManager : MonoBehaviour
{
    public GridObject player;
    public List<GridObject> walls;
    public List<GridObject> smoothBlocks;
    public List<GridObject> stickyBlocks;
    public List<GridObject> clingyBlocks;

    private void Update()
    {
        HandlePlayerMovement();
        HandleStickyBehavior();
        HandleClingyBehavior();
    }

    void HandlePlayerMovement()
    {
        Vector2Int targetPosition = player.gridPosition;

        if (Input.GetKeyDown(KeyCode.W))
            targetPosition += Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.S))
            targetPosition += Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.A))
            targetPosition += Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D))
            targetPosition += Vector2Int.right;

        if (IsPositionValid(targetPosition) && IsPositionEmpty(targetPosition, walls))
        {
            // Check if there is a Smooth or Clingy block at the target position
            GridObject smoothOrClingy = FindBlockAtPosition(targetPosition, smoothBlocks, clingyBlocks);
            if (smoothOrClingy != null)
            {
                Vector2Int pushPosition = targetPosition + (targetPosition - player.gridPosition);
                if (IsPositionValid(pushPosition) && IsPositionEmpty(pushPosition))
                {
                    if (smoothBlocks.Contains(smoothOrClingy))
                    {
                        smoothOrClingy.gridPosition = pushPosition;
                    }
                    else if (clingyBlocks.Contains(smoothOrClingy))
                    {
                        player.gridPosition = targetPosition;
                    }
                }
            }
            else
            {
                player.gridPosition = targetPosition;
            }
        }
    }

    void HandleStickyBehavior()
    {
        foreach (GridObject sticky in stickyBlocks)
        {
            Vector2Int moveDirection = Vector2Int.zero;
            if (IsAdjacentTo(player.gridPosition, sticky.gridPosition))
                moveDirection = player.gridPosition - sticky.gridPosition;

            if (moveDirection != Vector2Int.zero)
            {
                Vector2Int newStickyPosition = sticky.gridPosition + moveDirection;
                if (IsPositionValid(newStickyPosition) && IsPositionEmpty(newStickyPosition))
                    sticky.gridPosition = newStickyPosition;
            }
        }
    }

    void HandleClingyBehavior()
    {
        foreach (GridObject clingy in clingyBlocks)
        {
            Vector2Int pullDirection = Vector2Int.zero;
            if (IsAdjacentTo(player.gridPosition, clingy.gridPosition))
                pullDirection = player.gridPosition - clingy.gridPosition;

            if (pullDirection != Vector2Int.zero)
            {
                Vector2Int newClingyPosition = clingy.gridPosition + pullDirection;
                if (IsPositionValid(newClingyPosition) && IsPositionEmpty(newClingyPosition))
                    clingy.gridPosition = newClingyPosition;
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
    
    // Check all blocks if no specific list is given
    foreach (GridObject wall in walls)
        if (wall.gridPosition == position)
            return false;
    foreach (GridObject block in smoothBlocks)
        if (block.gridPosition == position)
            return false;
    foreach (GridObject block in stickyBlocks)
        if (block.gridPosition == position)
            return false;
    foreach (GridObject block in clingyBlocks)
        if (block.gridPosition == position)
            return false;

    return position != player.gridPosition;
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

using System.Collections.Generic;
using UnityEngine;

public class StickyBlock : MonoBehaviour
{
    private GridObject stickyBlock;  // The Sticky block itself
    private Dictionary<GridObject, Vector2Int> connectedBlocks = new Dictionary<GridObject, Vector2Int>();
    private Dictionary<Vector2Int, GridObject> gridObjects = new Dictionary<Vector2Int, GridObject>();
    private Vector2Int[] cardinalDirections = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
    List<GridObject> connected;
    bool isSticking;

    void Start()
    {
        stickyBlock = GetComponent<GridObject>();
        RegisterAllGridObjects();
        UpdateConnectedBlocks();
    }

    void Update()
    {
        // Continuously check and update the positions of connected blocks
        UpdateConnectedBlocks();

        // Move Sticky block in the direction of any connected block's movement
        if(isSticking){
            FollowConnectedBlockMovement();
        }
        
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
    private void UpdateConnectedBlocks()
    {
        // Clear the dictionary of connected blocks and re-check adjacent positions
        connectedBlocks.Clear();

        foreach (var direction in cardinalDirections)
        {
            Vector2Int adjacentPos = stickyBlock.gridPosition + direction;
            GridObject adjacentBlock = GetBlockAtPosition(adjacentPos);

            // Only track the player, Smooth blocks, or Clingy blocks as connected
            if (adjacentBlock != null && (adjacentBlock.tag == "Player" || adjacentBlock.tag == "SmoothBlock" || adjacentBlock.tag == "ClingyBlock"))
            {
                Debug.Log(adjacentBlock.name+" is around");
                // Store the adjacent block and its current position
                connectedBlocks[adjacentBlock] = adjacentBlock.gridPosition;
                connected.Add(adjacentBlock);
                isSticking = true;
            }
        }
    }

    private void FollowConnectedBlockMovement()
    {
        foreach (GridObject connect in connected) {
            Vector2Int currentPos = connect.gridPosition;

            if(currentPos != connectedBlocks[connect]) {
                Vector2Int moveDir = currentPos - connectedBlocks[connect];
                if(IsPositionValid(moveDir) && IsPositionEmpty(moveDir)) {
                    connectedBlocks[connect] = currentPos;
                }
                else{
                    Disconnect();
                }
            }
        }
    }

    private void Disconnect(){
        connected = null;
        isSticking = false;
    }

    private bool IsPositionValid(Vector2Int position)
    {
        int maxX = (int)GridMaker.reference.dimensions.x;
        int maxY = (int)GridMaker.reference.dimensions.y;
        return position.x >= 0 && position.x < maxX && position.y >= 0 && position.y < maxY;
    }

    private bool IsPositionEmpty(Vector2Int position)
    {
        return GetBlockAtPosition(position) == null;
    }

    private GridObject GetBlockAtPosition(Vector2Int position)
    {
        gridObjects.TryGetValue(position, out GridObject block);
        return block;
    }

    private void MoveBlock(GridObject block, Vector2Int newPosition)
    {
        block.gridPosition = newPosition;
        block.transform.position = new Vector3(newPosition.x, newPosition.y, 0);
    }
}

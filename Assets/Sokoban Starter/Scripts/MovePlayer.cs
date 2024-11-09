using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    private GridObject gridObject;
    void Start()
    {
        gridObject = GetComponent<GridObject>();
    }

    // Update is called once per frame
    void Update()
    {

        // Get dimensions
        int maxX = (int)GridMaker.reference.dimensions.x;
        int maxY = (int)GridMaker.reference.dimensions.y;



        if (Input.GetKeyDown(KeyCode.S))
        {
            gridObject.gridPosition.y += 1;
            if (gridObject.gridPosition.y > maxY)
                gridObject.gridPosition.y = maxY;  // Bottem
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
}
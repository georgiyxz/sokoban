using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private GridObject gridObject;
    private Vector2Int gridDimensions;

    private void Start()
    {
        gridObject = GetComponent<GridObject>();
        gridDimensions = new Vector2Int((int)GridMaker.reference.dimensions.x, (int)GridMaker.reference.dimensions.y);
    }

    void Update()
    {
        Vector2Int direction = GetInputDirection();

        if (direction != Vector2Int.zero)
        {
            MoveStickyBlocks(direction);
            MoveClingyBlocks(direction);
            TryMove(gridObject, direction);
        }
    }

    private Vector2Int GetInputDirection()
    {
        if (Input.GetKeyDown(KeyCode.W)) return new Vector2Int(0, -1);
        if (Input.GetKeyDown(KeyCode.S)) return new Vector2Int(0, 1);
        if (Input.GetKeyDown(KeyCode.A)) return new Vector2Int(-1, 0);
        if (Input.GetKeyDown(KeyCode.D)) return new Vector2Int(1, 0);
        return Vector2Int.zero;
    }

    private void MoveStickyBlocks(Vector2Int direction)
    {
        Vector2Int playerPosition = gridObject.gridPosition;

        List<GridObject> stickyBlocks = GetAdjacentStickyBlocks(playerPosition);
        foreach (GridObject sticky in stickyBlocks)
        {
            TryMoveSticky(sticky, direction);
        }
    }

    private void MoveClingyBlocks(Vector2Int direction)
    {
        Vector2Int playerPosition = gridObject.gridPosition;

        List<GridObject> clingyBlocks = GetAdjacentClingyBlocks(playerPosition);
        foreach (GridObject clingy in clingyBlocks)
        {
            TryMoveClingy(clingy, direction);
        }
    }

    private List<GridObject> GetAdjacentStickyBlocks(Vector2Int position)
    {
        List<GridObject> stickyBlocks = new List<GridObject>();
        Vector2Int[] adjacentDirections = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int dir in adjacentDirections)
        {
            GameObject stickyBlock = GetBlockAtPosition(position + dir, "Sticky");
            if (stickyBlock != null)
            {
                stickyBlocks.Add(stickyBlock.GetComponent<GridObject>());
            }
        }

        return stickyBlocks;
    }

    private List<GridObject> GetAdjacentClingyBlocks(Vector2Int position)
    {
        List<GridObject> clingyBlocks = new List<GridObject>();
        Vector2Int[] adjacentDirections = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int dir in adjacentDirections)
        {
            GameObject clingyBlock = GetBlockAtPosition(position + dir, "Clingy");
            if (clingyBlock != null)
            {
                clingyBlocks.Add(clingyBlock.GetComponent<GridObject>());
            }
        }

        return clingyBlocks;
    }

    private bool TryMoveSticky(GridObject sticky, Vector2Int direction)
    {
        Vector2Int stickyTargetPosition = sticky.gridPosition + direction;

        if (IsWithinBounds(stickyTargetPosition))
        {
            GameObject blockAtTarget = GetBlockAtPosition(stickyTargetPosition);

            if (blockAtTarget == null || blockAtTarget.CompareTag("Player"))
            {
                sticky.gridPosition = stickyTargetPosition;
                Debug.Log($"Sticky moved to {stickyTargetPosition}");
                return true;
            }
            else if (blockAtTarget.CompareTag("Smooth"))
            {
                if (TryPushBlock(blockAtTarget, direction))
                {
                    sticky.gridPosition = stickyTargetPosition;
                    return true;
                }
            }
        }

        Debug.Log("Sticky movement blocked.");
        return false;
    }


    private void TryMove(GridObject movingObject, Vector2Int direction)
    {
        Vector2Int targetPosition = movingObject.gridPosition + direction;

        if (!IsWithinBounds(targetPosition))
        {
            Debug.Log("Target position is out of bounds.");
            return;
        }

        if (IsWallAtPosition(targetPosition))
        {
            Debug.Log("Movement blocked by a wall.");
            return;
        }

        if (IsClingyAtPosition(targetPosition))
        {
            Debug.Log("Movement blocked by Clingy block.");
            return;
        }

        GameObject blockAtTarget = GetBlockAtPosition(targetPosition);

        if (blockAtTarget == null)
        {
            MovePlayer(movingObject, direction);
        }
        else if (blockAtTarget.CompareTag("Smooth"))
        {
            if (TryPushBlock(blockAtTarget, direction))
            {
                MovePlayer(movingObject, direction);
            }
        }
    }

    private void TryMoveClingy(GridObject clingy, Vector2Int direction)
    {
        Vector2Int clingyTargetPosition = clingy.gridPosition + direction;

        if (IsWithinBounds(clingyTargetPosition))
        {
            GameObject blockAtTarget = GetBlockAtPosition(clingyTargetPosition);
            Vector2Int oppositeDirection = -direction;
            Vector2Int oppositePosition = clingy.gridPosition + oppositeDirection;
            GameObject blockAtOpposite = GetBlockAtPosition(oppositePosition);

            if ((blockAtTarget == null || blockAtTarget.CompareTag("Player")) &&
                (blockAtOpposite == null || !blockAtOpposite.CompareTag("Player")))
            {
                clingy.gridPosition = clingyTargetPosition;
                Debug.Log($"Clingy moved to {clingyTargetPosition}");
            }
            else
            {
                Debug.Log("Clingy movement blocked by a non-player object or player in opposite direction.");
            }
        }
    }

    private void MovePlayer(GridObject movingObject, Vector2Int direction)
    {
        Vector2Int newPosition = movingObject.gridPosition + direction;
        movingObject.gridPosition = newPosition;
    }

    private bool TryPushBlock(GameObject block, Vector2Int direction)
    {
        GridObject blockGridObject = block.GetComponent<GridObject>();
        Vector2Int blockTargetPosition = blockGridObject.gridPosition + direction;

        if (IsWithinBounds(blockTargetPosition) &&
            GetBlockAtPosition(blockTargetPosition) == null &&
            !IsWallAtPosition(blockTargetPosition) &&
            !block.CompareTag("Clingy"))
        {
            blockGridObject.gridPosition = blockTargetPosition;
            return true;
        }
        return false;
    }


    private bool IsClingyAtPosition(Vector2Int position)
    {
        GameObject[] clingyBlocks = GameObject.FindGameObjectsWithTag("Clingy");

        foreach (GameObject clingy in clingyBlocks)
        {
            GridObject clingyGridObject = clingy.GetComponent<GridObject>();
            if (clingyGridObject.gridPosition == position)
            {
                return true;
            }
        }
        return false;
    }


    private bool IsWallAtPosition(Vector2Int position)
    {
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");

        foreach (GameObject wall in walls)
        {
            GridObject wallGridObject = wall.GetComponent<GridObject>();
            if (wallGridObject.gridPosition == position)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsWithinBounds(Vector2Int position)
    {
        return position.x > 0 && position.x < gridDimensions.x + 1 &&
               position.y > 0 && position.y < gridDimensions.y + 1;
    }

    private GameObject GetBlockAtPosition(Vector2Int position, string tag = null)
    {
        GameObject[] blocks = tag != null ? GameObject.FindGameObjectsWithTag(tag) : GameObject.FindObjectsOfType<GameObject>();

        foreach (GameObject block in blocks)
        {
            GridObject blockGridObject = block.GetComponent<GridObject>();
            if (blockGridObject != null && blockGridObject.gridPosition == position)
            {
                return block;
            }
        }
        return null;
    }
}
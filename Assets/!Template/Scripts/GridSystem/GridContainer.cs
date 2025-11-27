using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridContainer : MonoBehaviour
{
    [SerializeField] private Vector2Int sizeGrid;
    [SerializeField] private Grid grid;
    private GridCellData[,] cellDatas = new GridCellData[0, 0];
    private List<IGridObject> gridObjects = new List<IGridObject>();

    public Vector2Int SizeGrid => sizeGrid;

    public void Init()
    {
        cellDatas = new GridCellData[sizeGrid.x, sizeGrid.y];

        for (int x = 0; x < sizeGrid.x; x++)
        {
            for (int y = 0; y < sizeGrid.y; y++)
            {
                cellDatas[x, y] = new GridCellData(null);
            }
        }
    }

    public void PutObjectInCell(Vector2Int gridPosition, IGridObject gridObject, out Coroutine coroutine)
    {
        cellDatas[gridPosition.x, gridPosition.y].PutObject(gridObject);
        gridObject.SetPosition(GetWorldPosition(gridPosition), gridPosition, out coroutine);
    }

    public void ClearCell (Vector2Int gridPosition)
    {
        ClearCell(cellDatas[gridPosition.x, gridPosition.y]);
    }

    public void ClearCell(GridCellData cellData)
    {
        cellData.Clear();
    }

    public void MoveObjectFromCell(Vector2Int from, Vector2Int to, out Coroutine coroutine)
    {
        IGridObject gridObject = cellDatas[from.x, from.y].GridObject;
        ClearCell(from);
        PutObjectInCell(to, gridObject, out coroutine);
    }

    public void MixedObjectFromCell (Vector2Int from, Vector2Int to, out Coroutine coroutine, out Coroutine coroutine2)
    {
        IGridObject gridObject = cellDatas[from.x, from.y].GridObject;
        IGridObject gridObject2 = cellDatas[to.x, to.y].GridObject;
        ClearCell(from);
        ClearCell(to);

        PutObjectInCell(to, gridObject, out coroutine);
        PutObjectInCell(from, gridObject2, out coroutine2);
    }

    public void AddGridObject(IGridObject gridObject, params Vector2Int[] gridPositions)
    {
        //Debug.Log($"gridObjects.Contains {gridObjects.Contains(gridObject)}");
        if (gridObjects.Contains(gridObject))
            return;

        gridObjects.Add(gridObject);

        foreach (var gridPos in gridPositions)
        {
            //Debug.Log($"AddGridObject {gridPos}");
            PutObjectInCell(gridPos, gridObject, out Coroutine coroutine);
        }
    }

    public void RemoveGridObject (IGridObject gridObject)
    {
        gridObjects.Remove(gridObject);

        foreach (var cellData in cellDatas)
        {
            if (cellData.GridObject == gridObject)
            {
                ClearCell(cellData);
            }
        }
    }

    public Vector3 GetWorldPosition (Vector2Int gridPosition)
    {
        return grid.CellToWorld(new Vector3Int(gridPosition.x, gridPosition.y, 0));
    }

    public GridCellData GetCellData (Vector2Int gridPosition)
    {
        if (IsCellInGrid(gridPosition) == false)        
            return null;

        return cellDatas[gridPosition.x, gridPosition.y];
    }

    public bool TryGetCellData (Vector2Int gridPosition, out GridCellData gridCellData)
    {
        gridCellData = GetCellData(gridPosition);
        if (GridCellData.NullOrEmpty(gridCellData))
            return false;

        return true;
    }

    public bool IsPointInGrid(Vector2 point)
    {
        Vector3 posCellZero = grid.CellToWorld(Vector3Int.zero);
        Vector3 posCellEnd = grid.CellToWorld(new Vector3Int(sizeGrid.x, sizeGrid.y));

        point = OffsetPoint(point);
        return point.x >= posCellZero.x && point.y >= posCellZero.y && point.x <= posCellEnd.x && point.y <= posCellEnd.y;
    }

    public bool IsCellInGrid (Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < sizeGrid.x && cell.y >= 0 && cell.y < sizeGrid.y;
    }

    public Vector2Int GetInGridPosition(Vector2 point)
    {
        //Debug.Log($"GetInGridPosition {(Vector2Int)grid.WorldToCell(OffsetPoint(point))}");
        return (Vector2Int)grid.WorldToCell(point + (Vector2)(grid.cellSize * grid.transform.localScale.x / 2));
    }

    private Vector2 OffsetPoint(Vector2 point)
    {
        return point + (Vector2)(grid.cellSize * grid.transform.localScale.x / 2);
    }
}

public class GridCellData
{
    public bool Exisit => GridObject != null;
    public IGridObject GridObject;

    public GridCellData(IGridObject gridObject)
    {
        GridObject = gridObject;
    }

    public void Clear()
    {
        GridObject = null;
    }

    public void PutObject(IGridObject gridObject)
    {
        GridObject = gridObject;
    }

    public static bool NullOrEmpty (GridCellData gridCellData)
    {
        return gridCellData == null || gridCellData.Exisit == false;
    }
}

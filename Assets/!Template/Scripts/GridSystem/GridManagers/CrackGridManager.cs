using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GridContainer))]
public class CrackGridManager : MonoBehaviour
{
    private Vector2Int sizeGrid;
    [SerializeField] private GridContainer gridContainer;
    [SerializeField] private CrackBlock crackBlockPref;

    [Space]
    [SerializeField] private GameObject brokeVFX;
    //[SerializeField] private GameObject dropVFX;
    [SerializeField] private GameObject createVFX;

    [Space]
    [SerializeField] private int addScore = 100;
    [SerializeField] private float kofAddScore = 1.5f;

    private ScoreSystem scoreSystem;

    public event Action OnStartBrokeBlock;
    public event Action OnEndBrokeBlock;
    public event Action OnMoveBlock;
    public event Action OnEndMoveBlock;

    public void Init (ScoreSystem scoreSystem)
    {
        this.scoreSystem = scoreSystem;
        gridContainer.Init();
        sizeGrid = gridContainer.SizeGrid;
        StartCoroutine(CreateAllBlocks());
    }

    private IEnumerator CreateAllBlocks()
    {
        for (int x = 0; x < sizeGrid.x; x++)
        {
            for (int y = 0; y < sizeGrid.y; y++)
            {
                GridCellData gridCellData = gridContainer.GetCellData(new Vector2Int(x, y));

                if (GridCellData.NullOrEmpty(gridCellData))
                {
                    CrackBlock crackBlock = Instantiate(crackBlockPref, gridContainer.GetWorldPosition(new Vector2Int(x, y)) + (Vector3.up * 10f), transform.rotation);
                    crackBlock.Init();
                    Destroy(Instantiate(createVFX, crackBlock.transform.position, transform.rotation), 10f);

                    gridContainer.AddGridObject(crackBlock.GridObject, new Vector2Int(x, y));
                    yield return new WaitForSeconds(0.05f);
                }
            }
        }
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator MixedAllBlocks()
    {
        Coroutine[,] coroutines = new Coroutine[sizeGrid.x, sizeGrid.y];

        for (int x = 0; x < sizeGrid.x; x++)
        {
            for (int y = 0; y < sizeGrid.y; y++)
            {
                GridCellData gridCellData = gridContainer.GetCellData(new Vector2Int(x, y));
                if (GridCellData.NullOrEmpty(gridCellData) == false)
                {
                    Vector2Int p1 = new Vector2Int(x, y);
                    Vector2Int p2 = new Vector2Int(x, y);
                    GridCellData gridCellDataEnd = null;
                    while (p1 == p2 || gridContainer.TryGetCellData(p2, out gridCellDataEnd) == false)
                    {
                        p2 = new Vector2Int(UnityEngine.Random.Range(0, sizeGrid.x), UnityEngine.Random.Range(0, sizeGrid.y));
                    }

                    gridContainer.MixedObjectFromCell(p1, p2, out coroutines[x, y], out coroutines[p2.x, p2.y]);
                    /*gridCellData.GridObject.SetPosition(gridContainer.GetWorldPosition(p1),
                        p1, out coroutines[x, y]);
                    gridCellDataEnd.GridObject.SetPosition(gridContainer.GetWorldPosition(p2),
                        p2, out coroutines[p2.x, p2.y]);*/
                    yield return new WaitForSeconds(0.05f);
                }
            }
        }

        for (int x = 0; x < sizeGrid.x; x++)
        {
            for (int y = 0; y < sizeGrid.y; y++)
            {
                if(coroutines[x, y] != null)
                    yield return coroutines[x, y];
            }
        }

        yield return new WaitForSeconds(0.05f);
    }

    public void MoveInEmptySpaceBlocks ()
    {
        StartCoroutine(MoveCoroutine());
    }

    private IEnumerator MoveCoroutine()
    {
        OnMoveBlock?.Invoke();

        //Двигаем вниз
        while (true)
        {
            Debug.Log("Двигаем вниз");
            List<(Vector2Int, Vector2Int)> needMove = new List<(Vector2Int, Vector2Int)>();
            for (int y = 1; y < sizeGrid.y; y++)
            {
                for (int x = 0; x < sizeGrid.x; x++)
                {
                    Vector2Int p1 = new Vector2Int(x, y);
                    Vector2Int p2 = new Vector2Int(x, y - 1);
                    GridCellData cellData = gridContainer.GetCellData(p1);
                    GridCellData cellDataDown = gridContainer.GetCellData(p2);
                    if (GridCellData.NullOrEmpty(cellData) == false &&
                        GridCellData.NullOrEmpty(cellDataDown))
                    {
                        needMove.Add((p1, p2));
                    }
                }
            }

            if (needMove.Count == 0)
                break;

            needMove.Reverse();

            Coroutine[] coroutines = new Coroutine[needMove.Count];
            for (int i = 0; i < needMove.Count; i++)
            {
                gridContainer.MoveObjectFromCell(needMove[i].Item1, needMove[i].Item2, out coroutines[i]);
            }

            for (int i = 0; i < coroutines.Length; i++)
            {
                yield return coroutines[i];
            }

            yield return new WaitForEndOfFrame();
        }

        //Пауза
        yield return new WaitForSeconds(0.4f);

        //Двигаем влево
        bool moveLeftGlobal = true;
        while (moveLeftGlobal)
        {
            Debug.Log("Двигаем влево");
            moveLeftGlobal = false;

            for (int x = 1; x < sizeGrid.x; x++)
            {
                bool moveLeft = true;
                List<(Vector2Int, Vector2Int)> needMove = new List<(Vector2Int, Vector2Int)>();
                for (int y = 0; y < sizeGrid.y; y++)
                {
                    Vector2Int p1 = new Vector2Int(x, y);
                    Vector2Int p2 = new Vector2Int(x - 1, y);
                    GridCellData cellData = gridContainer.GetCellData(p1);
                    GridCellData cellDataSide = gridContainer.GetCellData(p2);
                    if (GridCellData.NullOrEmpty(cellDataSide) == false)
                    {
                        moveLeft = false;
                        break;
                    }

                    if (GridCellData.NullOrEmpty(cellData) == false)
                        needMove.Add((p1, p2));
                }

                if (moveLeft && needMove.Count > 0)
                {
                    Coroutine[] coroutines = new Coroutine[needMove.Count];
                    for (int i = 0; i < needMove.Count; i++)
                    {
                        gridContainer.MoveObjectFromCell(needMove[i].Item1, needMove[i].Item2, out coroutines[i]);
                    }

                    for (int i = 0; i < coroutines.Length; i++)
                    {
                        yield return coroutines[i];
                    }

                    moveLeftGlobal = true;
                }
            }

            yield return new WaitForEndOfFrame();
        }

        //Заполняем пустоты, если надо
        Debug.Log("Заполняем пустоты, если надо");
        if (IsHavePaies(3) == false)
        {
            Debug.Log("Заполняем пустоты!");
            Coroutine coroutine = StartCoroutine(CreateAllBlocks());
            yield return coroutine;

            //Пауза
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("Mixed?");
        while (IsHavePaies(3) == false)
        {
            Debug.Log("Mixed");
            yield return StartCoroutine(MixedAllBlocks());
        }

        OnEndMoveBlock?.Invoke();

        //Конец
        yield break;
    }

    private bool IsHavePaies(int count)
    {
        List<CrackBlock> listBlocks = new List<CrackBlock>();
        for (int x = 0; x < sizeGrid.x; x++)
        {
            for (int y = 0; y < sizeGrid.y; y++)
            {
                listBlocks.Clear();
                Vector2Int gridPos = new Vector2Int(x, y);
                if (gridContainer.TryGetCellData(gridPos, out GridCellData gridCellData))
                {
                    AddBlocksAround(gridPos, gridCellData.GridObject.GetMono().GetComponent<CrackBlock>().Sign, ref listBlocks);
                    if (listBlocks.Count >= count)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public void ClickPoint(Vector2 point)
    {
        //Debug.Log($"ClickPoint {point}");
        if (gridContainer.IsPointInGrid(point) == false)
            return;

        ClickGrid(gridContainer.GetInGridPosition(point));
    }

    public void ClickGrid (Vector2Int gridPosition)
    {
        GridCellData cellData = gridContainer.GetCellData(gridPosition);
        //Debug.Log($"NullOrEmpty {gridPosition} {GridCellData.NullOrEmpty(cellData)}");
        if (GridCellData.NullOrEmpty(cellData))
            return;

        IGridObject gridObject = cellData.GridObject;
        CrackBlock crackBlock = gridObject.GetMono().GetComponent<CrackBlock>();

        SignBlock sign = crackBlock.Sign;

        List<CrackBlock> crackBlocks = new List<CrackBlock>();
        crackBlocks.Add(crackBlock);

        AddBlocksAround(gridPosition, sign, ref crackBlocks);

        if (crackBlocks.Count < 3)
            return;

        for (int i = 0; i < crackBlocks.Count; i++)
        {
            gridContainer.RemoveGridObject(crackBlocks[i].GridObject);
            //crackBlocks[i].Broke();
        }
        StartCoroutine(BrokeCoroutine(crackBlocks.ToArray()));
    }

    private void AddBlocksAround(Vector2Int gridPosition, SignBlock sign, ref List<CrackBlock> crackBlocks)
    {
        CrackBlock[] blocks = new CrackBlock[4];
        Vector2Int[] blocksPositions = new Vector2Int[4];

        blocks[0] = _BlockTest(gridPosition.x - 1, gridPosition.y);
        blocksPositions[0] = new Vector2Int(gridPosition.x - 1, gridPosition.y);

        blocks[1] = _BlockTest(gridPosition.x + 1, gridPosition.y);
        blocksPositions[1] = new Vector2Int(gridPosition.x + 1, gridPosition.y);

        blocks[2] = _BlockTest(gridPosition.x, gridPosition.y - 1);
        blocksPositions[2] = new Vector2Int(gridPosition.x, gridPosition.y - 1);

        blocks[3] = _BlockTest(gridPosition.x, gridPosition.y + 1);
        blocksPositions[3] = new Vector2Int(gridPosition.x, gridPosition.y + 1);

        for (int i = 0; i < blocks.Length; i++)
        {
            if (blocks[i] != null && crackBlocks.Contains(blocks[i]) == false)
            {
                crackBlocks.Add(blocks[i]);
                AddBlocksAround(blocksPositions[i], sign, ref crackBlocks);
            }
        }

        CrackBlock _BlockTest(int x, int y) 
        {
            GridCellData cellData = gridContainer.GetCellData(new Vector2Int(x, y));

            if (GridCellData.NullOrEmpty(cellData))
                return null;

            CrackBlock crackBlock = cellData.GridObject.GetMono().GetComponent<CrackBlock>();
            if (crackBlock.Sign == sign)            
               return crackBlock;            

            return null;
        }
    }

    private IEnumerator BrokeCoroutine (CrackBlock[] crackBlocks)
    {
        OnStartBrokeBlock?.Invoke();

        for (int i = 0; i < crackBlocks.Length; i++)
        {
            Destroy(Instantiate(brokeVFX, crackBlocks[i].transform.position, transform.rotation), 10f);
            crackBlocks[i].Broke();
            scoreSystem.AddScore((int)(addScore * (kofAddScore * (i + 1))), crackBlocks[i].transform.position);
            yield return new WaitForSeconds(0.05f);
        }
        yield return new WaitForSeconds(0.5f);

        OnEndBrokeBlock?.Invoke();
    }
}
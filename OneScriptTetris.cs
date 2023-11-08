using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct pg
{
    public int[,] grid;
}

public class OneScriptTetris : MonoBehaviour
{
    [SerializeField] GameObject[] prefabPieces;
    pg[] prefabGridPieces;
    [SerializeField] GameObject prefabChunk;

    [SerializeField] Transform activePiece;
    int[,] gridPiece;

    [SerializeField] float downTime, inputTime;
    [SerializeField] bool isActive;
    float downTimer = 0f, inputTimer = 0f;

    int[,] grid2D;
    [SerializeField] int gridWidth, gridHeight;
    Transform[,] chunksObj;

    void Awake()
    {
        CreateGridPieces();
        CreateGrid2D();
        RefreshGrid();

        CreatePiece();
    }

    private void CreateGridPieces()
    {
        prefabGridPieces = new pg[7];
        prefabGridPieces[0].grid = new int[3, 3] { { 1, 1, 1 }, { 0, 1, 0 }, { 0, 0, 0 } }; //T
        prefabGridPieces[1].grid = new int[2, 2] { { 1, 1 }, { 1, 1 } }; //Cube
        prefabGridPieces[2].grid = new int[4, 4] { { 0, 1, 0, 0 }, { 0, 1, 0, 0 }, { 0, 1, 0, 0 }, { 0, 1, 0, 0 } }; //I
        prefabGridPieces[3].grid = new int[3, 3] { { 1, 1, 0 }, { 0, 1, 1 }, { 0, 0, 0 } }; //S
        prefabGridPieces[4].grid = new int[3, 3] { { 0, 1, 1 }, { 1, 1, 0 }, { 0, 0, 0 } }; //Z
        prefabGridPieces[5].grid = new int[3, 3] { { 1, 1, 0 }, { 1, 0, 0 }, { 1, 0, 0 } }; //LR
        prefabGridPieces[6].grid = new int[3, 3] { { 0, 1, 1 }, { 0, 0, 1 }, { 0, 0, 1 } }; //LL
    }

    private void CreateGrid2D()
    {
        grid2D = new int[gridHeight, gridWidth];
        chunksObj = new Transform[gridHeight, gridWidth];

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                grid2D[y, x] = 1;
            }
        }
    }

    private void CreatePiece()
    {
        int ran = Random.Range(0, prefabPieces.Length);
        gridPiece = prefabGridPieces[ran].grid.Clone() as int[,];

        Vector3 startPos = new Vector3((int)(gridWidth / 2) - 1, gridHeight - gridPiece.GetLength(0)-1, 0);

        GameObject tempPiece = Instantiate(prefabPieces[ran], startPos, Quaternion.identity);

        if (IsPossible(gridPiece, startPos))
        {
            activePiece = tempPiece.transform;
            isActive = true;
        }
        else
        {
            Debug.Log("GAMEOVER");
        }
    }

    void Update()
    {
        if (!isActive)
            return;

        GetInput();

        downTimer += Time.deltaTime;
        if(downTimer > downTime)
        {
            downTimer = 0f;
            MoveDown();
        }
    }

    private void MoveDown()
    {
        if (!activePiece)
            return;

        Vector3 pos = activePiece.position;
        pos.y -= 1;

        if(IsPossible(gridPiece, pos))
        {
            activePiece.position = pos;
        }
        else
        {
            PlacePiece();
        }
    }

    private void PlacePiece()
    {
        isActive = false;

        foreach (Transform child in activePiece)
        {
            chunksObj[(int)child.position.y, (int)child.position.x] = Instantiate(prefabChunk, child.position, child.rotation).transform;
            grid2D[(int)child.position.y, (int)child.position.x] = 1;
        }

        Destroy(activePiece.gameObject);
        CheckChunks();
        CreatePiece();
    }

    private void CheckChunks()
    {
        List<int> ysToDestroy = new List<int>();

        for (int y = 1; y < gridHeight - 1; y++)
        {
            for (int x = 1; x < gridWidth - 1; x++)
            {
                if (grid2D[y, x] == 0)
                    break;

                if (x == gridWidth - 2)
                    ysToDestroy.Add(y);
            }
        }

        if (ysToDestroy.Count > 0)
            DestroyChunks(ysToDestroy.ToArray());
    }

    private void DestroyChunks(int[] ys)
    {
        foreach(int y in ys)
        {
            for (int x = 1; x < gridWidth - 1; x++)
            {
                grid2D[y, x] = 0;
                Destroy(chunksObj[y, x].gameObject);
                chunksObj[y, x] = null;
            }
        }

        CheckVoids();
    }

    private void CheckVoids()
    {
        int inSeries = 0;

        for (int y = 1; y < gridHeight - 1; y++)
        {
            if (inSeries > 4 || y > gridHeight - 3)
                return;

            for (int x = 1; x < gridWidth - 1; x++)
            {
                if (grid2D[y, x] == 1)
                {
                    if (inSeries > 0)
                    {
                        CollapseRow(y, inSeries);
                        inSeries = 0;
                        y--;
                    }

                    break;
                }

                if (x == gridWidth - 2)
                    inSeries++;
            }
        }
    }

    private void CollapseRow(int startY, int series)
    {
        for (int y = startY; y < gridHeight - 1; y++)
        {
            for (int x = 1; x < gridWidth - 1; x++)
            {
                chunksObj[y - series, x] = chunksObj[y, x];

                if(chunksObj[y, x])
                {
                    Vector3 newPos = chunksObj[y, x].position;
                    newPos.y -= series;
                    chunksObj[y, x].position = newPos;
                }
            }
        }

        RefreshGrid();
    }

    private void GetInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
            AttemptRotate();

        inputTimer += Time.deltaTime;

        if (inputTimer > inputTime)
            inputTimer = 0f;
        else
            return;

        if (Input.GetKey(KeyCode.LeftArrow))
            AttemptMove(-1);

        if (Input.GetKey(KeyCode.RightArrow))
            AttemptMove(1);

        if (Input.GetKey(KeyCode.DownArrow))
            MoveDown();
    }

    private void AttemptMove(int side)
    {
        if (!activePiece)
            return;

        Vector3 pos = activePiece.position;
        pos.x += side;

        if(IsPossible(gridPiece, pos))
            activePiece.position = pos;
    }

    private void AttemptRotate()
    {
        int[,] origPiece = gridPiece.Clone() as int[,];
        
        for (int y = 0; y < gridPiece.GetLength(0); y++)
        {
            for (int x = 0; x < gridPiece.GetLength(1); x++)
            {
                gridPiece[y, x] = origPiece[x, gridPiece.GetLength(0)-1-y];
            }
        }

        if(IsPossible(gridPiece, activePiece.position))
        {
            List<Transform> tempChunk = new List<Transform>();
            foreach (Transform child in activePiece)
                tempChunk.Add(child);

            for (int y = 0; y < gridPiece.GetLength(0); y++)
            {
                for (int x = 0; x < gridPiece.GetLength(1); x++)
                {
                    if (gridPiece[y, x] == 1)
                    {
                        tempChunk[0].position = new Vector3(activePiece.position.x + x, activePiece.position.y + y, 0);
                        tempChunk.RemoveAt(0);
                    }
                }
            }
        }
        else
        {
            gridPiece = origPiece.Clone() as int[,];
        }
    }

    private bool IsPossible(int[,] arrayPiece, Vector3 nextPos)
    {
        for (int y = 0; y < arrayPiece.GetLength(0); y++)
        {
            for (int x = 0; x < arrayPiece.GetLength(1); x++)
            {
                if (arrayPiece[y, x] == 1)
                {
                    if (grid2D[(int)nextPos.y + y, (int)nextPos.x + x] == 1)
                        return false;
                }
            }
        }

        return true;
    }

    private void RefreshGrid()
    {
        for (int y = 1; y < gridHeight - 1; y++)
        {
            for (int x = 1; x < gridWidth - 1; x++)
            {
                grid2D[y, x] = 0;
            }
        }

        foreach (Transform chk in chunksObj)
        {
            if(chk)
                grid2D[(int)chk.position.y, (int)chk.position.x] = 1;
        }
    }

    private void OnDrawGizmos()
    {
        if (grid2D == null)
            return;

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (grid2D[y, x] == 0)
                    Gizmos.color = Color.green;
                else
                    Gizmos.color = Color.red;

                Gizmos.DrawSphere(new Vector3(x, y, 0), 0.1f);
            }
        }
    }
}

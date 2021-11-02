using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellBoard : MonoBehaviour
{
    [SerializeField] private GameObject CellGO;

    private Cell[,] boardMap = new Cell[GameManager.ROW, GameManager.COLUMN];

    private Cell switchSrcCell = null;
    private Cell switchTargetCell = null;

    private bool isSrcComplete = false;
    private bool isTargetComplete = false;

    public bool CheckBoardRow()
    {
        bool hasClear = false;
        for(int row=0; row<GameManager.ROW; row++)
        {
            for (int col=0; col<GameManager.COLUMN; col++)
            {
                int eq = 1;
                Cell cell = boardMap[row, col];
                int c = col + 1;
                while(c<GameManager.COLUMN)
                {
                    Cell nextCell = boardMap[row, c];
                    if (cell.cType!=nextCell.cType)
                    {
                        break;
                    }
                    eq++;
                    c++;
                }
                if (eq>2)
                {
                    hasClear = true;
                    int end = eq + col;
                    for (int cc=col; cc<end;cc++)
                    {
                        Cell cell3 = boardMap[row, cc];
                        cell3.SetNull(true);
                    }
                }
            }
        }
        return hasClear;
    }
    public bool CheckBoardCol()
    {
        bool hasClear = false;
        for (int col=0; col<GameManager.COLUMN; col++)
        {
            for(int row=0; row<GameManager.ROW; row++)
            {
                int eq = 1;
                Cell cell = boardMap[row, col];
                int r = row + 1;
                while(r<GameManager.ROW)
                {
                    Cell nextCell = boardMap[r, col];
                    if (cell.cType!=nextCell.cType)
                    {
                        break;
                    }
                    eq++;
                    r++;
                }
                if (eq>2)
                {
                    hasClear = true;
                    int end = eq + row;
                    for (int rr=row; rr<end; rr++)
                    {
                        Cell cell3 = boardMap[rr, col];
                        cell3.SetNull(true);
                    }
                }
            }
        }
        return hasClear;
    }

    public void CheckBoard()
    {
        CheckBoardCol();
        CheckBoardRow();
    }

    // 重置Board
    public void ResetBoard()
    {
        float wOffset = 0.0f;
        float hOffset = 0.0f;
        if (GameManager.ROW%2==0)
        {
            hOffset = 0.5f;
        }
        if (GameManager.COLUMN%2==0)
        {
            wOffset = 0.5f;
        }
        // 偏移计算：当前Sprite的Pixels Pre Unit是128， Sprite资源也是128 ，1：1
        float halfWOffset = GameManager.ROW / 2;
        float halfHOffset = GameManager.COLUMN / 2;
        for (int row=0; row<GameManager.ROW; row++)
        {
            for(int col=0; col<GameManager.COLUMN; col++)
            {
                Cell oldCell = boardMap[row, col];
                if (oldCell)
                {
                    GameObject.Destroy(oldCell.gameObject);
                    boardMap[row, col] = null;
                }
                GameObject go = GameObject.Instantiate(CellGO, this.transform);
                // 珠子初始位置，注意行列
                go.transform.localPosition = new Vector3(-1 * (halfHOffset - col) + wOffset, 1 * (halfWOffset - row) - hOffset, 0);
                go.transform.localScale = Vector3.one;
                go.transform.localRotation = Quaternion.identity;

                Cell c = go.GetComponent<Cell>();
                c.SetCell(row, col);
                boardMap[row, col] = c;
            }
        }
    }

    public void SwitchCell(Cell srcCell, TouchDIR? direction)
    {
        if (direction == null || srcCell == null) return;
        if (direction == TouchDIR.LEFT && srcCell.column == 0)
            return;
        if (direction == TouchDIR.RIGHT && srcCell.column == GameManager.COLUMN - 1)
            return;
        if (direction == TouchDIR.DOWN && srcCell.row == GameManager.ROW - 1)
            return;
        if (direction == TouchDIR.UP && srcCell.row == 0)
            return;

        Cell targetCell = GetCellByDir(srcCell, direction);
        if (targetCell == null)
            return;

        this.switchSrcCell = srcCell;
        this.switchTargetCell = targetCell;
        this.isSrcComplete = false;
        this.isTargetComplete = false;

        int srcRow = srcCell.row;
        int srcCol = srcCell.column;
        int tarRow = targetCell.row;
        int tarCol = targetCell.column;
        Vector3 srcPos = GetCellPosition(srcCell);
        Vector3 tarPos = GetCellPosition(targetCell);
        srcCell.SetMove(tarPos, SrcCellMoveComplete);
        targetCell.SetMove(srcPos, targetCellMoveComplete);

        srcCell.SetCell(tarRow, tarCol);
        targetCell.SetCell(srcRow, srcCol);

        boardMap[srcRow, srcCol] = targetCell;
        boardMap[tarRow, tarCol] = srcCell;
    }

    void SrcCellMoveComplete()
    {
        this.isSrcComplete = true;
        AllComplete();
    }

    void targetCellMoveComplete()
    {
        this.isTargetComplete = true;
        AllComplete();
    }

    void AllComplete()
    {
        if (isSrcComplete == false || isTargetComplete == false)
            return;

        bool isDecrypt = IsDecryptSuccess();
        if ( !isDecrypt )
        {
            //没有消除动作，还原
            this.isSrcComplete = false;
            this.isTargetComplete = false;
            int srcRow = this.switchSrcCell.row;
            int srcCol = this.switchSrcCell.column;
            int tarRow = this.switchTargetCell.row;
            int tarCol = this.switchTargetCell.column;
            Vector3 srcPos = GetCellPosition(this.switchSrcCell);
            Vector3 tarPos = GetCellPosition(this.switchTargetCell);
            this.switchSrcCell.SetMove(tarPos);
            this.switchTargetCell.SetMove(srcPos, targetCellMoveComplete);

            this.switchSrcCell.SetCell(tarRow, tarCol);
            this.switchTargetCell.SetCell(srcRow, srcCol);

            boardMap[srcRow, srcCol] = this.switchTargetCell;
            boardMap[tarRow, tarCol] = this.switchSrcCell;
            return;
        }

        DoAction();
    }

    void DoAction()
    {
        //处理消除
        if (IsHDecryptSuccess(this.switchSrcCell))
            DoHHandler(this.switchSrcCell);
        if (IsVDecryptSuccess(this.switchSrcCell))
            DoVHandler(this.switchSrcCell);
        if (IsHDecryptSuccess(this.switchTargetCell))
            DoHHandler(this.switchTargetCell);
        if (IsVDecryptSuccess(this.switchTargetCell))
            DoVHandler(this.switchTargetCell);
    }

    void DoHHandler(Cell cell)
    {
        CellType cType = cell.cType;
        int row = cell.row;
        int col = cell.column;
        int rCenter = cell.row;
        int cCenter = cell.column;
        // 自身肯定被消除
        cell.SetNull(true);
        // 处理行
        for (int i = 1; i <= 2; i++)
        {
            int c1 = cCenter + i * 1;
            int c2 = cCenter + i * -1;
            if (c1 >= 0 && c1 < GameManager.COLUMN)
            {
                Cell leftCell1 = this.boardMap[rCenter, c1];
                if (leftCell1.cType == cType)
                {
                    leftCell1.SetNull(true);
                }
            }
            if (c2 >= 0 && c2 < GameManager.COLUMN)
            {
                Cell rightCell2 = this.boardMap[rCenter, c2];
                if (rightCell2.cType == cType)
                {
                    rightCell2.SetNull(true);
                }
            }
        }
    }

    void DoVHandler(Cell cell)
    {
        CellType cType = cell.cType;
        int row = cell.row;
        int col = cell.column;
        int rCenter = cell.row;
        int cCenter = cell.column;
        // 自身肯定被消除
        cell.SetNull(true);
        // 处理列
        for (int i = 1; i <= 2; i++)
        {
            int r1 = rCenter + i * 1;
            int r2 = rCenter + i * -1;
            if (r1 >= 0 && r1 < GameManager.ROW)
            {
                Cell upCell1 = this.boardMap[r1, cCenter];
                if (upCell1.cType == cType)
                {
                    upCell1.SetNull(true);
                }
            }
            if (r2 >= 0 && r2 < GameManager.ROW)
            {
                Cell bottomCell2 = this.boardMap[r2, cCenter];
                if (bottomCell2.cType == cType)
                {
                    bottomCell2.SetNull(true);
                }
            }
        }
    }

    bool IsDecryptSuccess()
    {
        if (IsCellDecryptSuccess(this.switchSrcCell))
            return true;
        if (IsCellDecryptSuccess(this.switchTargetCell))
            return true;
        return false;
    }

    bool IsCellDecryptSuccess(Cell cell)
    {
        if (IsHDecryptSuccess(cell))
            return true;
        if (IsVDecryptSuccess(cell))
            return true;
        return false;
    }
    bool IsHDecryptSuccess(Cell cell)
    {
        if (1 + GetCellDecryptCountBytag(cell, 1, 0) + GetCellDecryptCountBytag(cell, -1, 0) >= 3)
        {
            return true;
        }
        return false;
    }

    bool IsVDecryptSuccess(Cell cell)
    {
        if (1 + GetCellDecryptCountBytag(cell, 0, 1) + GetCellDecryptCountBytag(cell, 0, -1) >= 3)
        {
            return true;
        }
        return false;
    }

    int GetCellDecryptCountBytag(Cell cell, int tagH, int tagV)
    {
        int count = 0;
        for (int i = 1; i <= 2; i++)
        {
            int tempRow = (cell.row + i * tagV);
            int tempCol = (cell.column + i * tagH);
            if (tempRow<0 || tempRow >= GameManager.ROW || tempCol<0 || tempCol >= GameManager.COLUMN)
                continue;
            Cell tempCell = boardMap[tempRow, tempCol];
            if (tempCell!=null && tempCell.cType==cell.cType)
            {
                count += 1;
            }
            else
            {
                break;
            }
        }
        return count;
    }

    private Vector3 GetCellPosition(Cell cell)
    {
        return new Vector3(cell.gameObject.transform.position.x, cell.gameObject.transform.position.y, cell.gameObject.transform.position.z);
    }

    private Cell GetCellByDir(Cell cell, TouchDIR? direction)
    {
        int row = cell.row;
        int col = cell.column;
        if (direction == TouchDIR.LEFT)
            col -= 1;
        else if (direction == TouchDIR.RIGHT)
            col += 1;
        else if (direction == TouchDIR.UP)
            row -= 1;
        else if (direction == TouchDIR.DOWN)
            row += 1;

        return boardMap[row, col];
    }
}

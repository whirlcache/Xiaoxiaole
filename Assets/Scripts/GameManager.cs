using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("盘子行列数")]
    public const int ROW = 4;
    public const int COLUMN = 4;

    [HideInInspector]
    public CellBoard board = null;

    public static GameManager instance
    {
        private set;
        get;
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(0,0, 80, 30), new GUIContent("Reset")))
        {
            board.ResetBoard();
        }
        if (GUI.Button(new Rect(0, 50, 80, 30), new GUIContent("Check Col")))
        {
            board.CheckBoardCol();
        }
        if (GUI.Button(new Rect(100, 50, 80, 30), new GUIContent("Check Row")))
        {
            board.CheckBoardRow();
        }
    }

    void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        board = GameObject.Find("CellBoard").GetComponent<CellBoard>();
        //board.ResetBoard();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum CellType
{
    GREEN, BLUE, PURPLE, ORANGE, RED
}

[RequireComponent(typeof(SpriteRenderer))]
public class Cell : MonoBehaviour
{
    public Sprite[] sprites;
    [HideInInspector]
    public int row;
    [HideInInspector]
    public int column;
    [HideInInspector]
    public CellType cType;
    [HideInInspector]
    public bool isNull = false;

    private SpriteRenderer sprite;
    private TextMesh tempText;

    public delegate void Complete();

    // Start is called before the first frame update
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        tempText = GetComponentInChildren<TextMesh>();
        tempText.gameObject.SetActive(false);
        Hashtable typeMap = new Hashtable();
        typeMap["1"] = CellType.GREEN;
        typeMap["2"] = CellType.BLUE;
        typeMap["3"] = CellType.PURPLE;
        typeMap["4"] = CellType.ORANGE;
        typeMap["5"] = CellType.RED;
        if (sprites.Length>0)
        {
            int index = Random.Range(0, sprites.Length);
            sprite.sprite = sprites[index];
            cType = (CellType)typeMap[(index+1).ToString()];
        }
    }

    public void SetCell(int row, int col)
    {
        this.row = row;
        this.column = col;
    }

    public void SetNull(bool b)
    {
        isNull = b;
        tempText.gameObject.SetActive(b);
    }

    public void SetMove(Vector3 position, TweenCallback func=null)
    {
        Tween tween = this.transform.DOMove(position, 0.1f);
        if (func!=null)
        {
            TweenCallback callback = func;
            tween.OnComplete(callback);
        }
    }
}

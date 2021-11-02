using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TouchDIR
{
    LEFT, RIGHT, UP, DOWN
}


public class TouchController : MonoBehaviour
{
    private bool isPress = false;
    private Vector3 clickPos = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            clickPos = Input.mousePosition;
            isPress = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isPress = false;
        }
        if (Input.GetMouseButton(0))
        {
            if (!isPress) return;

            TouchMoving();
        }
    }

    TouchDIR? GetDir(Vector3 src, Vector3 target)
    {
        Vector3 vec = target - src;
        if (vec.magnitude>20)
        {
            float angleY = Mathf.Acos(Vector3.Dot(vec.normalized, Vector2.up)) * Mathf.Rad2Deg;
            float angleX = Mathf.Acos(Vector3.Dot(vec.normalized, Vector2.right)) * Mathf.Rad2Deg;
            if (angleY <= 45)
                return TouchDIR.UP;
            if (angleY >= 135)
                return TouchDIR.DOWN;
            if (angleX <= 45)
                return TouchDIR.RIGHT;
            if (angleX >= 135)
                return TouchDIR.LEFT;
        }
        return null;
    }

    private void TouchMoving()
    {
        TouchDIR? dir = GetDir(clickPos, Input.mousePosition);
        if (dir == null) return;
        RaycastHit2D hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 origin = ray.origin;
        Vector3 direction = ray.origin + Vector3.forward;
        hit = Physics2D.Raycast(origin, direction);
        if (hit.collider && hit.collider.gameObject && hit.collider.gameObject.tag=="Cell")
        {
            if (GameManager.instance.board)
            {
                isPress = false;
                Cell cell = hit.collider.gameObject.GetComponent<Cell>();
                GameManager.instance.board.SwitchCell(cell, dir);
            }
        }
    }
}

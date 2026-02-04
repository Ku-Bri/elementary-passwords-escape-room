using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickToDraw : MonoBehaviour
{
    [SerializeField] private LineRenderer pathPrefab; //Create prefab with LineRenderer and EdgeCollider2D, then assign it here
    private LineRenderer pathCurrent; //The current path
    private bool touchDown = false; //Whether or not the mouse (r touch if mobile) is down
    Vector3 lastPos = Vector3.zero; //To check distance between points

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) //Wait for left click
        {
            TouchStart();
            return;
        }
        if (touchDown) //if dragging
        {
            if (Input.GetMouseButtonUp(0)) //wait for left unclick
                TouchEnd();
            else
                TouchDrag(); //otherwise drag
        }
        /* Same as above but with mobile controls
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);


            if (touch.phase == TouchPhase.Began)
            {
                TouchStart();
                return;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                TouchEnd();
                return;
            }

            if (touchDown)
            {
                TouchDrag();
            }
        }
        */
    }

    //On Start - Start dragging and create a new pathPrefab
    private void TouchStart()
    {
        touchDown = true;
        pathCurrent = Instantiate(pathPrefab, Vector3.zero, Quaternion.identity, transform);
    }

    //On Drag - Add current mouse position to the LineRenderer
    private void TouchDrag()
    {
        Vector3 _pos = Input.mousePosition;
        if (Vector3.Distance(_pos, lastPos) > 30f) //Only add a point if the mouse has moved enough so it doesn't make loads on one spot
        {
            lastPos = _pos;
            _pos = Camera.main.ScreenToWorldPoint(Input.mousePosition); //get the new mouse position reletive to the screen
            _pos.z = 0;
            pathCurrent.positionCount++; //add it to the end of the array
            pathCurrent.SetPosition(pathCurrent.positionCount - 1, _pos);
        }
    }

    //On End - Stop dragging and add an EdgeCollider2D
    private void TouchEnd()
    {
        touchDown = false;
        Vector3[] points = new Vector3[pathCurrent.positionCount]; //Create array large enough to hold all the LineRenderer points
        pathCurrent.GetPositions(points); //Assign the points to the Array
        EdgeCollider2D eCol = pathCurrent.GetComponent<EdgeCollider2D>(); //Get the EdgeCollider2D which we added to the prefab when creating it
        eCol.points = ToVector2Array(points); //Assugb the EdgeCollider2D points after converting the Vec3 array to a Vec3 Array
    }

    private Vector2[] ToVector2Array(Vector3[] v3)
    {
        return System.Array.ConvertAll<Vector3, Vector2>(v3, getV3fromV2);
    }

    private Vector2 getV3fromV2(Vector3 v3)
    {
        return new Vector2(v3.x, v3.y);
    }
}



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public Camera cam;
    Vector3 pos = new Vector3(200, 200, 0);
    public GameObject obj;


    void Start()
    {

    }

    void FixedUpdate()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        //Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
        Debug.Log(ray.origin);
        obj.transform.position = ray.origin;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Movable1 : MonoBehaviour
{
    private Camera cam;
    Ray ray;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        ray = cam.ScreenPointToRay(Input.mousePosition);

    }

    private void OnMouseDrag()
    {
        Vector3 targetPos = new Vector3(ray.origin.x, ray.origin.y, 0);
        transform.position = targetPos;
    }
}

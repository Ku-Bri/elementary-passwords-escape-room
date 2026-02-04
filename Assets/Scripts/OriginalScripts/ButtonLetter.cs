using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonLetter : MonoBehaviour
{
    private Camera cam;
    Ray ray;
    public WordSearch wordSearch;
    private LineRenderer lineRenderer;
    public bool isLineDrawEnabled = false;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 2;
    }

    // Update is called once per frame
    void Update()
    {
        ray = cam.ScreenPointToRay(Input.mousePosition);
        //lineRenderer.SetPosition(0, this.gameObject.transform.position);
        //lineRenderer.SetPosition(1, ray.origin);
        if (isLineDrawEnabled == true)
        {
            lineRenderer.SetPosition(0, this.gameObject.transform.position);

            lineRenderer.SetPosition(1, new Vector3(ray.origin.x,ray.origin.y,0));
        }
    }

    public void LetterClicked()
    {
        wordSearch.ButtonPressed(this.gameObject.transform);
        isLineDrawEnabled=true;
    }

    public void OnMouseDrag()
    {
        Debug.Log("drawingLine."); Debug.Log("drawingLine.."); Debug.Log("drawingLine...");
        lineRenderer.SetPosition(0, this.gameObject.transform.position);  // Set position of the starting point
        lineRenderer.SetPosition(1, ray.origin);
    }
}

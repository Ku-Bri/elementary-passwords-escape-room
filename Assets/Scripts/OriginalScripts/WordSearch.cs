using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class WordSearch : MonoBehaviour
{
    private Vector3 buttonOrigin;
    private Vector3 mousePos;
    private Camera cam;
    Ray ray;
    public Material mat;
    [SerializeField] private GameObject buttonPrefab;
    private char[] wordsearch = {'A','B','C','D', 'A', 'B', 'C', 'D', 'A', 'B', 'C', 'D', 'A', 'B', 'C', 'D', 'A', 'B', 'C', 'D', 'A', 'B', 'C', 'D', 'A', 'B', 'C', 'D', 'A', 'B', 'C', 'D', 'A', 'B', 'C', 'D', 'A', 'B', 'C', 'D', 'A', 'B', 'C', 'D', 'A', 'B', 'C', 'D', 'A', 'B', 'C', 'D', 'A', 'B', 'C', 'D', 'A', 'B', 'C', 'D', };
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < wordsearch.Length; i++) 
        {
            GameObject button = Instantiate(buttonPrefab, this.gameObject.transform);
            button.GetComponentInChildren<TMP_Text>().text = wordsearch[i].ToString();
            button.GetComponent<ButtonLetter>().wordSearch = this;
            
        }
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        ray = cam.ScreenPointToRay(Input.mousePosition);
        mousePos = new Vector3(ray.origin.x, ray.origin.y, 0);
    }

    public void ButtonPressed(Transform buttonsTransform)
    {
        Debug.Log("Button Pressed at: " + buttonsTransform.ToString());
        buttonOrigin = buttonsTransform.position;
        
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpriteButton : MonoBehaviour
{
    [SerializeField] private string url;
    public bool shouldOpenUI = false;
    public GameObject ui;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        if (shouldOpenUI == true)
        {
            ui.SetActive(true);
        }
        else
        {
            Debug.Log("Going to quiz website...");
            Application.OpenURL(url);
        }
    }
}

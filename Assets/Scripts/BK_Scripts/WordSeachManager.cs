using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordSeachManager : MonoBehaviour
{
    public string[] WordList = {"APPROVAL", "ETHICS", "IDENTITY", "ONLINE", "PASSWORD",
        "PERSONAL", "POLICY", "PRIVATE", "REGISTER", "SCAM", "SECURITY", "SPAM"};
    public GameObject[] chars;
    
    
    private int iRow;
    private int iCol;


    private void Awake()
    {
        int size = chars.Length -1;
        GameObject[,] wordSearch = new GameObject[size, size];

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

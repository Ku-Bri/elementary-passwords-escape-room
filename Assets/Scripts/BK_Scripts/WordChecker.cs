using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordChecker : MonoBehaviour
{
    public CharObject[] characters;
    private bool isAllTrue = false;
    public TextStrikeThrough wordOnList;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void IsWordComplete()
    {
        isAllTrue = true;
        //Debug.Log("Checking if for is complete in array of length " + characters.Length);
        for (int i = 0;  i <= characters.Length -1; i++)
        {
            //Debug.Log("Checking for each character");
            if (!characters[i].isCharSelected)
            {
                isAllTrue = false;
                break;
            }
        }
        if (isAllTrue)
        {
            for (int j = 0; j <= characters.Length - 1; j++)
            {
                characters[j].TextCompletedColor();
                characters[j]._button.interactable = false;
                wordOnList.LineEnabled();
            }
        }
    }
}

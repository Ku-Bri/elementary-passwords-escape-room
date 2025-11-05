using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterObject : MonoBehaviour
{

    [SerializeField]
    private bool isStartingChar;
    [SerializeField]
    private bool isCharInWord;
    [SerializeField]
    private bool isEndingChar;
    [SerializeField]
    private char character;
    [SerializeField]
    private bool isCharSelected;
    public TextMeshProUGUI _text;
    private Color textOriginalColor = new Color32(255, 255, 255, 255);
    private Color textSelectedColor = new Color32(123, 123, 123, 255);
    private Color textCompletedColor;

    public void CharChecker()
    {
        //Debug.Log("Entered CharChecker");
        isCharSelected = !isCharSelected;
        if (isCharSelected)
        {
            TextSelectedColor();
            
        } else
        {
            TextOriginalColor();
        }


        if (!isCharInWord)
        {

        }
    }

    public void TextOriginalColor()
    {
        _text.color = textOriginalColor;
        Debug.Log("Character changed to white");
    }

    public void TextSelectedColor()
    {
        _text.color = textSelectedColor;
        Debug.Log("Character changed to gray");
    }

    public void TextCompletedColor()
    {
        _text.color = textCompletedColor;
        Debug.Log("Character changed to dark green");
    }
}

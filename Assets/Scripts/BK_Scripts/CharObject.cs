using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class CharObject : MonoBehaviour
{

    /*[SerializeField]
    private bool isStartingChar;
    [SerializeField]
    private bool isCharInWord;
    [SerializeField]
    private bool isEndingChar;
    */
    //public char character;
    public bool isCharSelected = false;
    public TextMeshProUGUI _text;
    public Button _button;

    private Color textOriginalColor = new Color32(255, 255, 255, 255);
    private Color textSelectedColor = new Color32(123, 123, 123, 255);
    private Color textCompletedColor = new Color32(45, 120, 54, 255);

    public void CharChecker()
    {
        //Debug.Log("Entered CharChecker");
        isCharSelected = !isCharSelected;
        Debug.Log(_text.text + " isCharSelected is " + isCharSelected);
        if (isCharSelected)
        {
            TextSelectedColor();

        }
        else
        {
            TextOriginalColor();
        }

        /*
        if (!isCharInWord)
        {

        }
        */
    }

    public void TextOriginalColor()
    {
        _text.color = textOriginalColor;
        //Debug.Log("Character changed to white");
    }

    public void TextSelectedColor()
    {
        _text.color = textSelectedColor;
        //Debug.Log("Character changed to gray");
    }

    public void TextCompletedColor()
    {
        _text.color = textCompletedColor;
        Debug.Log(_text.text + " changed to dark green");
    }

    public bool IsCharSelected()
    {
        return isCharSelected;
    }
}

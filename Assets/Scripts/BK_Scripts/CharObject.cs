using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class CharObject : MonoBehaviour
{

    [Header("Identity")]
    [SerializeField] private string charId;
    public string CharId => charId;

    [Header("State")]
    public bool isCharSelected = false;

    [Header("UI References")]
    public TextMeshProUGUI _text;
    public Button _button;

    private Color textOriginalColor = new Color32(255, 255, 255, 255);
    private Color textSelectedColor = new Color32(123, 123, 123, 255);
    private Color textCompletedColor = new Color32(45, 120, 54, 255);



#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(charId))
        {
            charId = GUID.Generate().ToString();
            EditorUtility.SetDirty(this);
        }
    }
#endif


    public void CharChecker()
    {
        //Debug.Log("Entered CharChecker");

        isCharSelected = !isCharSelected;
        Debug.Log(_text.text + " isCharSelected is " + isCharSelected);

        if (isCharSelected) {TextSelectedColor();}
        else {TextOriginalColor();}
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
        //Debug.Log(_text.text + " changed to dark green");
    }


    private void OnEnable()
    {
        // Respect "completed" state first: if the button is disabled, keep it green.
        if (_button != null && !_button.interactable)
        {
            TextCompletedColor();
            return;
        }

        // Otherwise, selected -> gray, unselected -> white.
        if (isCharSelected) TextSelectedColor();
        else TextOriginalColor();
    }



}

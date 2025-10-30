using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextStrikeThrough : MonoBehaviour
{
    public bool strikeThrough;
    public Image line;
    
    private void Awake()
    {
        strikeThrough = false;
        line = transform.GetChild(1).GetComponent<Image>();
       line.enabled = strikeThrough;
    }
    public void OnStrikeThrough()
    {
        line.enabled =!strikeThrough;
        strikeThrough = !strikeThrough;
    }

}
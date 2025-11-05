using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FlashText_Opacity : MonoBehaviour
{
    public TextMeshProUGUI tmpText;

    // Start is called before the first frame update
    void Start()
    {
        string modifiedText = "";
        for (int i = 0; i < tmpText.text.Length; i++)
        {
            int changeAlpha = Random.Range(25, 99);

            modifiedText += $"<alpha={changeAlpha}>{tmpText.text[i]}</alpha>";
        }
        tmpText.text = modifiedText;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PasswordStrength : MonoBehaviour
{
    private int letterNum = 0;
    public GameObject incorrectUI;
    public GameObject passwordResultUI;
    public TMP_Text option0Text;
    public TMP_Text option1Text;
    public string passcode = "STRONG";
    private string currentPasscode;
    public int[] correctList = { 0, 1, 1, 0, 0, 1 };
    public GameObject continueButton;
    // Start is called before the first frame update
    void Start()
    {
        currentPasscode = passwordResultUI.GetComponent<TMP_Text>().text;
        UpdateOptionUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentPasscode.Contains("G"))
        {
            continueButton.SetActive(true);
        }
    }

    public void PushOption(int option)
    {
        if (option == correctList[letterNum])
        {
            CorrectAnswer();
        }
        else
        {
            IncorrectAnswer();
        }
    }

    void UpdateOptionUI()
    {
        switch (letterNum)
        {
            case 0:
                option0Text.text = "Dino2Jump!Tree9";
                option1Text.text = "password";
                break;
            case 1:
                option0Text.text = "Apple";
                option1Text.text = "Mango$Sled47Wind";
                break;
            case 2:
                option0Text.text = "Password123!";
                option1Text.text = "bjsTs123$$";
                break;
            case 3:
                option0Text.text = "krswim84";
                option1Text.text = "jayhawks";
                break;
            case 4:
                option0Text.text = "Sky@Rocket123Moon";
                option1Text.text = "123456";
                break;
            case 5:
                option0Text.text = "abc123";
                option1Text.text = "Zebra_Run88#Blue";
                break;
        }
    }

    void CorrectAnswer()
    {
        var regex = new Regex(Regex.Escape("_"));
        currentPasscode = regex.Replace(currentPasscode, passcode[letterNum].ToString(), 1);
        Debug.Log(currentPasscode);
        passwordResultUI.GetComponent<TMP_Text>().text = currentPasscode;
        if (letterNum < passcode.Length-1)
        {
            letterNum++;
        }
        UpdateOptionUI();
    }

    void IncorrectAnswer()
    {
        incorrectUI.SetActive(true);
    }
}

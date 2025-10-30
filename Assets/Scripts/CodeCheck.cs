using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CodeCheck : MonoBehaviour
{
    public string levelNameToCheck;
    //private int intCode = 0;
    public TMP_InputField inputField;
    public GameObject incorrectUI;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ContinueButton()
    {
        string temp = inputField.text;
        TryContinue(temp);
        //TryContinue("624");
    }

    public void PreviousButton()
    {
        LoadPreviousScene();
    }

    void TryContinue(string strCode = "Def")
    {
        Debug.Log(levelNameToCheck);
        Debug.Log(strCode);
        switch (levelNameToCheck)
        {
            
            case "SC_Room1":
                Debug.Log(levelNameToCheck);
                Debug.Log(strCode);
                if (strCode.Equals("624"))
                {
                    LoadNextScene();
                    return;
                }
                Debug.LogWarning("Incorrect code, can't advance to the next screen");
                Incorrect();
                return;
            case "HiddenText":
                if (strCode.Equals("PROFILES"))
                {
                    LoadNextScene();
                    return;
                }
                Debug.LogWarning("Incorrect code, can't advance to the next screen");
                Incorrect();
                return;
            case "TestYourKnowledge":
                if (strCode.Equals("3456"))
                {
                    LoadNextScene();
                    return;
                }
                Debug.LogWarning("Incorrect code, can't advance to the next screen");
                Incorrect();
                return;
            case "Directional Lock":
                if (strCode.Equals("UURRD"))
                {
                    LoadNextScene();
                    return;
                }
                Debug.LogWarning("Incorrect code, can't advance to the next screen");
                Incorrect();
                return;
            case "PasswordStrength":
                break;
            default:
                Debug.LogError("Unkown scene");
                break;
        }
    }

    public void Incorrect()
    {
        incorrectUI.SetActive(true);
    }

    private void LoadNextScene()
    {
        if (SceneManager.GetActiveScene().buildIndex < SceneManager.sceneCountInBuildSettings - 1)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {
            Debug.LogError("Can't advance further, no more scenes in build index.");
        }
    }

    private void LoadPreviousScene()
    {
        if (SceneManager.GetActiveScene().buildIndex > 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        }
        else
        {
            Debug.LogError("Can't go back any more, We're already on the first scene");
        }
    }
}

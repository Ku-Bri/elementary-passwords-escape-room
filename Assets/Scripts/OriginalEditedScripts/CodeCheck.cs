using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class CodeCheck : MonoBehaviour
{
    public string levelNameToCheck;
    //private int intCode = 0;
    public TMP_InputField inputField;
    public GameObject incorrectUI;
    public GameObject unlockPanel;
    public float timeToWait;

    void Awake()
    {
        unlockPanel.SetActive(false);
    }
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

        switch (levelNameToCheck)
        {
            
            case "WordSearch":
                Debug.Log(levelNameToCheck);
                Debug.Log(strCode);
                if (strCode.Equals("624"))
                {
                    unlockPanel.SetActive(true);
                    Invoke("LoadNextScene", timeToWait);
                    //LoadNextScene();
                    return;
                }
                Debug.LogWarning("Incorrect code, can't advance to the next screen");
                Incorrect();
                return;
            case "HiddenText":
                if (strCode.ToUpper().Equals("PROFILES"))
                {
                    unlockPanel.SetActive(true);
                    Invoke("LoadNextScene", timeToWait);
                    //LoadNextScene();
                    return;
                }
                Debug.LogWarning("Incorrect code, can't advance to the next screen");
                Incorrect();
                return;
            case "TestYourKnowledge":
                if (strCode.Equals("3456"))
                {
                    unlockPanel.SetActive(true);
                    Invoke("LoadNextScene", timeToWait); 
                    //LoadNextScene();
                    return;
                }
                Debug.LogWarning("Incorrect code, can't advance to the next screen");
                Incorrect();
                return;
            case "Directional Lock":
                if (strCode.ToUpper().Equals("UURRD"))
                {
                    unlockPanel.SetActive(true);
                    Invoke("LoadNextScene", timeToWait);
                    //LoadNextScene();
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

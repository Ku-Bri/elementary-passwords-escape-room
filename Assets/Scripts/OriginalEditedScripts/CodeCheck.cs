using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    // -------- WordPass1 --------
    private string wordPass1FileName = "WordPass1_SceneState.json";
    private string passwordStateFileName = "PasswordSceneState.json";

    private int codeSet;
    private int unlockCode;


    // Primary(runtime) paths
    private string wordPass1SavePath => Path.Combine(Application.persistentDataPath, wordPass1FileName);
    private string passwordStatePath => Path.Combine(Application.persistentDataPath, passwordStateFileName);

    [Serializable]
    private class WordPass1State
    {
        public int codeSet;          // <-- Make sure this matches the JSON field name
    }

    [Serializable]
    private class PasswordSceneState
    {
        public bool scenePreviouslyLoaded;
        public string textList;
        public string passcodes;
        public bool passcodesObjectActive;

        public int code1;
        public int code2;
        public int code3;
        public int slotIndex;
    }

    // ---------------- Helpers to find the file (Editor + Build) ----------------
    private string GetWordPass1Path()
    {
        string p = wordPass1SavePath;
        if (File.Exists(p)) return p;

#if UNITY_EDITOR
        string editorPath = Path.Combine(Application.dataPath, "SaveData", wordPass1FileName);
        if (File.Exists(editorPath)) return editorPath;
#endif
        return p;
    }

    private string GetPasswordStatePath()
    {
        string p = passwordStatePath;
        if (File.Exists(p)) return p;
/*
#if UNITY_EDITOR
        string editorPath = Path.Combine(Application.dataPath, "SaveData", passwordStateFileName);
        if (File.Exists(editorPath)) return editorPath;
#endif*/
        return p;
    }



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
                if (!RetrieveCodeSet())
                {
                    Debug.LogWarning("[CodeCheck] Could not determine codeSet. Aborting check.");
                    Incorrect();
                    return;
                }
                if (!ResolveUnlockCode(codeSet))
                {
                    Debug.LogWarning("[CodeCheck] Could not resolve unlock code from PasswordSceneState. Aborting check.");
                    Incorrect();
                    return;
                }

                if (int.TryParse(strCode, out var entered) && entered == unlockCode)
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


    // ---------------- Read codeSet from WordPass1 ----------------
    private bool RetrieveCodeSet()
    {
        string path = GetWordPass1Path();
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[CodeCheck] WordPass1 file not found at: {path}");
            return false;
        }

        try
        {
            string json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<WordPass1State>(json);
            if (data == null)
            {
                Debug.LogWarning($"[CodeCheck] Could not parse WordPass1 JSON at: {path}");
                return false;
            }

            codeSet = data.codeSet;  // e.g., 1, 2, or 3
            Debug.Log($"[CodeCheck] codeSet = {codeSet} (from {path})");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CodeCheck] Failed reading WordPass1: {ex.Message}");
            return false;
        }
    }

    // ---------------- Resolve unlockCode from PasswordSceneState ----------------
    private bool ResolveUnlockCode(int setIndex)
    {
        string path = GetPasswordStatePath();
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[CodeCheck] PasswordSceneState file not found at: {path}");
            return false;
        }

        try
        {
            string json = File.ReadAllText(path);
            var state = JsonUtility.FromJson<PasswordSceneState>(json);
            if (state == null)
            {
                Debug.LogWarning($"[CodeCheck] Could not parse PasswordSceneState JSON at: {path}");
                return false;
            }

            // Map setIndex → code1/2/3
            switch (setIndex)
            {
                case 1: unlockCode = state.code1; break;
                case 2: unlockCode = state.code2; break;
                case 3: unlockCode = state.code3; break;
                default:
                    Debug.LogWarning($"[CodeCheck] Invalid codeSet '{setIndex}'. Expected 1, 2, or 3.");
                    return false;
            }

            Debug.Log($"[CodeCheck] unlockCode = {unlockCode} (from {path}, set={setIndex})");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CodeCheck] Failed reading PasswordSceneState: {ex.Message}");
            return false;
        }
    }

}

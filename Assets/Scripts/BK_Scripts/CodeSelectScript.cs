using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class CodeSelectScript : MonoBehaviour
{
    public TextMeshProUGUI question;
    private string qText = "What is the PASSCODE the PasswordManager created for you for Set";
    private int codeSet;
    private string saveFileName = "WordPass1_SceneState.json";
    public bool scenePreviouslyLoaded;


    // ---------- File Path ----------
    private string SavePath =>
        Path.Combine(Application.persistentDataPath, saveFileName);


    // ---------- Save Structure ----------
    [System.Serializable]

    public class SaveData
    {
        public bool scenePreviouslyLoaded;
        public string passcodeCheck;
        public int codeSet;
    }


    // ---------- On Scene Start ----------
    private void Start()
    {
        // NEW: Load saved state AFTER setting defaults
        if (File.Exists(SavePath))
        {
            LoadState();
            Debug.Log("LoadState() called from Start()"); // <-- Now safe to restore without overwriting initialization
        }
        else
        {
            scenePreviouslyLoaded = false;
            codeSet = Random.Range(1, 4);
            Debug.Log("CodeSet create and is set to " + codeSet);
            question.text = qText + codeSet + " ?";
            SaveState();
        }

        if (!scenePreviouslyLoaded)
        {
            scenePreviouslyLoaded = true;
        }
    }


    // ---------- On Scene Exit → SAVE ----------
    private void OnDisable()
    {


        if (!gameObject.scene.isLoaded) return;  // prevents saving on scene unload
        if (SaveQuitGuard.IsQuitting) return;
        SaveState();


    }

    public void SaveState()
    {

        Debug.Log(">>> SaveState() CALLED <<<");

        SaveData data = new SaveData();

        data.scenePreviouslyLoaded = scenePreviouslyLoaded;
        data.passcodeCheck = question.text;
        data.codeSet = codeSet;
        Debug.Log("CodeSet saved as " + data.codeSet);
        
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);


        Debug.Log(">>> Save COMPLETE at: " + SavePath + " <<<");
        Debug.Log("JSON SAVED:\n" + json);
    }

    // ---------- Load ----------
    private void LoadState()
    {
        Debug.Log(">>> LoadState() CALLED <<<");


        if (!File.Exists(SavePath))
        {
            Debug.Log(">>> NO SAVE FILE FOUND at: " + SavePath);
            return;
        }

        string json = File.ReadAllText(SavePath);

        Debug.Log("JSON LOADED:\n" + json);

        SaveData data = JsonUtility.FromJson<SaveData>(json);

        scenePreviouslyLoaded = data.scenePreviouslyLoaded;
        question.text = data.passcodeCheck;
        codeSet = data.codeSet;
        Debug.Log("CodeSet loaded as " + codeSet);

        Debug.Log("Loaded PasswordManager state ← " + SavePath);
    }
}

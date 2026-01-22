
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PasswordManager1 : MonoBehaviour
{
    public SceneMovement sm;
    private List<string> pm = new List<string>();
    private int slotIndex = 0;

    public string saveFileName = "PasswordSceneState.json";

    public TextMeshProUGUI textList;
    public GameObject passcodesObject;
    public TextMeshProUGUI passcodes;

    private int code1;
    private int code2;
    private int code3;

    private bool scenePreviouslyLoaded = false;
    private bool userHasAddedPasswords = false;


    public GameObject sceneObjects;
    public GameObject notebook;
    public GameObject textFile;
    public GameObject stickyNote;

    public bool sceneObjectsActive;
    public bool notebookActive;
    public bool textFileActive;
    public bool stickyNoteActive;


    // ---------- File Path ----------
    private string SavePath =>
        Path.Combine(Application.persistentDataPath, saveFileName);

    // ---------- Save Structure ----------
    [System.Serializable]

    public class SaveData
    {
        public bool scenePreviouslyLoaded;
        public bool userHasAddedPasswords;

        public string textList;
        public string passcodes;
        public bool passcodesObjectActive;

        public int code1;
        public int code2;
        public int code3;

        public int slotIndex;

        public bool sceneObjectsActive;
        public bool notebookActive;
        public bool textFileActive;
        public bool stickyNoteActive;
        
    }


    // ---------- On Scene Start ----------

    private void Start()
    {
        // Your original UI setup
        textList.text = "<size=50><align=\"center\">_SECURED PASSWORDS_</align>\n\n";
        passcodes.text = "<size=45><align=\"center\">\n\nPASSCODES</align>\n";
        passcodesObject.SetActive(false);

        // NEW: Load saved state AFTER setting defaults
        if (File.Exists(SavePath))
        {
            LoadState();   // <-- Now safe to restore without overwriting initialization
        }
        else
        {
            scenePreviouslyLoaded = false;
        }

        if (!scenePreviouslyLoaded)
        {
            scenePreviouslyLoaded = true;
            createRandomNums();
        }
    }


    private void createRandomNums()
    {
        code1 = Random.Range(1000, 9999);
        code2 = Random.Range(1000, 9999);
        code3 = Random.Range(1000, 9999);
    }

    // ---------- Populate UI ----------
    public void Populate(string website, string username, string password)
    {
        userHasAddedPasswords = true;

        pm.Add(website);
        pm.Add(username);
        pm.Add(password);

        textList.text +=
            "<size=45>Set " + (slotIndex + 1) + "</size>\n" +
            "<size=35>" + website + "</size>\n" +
            "<size=35>" + username + "</size>\n" +
            "<size=35>" + password + "</size>\n\n";

        slotIndex++;

        if (slotIndex == 3)
        {
            passcodesObject.SetActive(true);
            passcodes.text =
                $"<size=45><align=\"center\">Set1</align></size>\n<size=35><align=\"center\">{code1}</align></size>\n\n" +
                $"<size=45><align=\"center\">Set2</align></size>\n<size=35><align=\"center\">{code2}</align></size>\n\n" +
                $"<size=45><align=\"center\">Set3</align></size>\n<size=35><align=\"center\">{code3}</align></size>";
        }
    }

    // ---------- On Scene Exit → SAVE ----------
    private void OnDisable()
    {
        SaveState();
    }

    public void SaveState()
    {

        Debug.Log(">>> SaveState() CALLED <<<");

        SaveData data = new SaveData();

        data.scenePreviouslyLoaded = scenePreviouslyLoaded;
        data.userHasAddedPasswords = userHasAddedPasswords;
        data.textList = textList.text;
        data.passcodes = passcodes.text;
        data.passcodesObjectActive = passcodesObject.activeSelf;

        data.code1 = code1;
        data.code2 = code2;
        data.code3 = code3;
        data.slotIndex = slotIndex;


        //data.sceneObjectsActive = sceneObjects != null && sceneObjects.activeSelf;
        /*data.notebookActive = notebook != null && notebook.activeSelf;
        data.textFileActive = textFile != null && textFile.activeSelf;
        data.stickyNoteActive = stickyNote != null && stickyNote.activeSelf;
        */
        data.sceneObjectsActive = sceneObjects.activeSelf;
        data.notebookActive = notebook.activeSelf;
        data.textFileActive = textFile.activeSelf;
        data.stickyNoteActive = stickyNote.activeSelf;
        
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);


        Debug.Log(">>> Save COMPLETE at: " + SavePath + " <<<");
        Debug.Log("JSON SAVED:\n" + json);

        sm.LoadNextScene();
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
        userHasAddedPasswords = data.userHasAddedPasswords;

        if (userHasAddedPasswords)
        {
            textList.text = data.textList;
            passcodes.text = data.passcodes;
            passcodesObject.SetActive(data.passcodesObjectActive);

            code1 = data.code1;
            code2 = data.code2;
            code3 = data.code3;

            slotIndex = data.slotIndex;
        }


        //if (sceneObjects != null) sceneObjects.SetActive(data.sceneObjectsActive);
        if (notebook != null) notebook.SetActive(data.notebookActive);
        if (textFile != null) textFile.SetActive(data.textFileActive);
        if (stickyNote != null) stickyNote.SetActive(data.stickyNoteActive);
        
        Debug.Log("Loaded PasswordManager state ← " + SavePath);
    }
}

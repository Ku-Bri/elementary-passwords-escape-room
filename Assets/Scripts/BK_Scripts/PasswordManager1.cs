
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    private bool scenePreviouslyLoaded;
    private bool userHasAddedPasswords;


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

        public Vector3 notebookPos;
        public Vector3 textFilePos;
        public Vector3 stickyNotePos;

        public int activePanelIndex = -1; // -1 means no panel open
}


    // ---------- On Scene Start ----------

    private void Start()
    {
        if (File.Exists(SavePath))
        {
            LoadState();   // <-- Now safe to restore without overwriting initialization
        }
        else
        {
            scenePreviouslyLoaded = false;
            userHasAddedPasswords = false;
            // Your original UI setup
            textList.text = "<align=\"center\">_SECURED PASSWORDS_</align>\n\n";
            passcodes.text = "<align=\"center\">\n\nPASSCODES</align>\n";
            passcodesObject.SetActive(false);
        }

        if (!scenePreviouslyLoaded)
        {
            scenePreviouslyLoaded = true;
            createRandomNums();
        }
    }

    //Creates the random codes/pins and set the passcode text to them. 
    private void createRandomNums()
    {
        code1 = Random.Range(1000, 9999);
        code2 = Random.Range(1000, 9999);
        code3 = Random.Range(1000, 9999);
        passcodes.text +=
                $"<align=\"center\">Set1</align>\n<align=\"center\">{code1}</align>\n\n" +
                $"\n<align=\"center\">Set2</align>\n<align=\"center\">{code2}</align>\n\n" +
                $"\n\n<align=\"center\">Set3</align>\n<align=\"center\">{code3}</align>";
    }

    // ---------- Populate UI ----------
    public void Populate(string website, string username, string password)
    {
        userHasAddedPasswords = true;

        pm.Add(website);
        pm.Add(username);
        pm.Add(password);

        textList.text +=
            "Set " + (slotIndex + 1) + "\n" +
             website  + "\n" +
             username  + "\n" +
             password + "\n\n";

        slotIndex++;

        if (slotIndex == 3)
        {
            passcodesObject.SetActive(true);
            
        }
    }

    // ---------- On Scene Exit → SAVE ----------
    private void OnDisable()
    {

        if (SaveQuitGuard.IsQuitting) return;   // <-- do NOT save when app/editor is quitting
        SaveState();

    }

    public void SaveState()
    {

        //Debug.Log(">>> SaveState() CALLED <<<");

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

        data.notebookPos = notebook.transform.position;
        data.textFilePos = textFile.transform.position;
        data.stickyNotePos = stickyNote.transform.position;

        var po = Object.FindObjectsOfType<PanelOperatorMulti>(true).FirstOrDefault();
        if (po != null)
        {
            for (int i = 0; i < po.isPanelActiveArray.Length; i++)
            {
                if (po.isPanelActiveArray[i])
                {
                    data.activePanelIndex = i;
                    break;
                }
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);


        //Debug.Log(">>> Save COMPLETE at: " + SavePath + " <<<");
        //Debug.Log("JSON SAVED:\n" + json);

        sm.LoadNextScene();
    }

    // ---------- Load ----------
    private void LoadState()
    {
        //Debug.Log(">>> LoadState() CALLED <<<");


        if (!File.Exists(SavePath))
        {
            //Debug.Log(">>> NO SAVE FILE FOUND at: " + SavePath);
            return;
        }

        string json = File.ReadAllText(SavePath);

        //Debug.Log("JSON LOADED:\n" + json);

        SaveData data = JsonUtility.FromJson<SaveData>(json);

        scenePreviouslyLoaded = data.scenePreviouslyLoaded;
        userHasAddedPasswords = data.userHasAddedPasswords;

        textList.text = data.textList;
        passcodes.text = data.passcodes; passcodesObject.SetActive(data.passcodesObjectActive);
       
        code1 = data.code1;
        code2 = data.code2;
        code3 = data.code3;

        slotIndex = data.slotIndex;

        if (userHasAddedPasswords)
        {
            
        }

        notebook.transform.position = data.notebookPos;
        textFile.transform.position = data.textFilePos;
        stickyNote.transform.position = data.stickyNotePos;

        //if (sceneObjects != null) sceneObjects.SetActive(data.sceneObjectsActive);
        if (notebook != null) notebook.SetActive(data.notebookActive);
        if (textFile != null) textFile.SetActive(data.textFileActive);
        if (stickyNote != null) stickyNote.SetActive(data.stickyNoteActive);

        StartCoroutine(RestorePanelNextFrame(data.activePanelIndex));

        //Debug.Log("Loaded PasswordManager state ← " + SavePath);
    }


    private System.Collections.IEnumerator RestorePanelNextFrame(int activePanelIndex)
    {
        yield return null; // let PanelOperatorMulti.Awake/Start run first

        // Restore previously active panel
        var po = Object.FindObjectsOfType<PanelOperatorMulti>(true).FirstOrDefault();
        if (po != null)
        {
            for (int j = 0; j < po.panelArray.Length; j++)
                po.SetPanelInactive(j);

            if (activePanelIndex >= 0 && activePanelIndex < po.panelArray.Length)
                po.SetPanelActive(activePanelIndex);
            else
                po.UnhideSceneObjects();
        }
    }

}

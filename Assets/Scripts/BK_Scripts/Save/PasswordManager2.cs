
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PasswordManager2 : MonoBehaviour
{
    private List<string> pm = new List<string>();
    private int slotIndex = 0;

    public TextMeshProUGUI textList;
    public GameObject passcodesObject;
    public TextMeshProUGUI passcodes;

    private int code1;
    private int code2;
    private int code3;

    private bool scenePreviouslyLoaded = false;

    // ---------- File Path ----------
    private string SavePath =>
        Path.Combine(Application.persistentDataPath, "PasswordSceneState.json");

    // ---------- Save Structure ----------
    [System.Serializable]

    public class SaveData
    {
        public bool scenePreviouslyLoaded;
        public bool hasUserAddedPasswords;

        public string textList;
        public string passcodes;
        public bool passcodesObjectActive;

        public int code1;
        public int code2;
        public int code3;

        public int slotIndex;
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

    private void SaveState()
    {
        SaveData data = new SaveData();

        data.scenePreviouslyLoaded = scenePreviouslyLoaded;
        data.textList = textList.text;
        data.passcodes = passcodes.text;
        data.passcodesObjectActive = passcodesObject.activeSelf;

        data.code1 = code1;
        data.code2 = code2;
        data.code3 = code3;
        data.slotIndex = slotIndex;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);

        Debug.Log("Saved PasswordManager state → " + SavePath);
    }

    // ---------- Load ----------
    private void LoadState()
    {
        string json = File.ReadAllText(SavePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        scenePreviouslyLoaded = data.scenePreviouslyLoaded;

        textList.text = data.textList;
        passcodes.text = data.passcodes;
        passcodesObject.SetActive(data.passcodesObjectActive);

        code1 = data.code1;
        code2 = data.code2;
        code3 = data.code3;

        slotIndex = data.slotIndex;

        Debug.Log("Loaded PasswordManager state ← " + SavePath);
    }
}

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class HiddenTextSceneStateManager : MonoBehaviour
{
    [System.Serializable]
    public class SavedObject
    {
        public string path;   // hierarchical path (unique for parents or children)
        public bool active;
        public string text;
    }

    [System.Serializable]
    public class SaveFile
    {
        public List<SavedObject> objects = new List<SavedObject>();
    }

    public GameObject[] objectsToSave; // drag your parents & children here

    private string SavePath =>
        Path.Combine(Application.persistentDataPath, SceneManager.GetActiveScene().name + "_state.json");

    // ------------------ LIFECYCLE ------------------
    private void Start()
    {
        LoadState(); // Load on entering scene
    }

    private void OnDisable()
    {
        SaveState(); // Save when leaving scene
    }

    // ------------------ SAVE ------------------
    public void SaveState()
    {
        SaveFile file = new SaveFile();

        foreach (var obj in objectsToSave)
        {
            if (obj == null) continue;

            SavedObject saved = new SavedObject();
            saved.path = GetPath(obj.transform);
            saved.active = obj.activeSelf;

            TMP_Text tmp = obj.GetComponentInChildren<TMP_Text>(true);
            saved.text = tmp ? tmp.text : null;

            file.objects.Add(saved);
        }

        string json = JsonUtility.ToJson(file, true);
        File.WriteAllText(SavePath, json);

        Debug.Log("Saved state → " + SavePath);
    }

    // ------------------ LOAD ------------------
    public void LoadState()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("No save file found → using Unity defaults.");
            return;
        }

        string json = File.ReadAllText(SavePath);
        SaveFile file = JsonUtility.FromJson<SaveFile>(json);

        foreach (var saved in file.objects)
        {
            Transform t = FindByPath(saved.path);
            if (t == null) continue;

            GameObject obj = t.gameObject;

            obj.SetActive(saved.active);

            TMP_Text tmp = obj.GetComponentInChildren<TMP_Text>(true);
            if (tmp && saved.text != null)
                tmp.text = saved.text;
        }

        Debug.Log("Loaded state ← " + SavePath);
    }

    // ------------------ HELPERS ------------------
    private string GetPath(Transform t)
    {
        return t.parent == null ? t.name : GetPath(t.parent) + "/" + t.name;
    }

    private Transform FindByPath(string path)
    {
        string[] parts = path.Split('/');
        Transform current = null;

        foreach (string part in parts)
        {
            if (current == null)
            {
                GameObject rootObj = GameObject.Find(part);
                if (rootObj == null) return null;
                current = rootObj.transform;
            }
            else
            {
                current = current.Find(part);
                if (current == null) return null;
            }
        }

        return current;
    }
}

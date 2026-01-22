using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private GameObject EventSystem;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(this.gameObject);
        SceneManager.activeSceneChanged += ChangedActiveScene;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void ChangedActiveScene(Scene current, Scene next)
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            Debug.Log("No event system found, adding one...");
            Instantiate(EventSystem);
        }
        GameObject.Find("Title").GetComponent<TMP_Text>().text = SceneManager.GetActiveScene().name;
    }




    // ========== CLEANUP: delete save file on quit / stop ==========
    private const string SaveFileName = "PasswordSceneState.json";
    private const string SaveFileName1 = "WordPass1_SceneState.json";

#if UNITY_EDITOR
    private void OnEnable()
    {
        // Keep your existing subscriptions; add this if not present:
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged_DeleteSave;
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged_DeleteSave;
    }

    // IMPORTANT: delete AFTER play mode has fully stopped, not on ExitingPlayMode.
    private void OnPlayModeStateChanged_DeleteSave(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            // Mark quitting so OnDisable() in scene objects won't save again.
            SaveQuitGuard.IsQuitting = true;
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            TryDeleteSaveFile();   // Now it's safe to delete—nothing else will re-save.
        }
    }
#endif

    private void OnApplicationQuit()
    {
        // Mark quitting so OnDisable() won’t save in builds.
        SaveQuitGuard.IsQuitting = true;
        TryDeleteSaveFile();
    }

    private void TryDeleteSaveFile()
    {
        // Delete from persistentDataPath (where your PasswordManager1 writes)
        var persistentPath = Path.Combine(Application.persistentDataPath, SaveFileName);
        var persistentPath1 = Path.Combine(Application.persistentDataPath, SaveFileName1);
        SafeDelete(persistentPath, "[GameManager] Deleted save file: ", "[GameManager] Could not delete save file: ");
        SafeDelete(persistentPath1, "[GameManager] Deleted save file: ", "[GameManager] Could not delete save file: ");

#if UNITY_EDITOR
        // If you sometimes save copies in Assets/SaveData while testing, remove those too.
        var editorPath = Path.Combine(Application.dataPath, "SaveData", SaveFileName);
        var editorPath1 = Path.Combine(Application.dataPath, "SaveData", SaveFileName1);
        SafeDelete(editorPath, "[GameManager] Deleted editor save file: ", "[GameManager] Could not delete editor save file: ");
        SafeDelete(editorPath1, "[GameManager] Deleted editor save file: ", "[GameManager] Could not delete editor save file: ");
#endif
    }

    private void SafeDelete(string path, string okMsg, string failMsg)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log(okMsg + path);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"{failMsg}{path} ({ex.Message})");
        }
    }

}

public static class SaveQuitGuard
{
    public static bool IsQuitting { get; set; } = false;
}

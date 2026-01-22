using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private GameObject EventSystem;
    public PasswordManager1 pm;

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
        /*pm = FindObjectOfType<PasswordManager1>();

        if (pm != null)
        {
            pm.SaveState();
        }
        */

        if (FindObjectOfType<EventSystem>() == null)
        {
            Debug.Log("No event system found, adding one...");
            Instantiate(EventSystem);
        }
        GameObject.Find("Title").GetComponent<TMP_Text>().text = SceneManager.GetActiveScene().name;
    }


    // ------------------------- App End Cleanup -------------------------
    private void OnApplicationQuit()
    {
        TryDeleteSaveFile();
    }

    private void TryDeleteSaveFile()
    {
        // Always delete the save in persistentDataPath
        var persistentPath = Path.Combine(Application.persistentDataPath, pm.saveFileName);
        try
        {
            if (File.Exists(persistentPath))
            {
                File.Delete(persistentPath);
                Debug.Log($"[GameManager] Deleted save file: {persistentPath}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[GameManager] Could not delete {persistentPath}: {ex.Message}");

        }
    }
}

using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneStateManager : MonoBehaviour
{
    private static SceneStateManager instance;

    private string filePath;

    // Called when the script instance is being loaded
    void Awake()
    {
        // Implementing Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persist this GameObject across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy any duplicate instance
        }
    }

    void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, "sceneState.json");
        SceneManager.sceneUnloaded += OnSceneUnloaded; // Subscribe to scene unload event
        LoadSceneState(); // Load previous state when this is initialized
    }

    void OnSceneUnloaded(Scene scene)
    {
        SaveSceneState(); // Save the state when the scene is unloaded
    }

    [System.Serializable]
    public class SceneData
    {
        public GameObjectData[] objects;
    }

    [System.Serializable]
    public class GameObjectData
    {
        public string name;
        public string text; // For TextMeshPro objects
        public bool isActive;
    }

    public void SaveSceneState()
    {
        SceneData sceneData = new SceneData();
        GameObject[] gameObjects = FindObjectsOfType<GameObject>();

        sceneData.objects = new GameObjectData[gameObjects.Length];

        for (int i = 0; i < gameObjects.Length; i++)
        {
            GameObjectData data = new GameObjectData
            {
                name = gameObjects[i].name,
                isActive = gameObjects[i].activeSelf
            };

            TextMeshPro tmp = gameObjects[i].GetComponent<TextMeshPro>();
            if (tmp != null)
            {
                data.text = tmp.text;
            }

            sceneData.objects[i] = data;
        }

        string json = JsonUtility.ToJson(sceneData, true);
        File.WriteAllText(filePath, json);
    }

    public void LoadSceneState()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            SceneData sceneData = JsonUtility.FromJson<SceneData>(json);

            foreach (var objectData in sceneData.objects)
            {
                GameObject obj = GameObject.Find(objectData.name);
                if (obj != null)
                {
                    obj.SetActive(objectData.isActive);

                    TextMeshPro tmp = obj.GetComponent<TextMeshPro>();
                    if (tmp != null)
                    {
                        tmp.text = objectData.text;
                    }
                }
            }
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded; // Clean up event subscription
    }
}

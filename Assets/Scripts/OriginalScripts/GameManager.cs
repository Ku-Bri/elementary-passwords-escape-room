using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

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
}

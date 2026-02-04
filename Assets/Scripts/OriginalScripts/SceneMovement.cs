using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SceneMovement : MonoBehaviour
{
    // Start is called before the first frame update
    private void Awake()
    {
        //DontDestroyOnLoad(GameObject.FindGameObjectWithTag("NavBar"));
        //DontDestroyOnLoad(FindObjectOfType<EventSystem>());
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadNextScene()
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

    public void LoadPreviousScene()
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
}

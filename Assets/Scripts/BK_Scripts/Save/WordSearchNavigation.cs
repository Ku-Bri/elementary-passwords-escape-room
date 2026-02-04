using UnityEngine;

public class WordSearchNavigation : MonoBehaviour
{
    public SceneMovement sceneMovement; // drag your existing SceneMovement here

    // Wire your Word Search "Next" button to this.
    public void SaveThenNext()
    {
        SaveSystem.SaveWordSearch();
        if (sceneMovement != null) sceneMovement.LoadNextScene();
        else Debug.LogWarning("[WordSearchNavigation] SceneMovement reference missing.");
    }
}

using UnityEngine;

public class WordSearchStateBootstrapper : MonoBehaviour
{
    private void Start()
    {
        // When the Word Search scene opens (first time or after Back), restore state if present.
        SaveSystem.LoadWordSearch();
    }
}
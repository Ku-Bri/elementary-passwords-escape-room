using UnityEngine;

public class PanelOperatorImproved : MonoBehaviour
{
    [Header("Panels (0 = Instructions, 1 = Hint, etc.)")]
    public GameObject[] panelArray;

    [Header("Puzzle root to hide/show (GridRoot)")]
    public GameObject sceneObjects; // equivalent to your old PanelOperator.sceneObjects [1](https://univoftulsa-my.sharepoint.com/personal/bridget-kurr_utulsa_edu1/Documents/Microsoft%20Copilot%20Chat%20Files/SceneMovement.cs)

    [Header("Directional Lock Manager (so we can force-close choice panel)")]
    public DirectionalLockManagerChoice puzzleManager;

    [Header("Start State")]
    public bool showInstructionsOnStart = true;
    public int instructionsPanelIndex = 0;

    private bool[] isPanelActive;

    private void Awake()
    {
        isPanelActive = new bool[panelArray.Length];

        // Start with all panels hidden
        for (int i = 0; i < panelArray.Length; i++)
        {
            isPanelActive[i] = false;
            if (panelArray[i] != null) panelArray[i].SetActive(false);
        }
    }

    private void Start()
    {
        if (showInstructionsOnStart && panelArray.Length > instructionsPanelIndex)
        {
            isPanelActive[instructionsPanelIndex] = true;
            panelArray[instructionsPanelIndex].SetActive(true);
        }

        UpdatePuzzleVisibility();
    }

    public void ShowPanel(int i)
    {
        // Toggle requested panel
        isPanelActive[i] = !isPanelActive[i];
        panelArray[i].SetActive(isPanelActive[i]);

        // Optional: enforce only one panel open at a time (matches your current flow) [1](https://univoftulsa-my.sharepoint.com/personal/bridget-kurr_utulsa_edu1/Documents/Microsoft%20Copilot%20Chat%20Files/SceneMovement.cs)
        if (isPanelActive[i])
        {
            for (int j = 0; j < panelArray.Length; j++)
            {
                if (j == i) continue;
                isPanelActive[j] = false;
                panelArray[j].SetActive(false);
            }
        }

        UpdatePuzzleVisibility();
    }

    private void UpdatePuzzleVisibility()
    {
        bool anyPanelOpen = false;
        for (int j = 0; j < isPanelActive.Length; j++)
        {
            if (isPanelActive[j]) { anyPanelOpen = true; break; }
        }

        // Hide/show puzzle root
        if (sceneObjects != null)
            sceneObjects.SetActive(!anyPanelOpen);

        // If any overlay is open, force-close choice panel and re-enable grid interaction
        if (anyPanelOpen && puzzleManager != null)
            puzzleManager.ForceCloseChoicePanelAndEnableGrid();
    }
}
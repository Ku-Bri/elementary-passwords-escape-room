using UnityEngine;

public class PanelOperatorRevised : MonoBehaviour
{
    [Header("Panels")]
    public GameObject instructionsPanel;
    public GameObject hintPanel;
    public GameObject questionsPanel;

    // 0 = none (Questions), 1 = Instructions, 2 = Hint
    [SerializeField] private int activeOverlay = 1;

    private void Awake()
    {
        ApplyOverlayState(activeOverlay);
    }

    public void ToggleInstructions()
    {
        activeOverlay = (activeOverlay == 1) ? 0 : 1;
        ApplyOverlayState(activeOverlay);
    }

    public void ToggleHint()
    {
        activeOverlay = (activeOverlay == 2) ? 0 : 2;
        ApplyOverlayState(activeOverlay);
    }

    public int GetActiveOverlayIndex() => activeOverlay;

    public void RestoreActiveOverlayIndex(int overlayIndex)
    {
        if (overlayIndex < 0 || overlayIndex > 2) overlayIndex = 0;
        activeOverlay = overlayIndex;
        ApplyOverlayState(activeOverlay);
    }

    public void ForceShowInstructions()
    {
        activeOverlay = 1;
        ApplyOverlayState(activeOverlay);
    }

    private void ApplyOverlayState(int overlayIndex)
    {
        bool showInstructions = overlayIndex == 1;
        bool showHint = overlayIndex == 2;
        bool showQuestions = overlayIndex == 0;

        if (instructionsPanel != null) instructionsPanel.SetActive(showInstructions);
        if (hintPanel != null) hintPanel.SetActive(showHint);
        if (questionsPanel != null) questionsPanel.SetActive(showQuestions);
    }
}
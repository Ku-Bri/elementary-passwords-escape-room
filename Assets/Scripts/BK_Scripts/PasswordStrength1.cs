using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class PasswordStrength1 : MonoBehaviour
{
    [Header("Data Source (ScriptableObject)")]
    [SerializeField] private QuestionData passwordSet;        // Drag your QuestionData asset (Password Set Rm 5)

    [Header("UI: Core Game")]
    [SerializeField] private TMP_Text passwordResultText;     // Cryptext label; starts with underscores, e.g., "______"
    [SerializeField] private TMP_Text option0Text;            // TMP on Option 0 button
    [SerializeField] private TMP_Text option1Text;            // TMP on Option 1 button
    [SerializeField] private GameObject passwordSelectionPanel; // Parent of the two option buttons/labels

    [Header("UI: Feedback Panels")]
    [SerializeField] private GameObject correctPanel;         // Briefly shown after a correct answer
    [SerializeField] private GameObject wrongPanel;           // Briefly shown after a wrong answer
    [SerializeField] private TMP_Text wrongPanelMessage;      // Optional text inside wrongPanel
    [SerializeField] private GameObject completePanel;        //Show and stay on when gameCompleted == true

    public AudioSource winnerAudioClip;

    // NOTE: Info panels exist in your scene, but per request we do not toggle them here anymore.
    // [Header("UI: Info Panels (Toggles)")]
    // [SerializeField] private GameObject instructionsPanel; // (Removed usage)
    // [SerializeField] private GameObject hintPanel;         // (Removed usage)

    [Header("Covers (Per-Letter)")]
    [Tooltip("Parent object containing one cover image per passcode slot.")]
    [SerializeField] private GameObject codeCoverGroup;
    [Tooltip("CanvasGroups for each cover (left-to-right). Count should match passcode length.")]
    [SerializeField] private List<CanvasGroup> codeCoverSlots = new List<CanvasGroup>();
    private bool gameCompleted = false;

    [Header("Blink Settings")]
    [SerializeField, Range(0f, 1f)] private float blinkMinAlpha = 0.35f;
    [SerializeField, Range(0f, 1f)] private float blinkMaxAlpha = 1.00f;
    [SerializeField, Range(0.1f, 10f)] private float blinkSpeed = 4f;

    [Header("Gameplay")]
    [SerializeField] private string passcode = "STRONG";      // Revealed one letter per correct answer
    [SerializeField] private int maxWrong = 3;                // After 3 wrong, reset progress

    [Header("Panel Durations (seconds)")]
    [SerializeField, Min(0.1f)] private float correctPanelDuration = 0.9f;
    [SerializeField, Min(0.1f)] private float wrongPanelDuration = 1.4f;

    // --- internal state ---
    private string currentPasscode;
    private int letterNum = 0;                // index into passcode [0..passcode.Length-1]
    private int correctOptionIndex = 0;       // which option currently holds the strong answer (0 or 1)
    private int wrongCount = 0;

    // pools from ScriptableObject
    private readonly List<TrueFalsQuestion> strongAll = new();
    private readonly List<TrueFalsQuestion> weakAll = new();

    // queues (shuffled, then dequeued; refilled when empty)
    private readonly Queue<TrueFalsQuestion> strongQ = new();
    private readonly Queue<TrueFalsQuestion> weakQ = new();

    private System.Random rng;

    // blinking / covers
    private int activeBlinkIndex = -1;
    private Coroutine blinkRoutine;

    // reveal: whether the covers are hidden by the Reveal Code toggle
    private bool coversHiddenViaReveal = false; // start shown; Reveal toggles to hide

    // (Removed) Whether a feedback panel is showing — we no longer gate by this in this script.
    // private bool isFeedbackActive = false;

    private void Awake()
    {
        rng = new System.Random();

        // CHANGED: Only control Correct/Wrong panels here. Do not toggle Instructions/Hint/Selection.
        if (correctPanel != null) correctPanel.SetActive(false);
        if (wrongPanel != null) wrongPanel.SetActive(false);

        // Covers group exists but starts hidden; Reveal Code button will show/hide it
        if (codeCoverGroup != null) codeCoverGroup.SetActive(true);
        //HideAllCovers();

        completePanel.SetActive(false);
    }

    private void Start()
    {
        /* // Cryptext init
         var underscores = new string('_', passcode.Length);
         if (passwordResultText != null)
         {
             if (string.IsNullOrWhiteSpace(passwordResultText.text))
                 passwordResultText.text = underscores;
             currentPasscode = passwordResultText.text;
         }
         else
         {
             currentPasscode = underscores;
         }*/

        // Cryptext init — start EMPTY, no underscores
        currentPasscode = string.Empty;
        if (passwordResultText != null)
            passwordResultText.text = currentPasscode;

        BuildPoolsFromSO();
        RefillQueuesIfNeeded();
        NextRound(); // prepares the first pair

        // Covers start enabled; show past=solid, current=blinking, future=hidden
        if (codeCoverGroup != null) codeCoverGroup.SetActive(true);
        InitCoversHiddenExceptActive(letterNum);

        // CHANGED: Removed any top-level panel visibility control.
        // RefreshTopLevelPanels();
    }

    // ==================== PUBLIC UI HOOKS ====================

    /// <summary>Option buttons: Option 0 -> PushOption(0), Option 1 -> PushOption(1)</summary>
    public void PushOption(int optionIndex)
    {

        if (gameCompleted) return;

        // Read-only check is fine; we no longer toggle this panel in this script.
        if (passwordSelectionPanel != null && !passwordSelectionPanel.activeSelf) return;

        if (optionIndex == correctOptionIndex) HandleCorrect();
        else HandleIncorrect();
    }

    /// <summary>Reveal Code button: toggles the COVER visibility only (not the selection panel)</summary>
    public void OnRevealCode()
    {
        coversHiddenViaReveal = !coversHiddenViaReveal;

        if (codeCoverGroup == null) return;

        // CHANGED: Do not gate on feedback anymore; overlays sit on top.
        bool canShowCovers = !coversHiddenViaReveal &&
                             (passwordSelectionPanel == null || passwordSelectionPanel.activeSelf);

        codeCoverGroup.SetActive(canShowCovers);

        if (canShowCovers)
        {
            if (gameCompleted)
            {
                for (int i = 0; i < codeCoverSlots.Count; i++)
                {
                    var cg = codeCoverSlots[i];
                    if (cg != null)
                    {
                        cg.gameObject.SetActive(true);
                        cg.alpha = 1f;
                    }
                }
                StopBlink();
                return; // do not blink once game is completed
            }
            else
            {
                // Normal play: show past solid, current blinking, future hidden
                InitCoversHiddenExceptActive(letterNum);
            }
        }
        else
        {
            StopBlink();
            HideAllCovers();
        }

    }

    // (Removed) Info panel toggles per request:
    // public void OnToggleInstructions() {...}
    // public void OnToggleHint() {...}

    // ==================== ROUND FLOW ====================

    private void NextRound()
    {
        RefillQueuesIfNeeded();

        // One strong + one weak (consumed from queues)
        var strong = strongQ.Dequeue();
        var weak = weakQ.Dequeue();

        // Random placement of the strong answer
        correctOptionIndex = rng.Next(0, 2);

        // Re-enable Auto Size for new content
        option0Text.enableAutoSizing = true;
        option1Text.enableAutoSizing = true;

        if (correctOptionIndex == 0)
        {
            option0Text.text = strong.questionText;
            option1Text.text = weak.questionText;
        }
        else
        {
            option0Text.text = weak.questionText;
            option1Text.text = strong.questionText;
        }

        // Sync sizes only if selection is visible (otherwise we’ll sync next time it becomes visible)
        if (passwordSelectionPanel == null || passwordSelectionPanel.activeSelf)
            StartCoroutine(SyncOptionFontSizesNextFrame());
    }

    private void HandleCorrect()
    {
        /*// Reveal next character (replace first underscore)
        if (passwordResultText != null)
        {
            var regex = new Regex(Regex.Escape("_"));
            currentPasscode = regex.Replace(currentPasscode, passcode[letterNum].ToString(), 1);
            passwordResultText.text = currentPasscode;
        }*/


        // Safety guard—do nothing if already complete or index out of range
        if (gameCompleted || letterNum >= passcode.Length) return;

        // Append next letter with a leading space (no underscores at all)
        if (passwordResultText != null)
        {
            // First letter = no leading space; subsequent letters = " " + letter
            string next = passcode[letterNum].ToString();
            currentPasscode = (letterNum == 0) ? next : $"{currentPasscode} {next}";
            passwordResultText.text = currentPasscode;
        }

        // CHANGED: Do NOT reset wrongCount on correct anymore.
        // wrongCount = 0;  // <-- removed

        SolidifyCover(letterNum);

        bool finished = (letterNum >= passcode.Length - 1);

        if (!finished)
        {
            letterNum++;
            ShowCorrectFeedbackThen(() =>
            {
                // After feedback, if covers are visible, prep next blinking cover
                if (codeCoverGroup != null && codeCoverGroup.activeSelf)
                    InitCoversHiddenExceptActive(letterNum);
                NextRound();
            });
        }
        /*else
        {
            ShowCorrectFeedbackThen(() =>
            {
                // End state: do NOT toggle selection panel here per request.
                StopBlink();
                HideAllCovers();
                // if (passwordSelectionPanel != null) passwordSelectionPanel.SetActive(false); // removed
            });
        }*/
        else
        {

            // Mark completion BEFORE showing feedback
            gameCompleted = true;
            completePanel.SetActive(true);
            winnerAudioClip.Play();


            //ShowCorrectFeedbackThen(() =>
            {
                // Stop blinking after final letter; do NOT auto-hide covers.
                StopBlink();

                // Make all covers visible & solid (no blink)
                for (int i = 0; i < codeCoverSlots.Count; i++)
                {
                    var cg = codeCoverSlots[i];
                    if (cg != null)
                    {
                        cg.gameObject.SetActive(true);
                        cg.alpha = 1f;
                    }
                }
                // Do NOT call HideAllCovers(); the player can still toggle covers via Reveal Code.
            }//);
        }
    }

    private void HandleIncorrect()
    {
        wrongCount++;
        bool willReset = (wrongCount >= maxWrong);

        ShowWrongFeedbackThen(willReset, () =>
        {
            if (willReset)
            {
                ResetPasscodeProgress();
                wrongCount = 0; // Reset only when maxWrong reached
            }
            NextRound();
        });
    }

    // ==================== FEEDBACK PANELS ====================

    private void ShowCorrectFeedbackThen(Action onDone)
    {
        // CHANGED: Do NOT hide selection or covers during feedback; overlays simply appear on top.
        // if (passwordSelectionPanel != null) passwordSelectionPanel.SetActive(false); // removed
        // StopBlink(); if (codeCoverGroup != null) codeCoverGroup.SetActive(false);   // removed

        if (correctPanel != null)
        {
            correctPanel.SetActive(true);
            StartCoroutine(HidePanelAfter(correctPanel, correctPanelDuration, () =>
            {
                correctPanel.SetActive(false);
                // Do not toggle selection here
                onDone?.Invoke();
            }));
        }
        else
        {
            onDone?.Invoke();
        }
    }

    private void ShowWrongFeedbackThen(bool willReset, Action onDone)
    {
        // CHANGED: Do NOT hide selection or covers during feedback; overlays simply appear on top.
        // if (passwordSelectionPanel != null) passwordSelectionPanel.SetActive(false); // removed
        // StopBlink(); if (codeCoverGroup != null) codeCoverGroup.SetActive(false);   // removed

        if (wrongPanelMessage != null)
        {
            if (willReset)
            {
                wrongPanelMessage.text = "Resetting results…";
            }
            else
            {
                int remaining = Mathf.Max(0, maxWrong - wrongCount);
                wrongPanelMessage.text = $"Wrong answer. {remaining} attempt(s) left.";
            }
        }

        if (wrongPanel != null)
        {
            wrongPanel.SetActive(true);
            StartCoroutine(HidePanelAfter(wrongPanel, wrongPanelDuration, () =>
            {
                wrongPanel.SetActive(false);
                // Do not toggle selection here
                onDone?.Invoke();
            }));
        }
        else
        {
            onDone?.Invoke();
        }
    }

    private System.Collections.IEnumerator HidePanelAfter(GameObject panel, float seconds, Action onDone)
    {
        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        onDone?.Invoke();
    }

    // ==================== DATA MGMT ====================

    private void BuildPoolsFromSO()
    {
        strongAll.Clear();
        weakAll.Clear();

        if (passwordSet == null || passwordSet.questions == null)
        {
            Debug.LogError("PasswordStrength1: QuestionData or its 'questions' array is null.");
            return;
        }

        foreach (var q in passwordSet.questions)
        {
            if (q == null) continue;
            if (q.correctAnswer) strongAll.Add(q);
            else weakAll.Add(q);
        }

        if (strongAll.Count == 0 || weakAll.Count == 0)
            Debug.LogWarning($"PasswordStrength1: Need at least one strong and one weak entry. Strong:{strongAll.Count} Weak:{weakAll.Count}");
    }

    private void RefillQueuesIfNeeded()
    {
        if (strongQ.Count == 0)
            foreach (var s in strongAll.OrderBy(_ => rng.Next()))
                strongQ.Enqueue(s);

        if (weakQ.Count == 0)
            foreach (var w in weakAll.OrderBy(_ => rng.Next()))
                weakQ.Enqueue(w);
    }

    // ==================== COVERS & BLINK ====================

    /*private void PrepareCoversForStart()
    {
        if (codeCoverGroup == null) return;
        // Covers start hidden; Reveal Code toggles visibility later.
        HideAllCovers();
        codeCoverGroup.SetActive(false);
        coversHiddenViaReveal = true;
    }*/

    /// <summary>
    /// Make all covers invisible *except* the active index (blinking).
    /// Already-solved slots -> visible & solid; future slots -> hidden.
    /// </summary>
    private void InitCoversHiddenExceptActive(int activeIndex)
    {
        if (codeCoverGroup == null || !codeCoverGroup.activeSelf) return;

        for (int i = 0; i < codeCoverSlots.Count; i++)
        {
            var cg = codeCoverSlots[i];
            if (cg == null) continue;

            if (i < activeIndex)
            {
                cg.gameObject.SetActive(true);
                cg.alpha = 1f;     // past: solid
            }
            else if (i == activeIndex)
            {
                cg.gameObject.SetActive(true);
                cg.alpha = 1f;     // will blink
            }
            else
            {
                cg.gameObject.SetActive(false); // future: hidden
            }
        }

        StartBlinkFor(activeIndex);
    }

    private void RefreshCoversIfVisible()
    {
        if (codeCoverGroup == null || !codeCoverGroup.activeSelf) return;

        for (int i = 0; i < codeCoverSlots.Count; i++)
        {
            var cg = codeCoverSlots[i];
            if (cg == null) continue;

            if (i < letterNum)
            {
                cg.gameObject.SetActive(true);
                cg.alpha = 1f;
            }
            else if (i == letterNum)
            {
                cg.gameObject.SetActive(true);
                // alpha animated by blink
            }
            else
            {
                cg.gameObject.SetActive(false);
            }
        }
    }

    private void StartBlinkFor(int index)
    {
        StopBlink();
        if (codeCoverGroup == null || !codeCoverGroup.activeSelf) return;
        if (index < 0 || index >= codeCoverSlots.Count) return;

        activeBlinkIndex = index;
        blinkRoutine = StartCoroutine(BlinkRoutine());
    }

    private void StopBlink()
    {
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }
        activeBlinkIndex = -1;
    }

    private System.Collections.IEnumerator BlinkRoutine()
    {
        while (activeBlinkIndex >= 0 &&
               codeCoverGroup != null && codeCoverGroup.activeSelf)
        {
            var cg = codeCoverSlots[activeBlinkIndex];
            if (cg != null)
            {
                float t = 0.5f * (1f + Mathf.Sin(Time.unscaledTime * (blinkSpeed * Mathf.PI * 2f)));
                cg.alpha = Mathf.Lerp(blinkMinAlpha, blinkMaxAlpha, t);
            }
            yield return null;
        }
    }

    private void SolidifyCover(int index)
    {
        if (codeCoverGroup == null) return;
        if (index < 0 || index >= codeCoverSlots.Count) return;

        var cg = codeCoverSlots[index];
        if (cg != null)
        {
            cg.gameObject.SetActive(true);
            cg.alpha = 1f;
        }
        if (activeBlinkIndex == index) StopBlink();
    }

    private void HideAllCovers()
    {
        for (int i = 0; i < codeCoverSlots.Count; i++)
        {
            var cg = codeCoverSlots[i];
            if (cg == null) continue;
            cg.gameObject.SetActive(false);
        }
        StopBlink();
    }

    private void ResetPasscodeProgress()
    {
        /*letterNum = 0;

        var underscores = new string('_', passcode.Length);
        currentPasscode = underscores;

        if (passwordResultText != null)
            passwordResultText.text = currentPasscode;
        */

        gameCompleted = false;  
        letterNum = 0;
        currentPasscode = string.Empty; // no underscores on reset either
        if (passwordResultText != null)
            passwordResultText.text = currentPasscode;


        // If covers are visible, restart with slot 0 blinking; else keep hidden
        if (codeCoverGroup != null && codeCoverGroup.activeSelf)
            InitCoversHiddenExceptActive(0);
        else
            HideAllCovers();
    }

    // ==================== TOP-LEVEL PANEL LOGIC ====================
    // CHANGED: Entire top-level panel visibility management removed per request.
    // private void RefreshTopLevelPanels() { ... }

    // ==================== OPTION TEXT CONSISTENCY ====================

    private System.Collections.IEnumerator SyncOptionFontSizesNextFrame()
    {
        // Wait a frame so TMP can auto-size based on new content
        yield return null;

        Canvas.ForceUpdateCanvases();
        option0Text.ForceMeshUpdate();
        option1Text.ForceMeshUpdate();

        float size0 = option0Text.fontSize;
        float size1 = option1Text.fontSize;
        float minSize = Mathf.Min(size0, size1);

        option0Text.enableAutoSizing = false;
        option1Text.enableAutoSizing = false;
        option0Text.fontSize = minSize;
        option1Text.fontSize = minSize;
    }
}
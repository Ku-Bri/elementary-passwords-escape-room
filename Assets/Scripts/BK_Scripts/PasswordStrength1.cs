using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PasswordStrength1 : MonoBehaviour
{
    [Header("Data Source (ScriptableObject)")]
    [SerializeField] private QuestionData passwordSet;    // Drag your QuestionData asset (Password Set Rm 5)

    [Header("UI: Core")]
    [SerializeField] private TMP_Text passwordResultText; // Cryptext label that starts with underscores (e.g., "______")
    [SerializeField] private TMP_Text option0Text;        // Button 0 label (TMP)
    [SerializeField] private TMP_Text option1Text;        // Button 1 label (TMP)
    [SerializeField] private GameObject passwordSelectionPanel; // Parent for the two option buttons/labels

    [Header("UI: Feedback Panels")]
    [SerializeField] private GameObject correctPanel;     // Shown briefly after a correct answer
    [SerializeField] private GameObject wrongPanel;       // Shown briefly after a wrong answer
    [SerializeField] private TMP_Text wrongPanelMessage;  // Optional: message text inside wrongPanel (remaining attempts, reset text)
    [SerializeField] private GameObject continueButton;   // Enabled when 'G' is visible in the cryptext (kept from your original flow)

    [Header("Covers (Per-Letter)")]
    [Tooltip("Parent object containing one cover image per passcode slot.")]
    [SerializeField] private GameObject codeCoverGroup;
    [Tooltip("CanvasGroups for each cover (left-to-right). Count should match passcode length.")]
    [SerializeField] private List<CanvasGroup> codeCoverSlots = new List<CanvasGroup>();

    [Header("Blink Settings")]
    [SerializeField, Range(0f, 1f)] private float blinkMinAlpha = 0.35f;
    [SerializeField, Range(0f, 1f)] private float blinkMaxAlpha = 1.00f;
    [SerializeField, Range(0.1f, 10f)] private float blinkSpeed = 4f;  // blink cycles/sec (roughly)

    [Header("Gameplay")]
    [SerializeField] private string passcode = "STRONG";  // Revealed one letter per correct answer
    [SerializeField] private int maxWrong = 3;            // After 3 wrong, reset progress

    [Header("Timings (seconds)")]
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

    // queues (shuffled, then dequeued, refilled when empty)
    private readonly Queue<TrueFalsQuestion> strongQ = new();
    private readonly Queue<TrueFalsQuestion> weakQ = new();

    private System.Random rng;

    // blinking
    private int activeBlinkIndex = -1;
    private Coroutine blinkRoutine;

    // reveal toggle (covers hidden while true)
    private bool coversHiddenViaReveal = false;

    private void Awake()
    {
        rng = new System.Random();

        if (passwordSet == null)
            Debug.LogError("PasswordStrength1: Missing QuestionData ScriptableObject reference.");

        // Default UI states
        if (continueButton != null) continueButton.SetActive(false);
        if (correctPanel != null) correctPanel.SetActive(false);
        if (wrongPanel != null) wrongPanel.SetActive(false);

        // Selection panel should be visible at start
        if (passwordSelectionPanel != null) passwordSelectionPanel.SetActive(true);

        // Covers start hidden; only the active slot will appear when solving.
        if (codeCoverGroup != null) codeCoverGroup.SetActive(true);  // parent is active, but individual slots control visibility
    }

    private void Start()
    {
        // Initialize cryptext with underscores
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
        }

        BuildPoolsFromSO();
        RefillQueuesIfNeeded();
        NextRound();

        // Covers: invisible until solving a slot. Start with slot 0 only (blinking).
        InitCoversHiddenExceptActive(0);
    }

    private void Update()
    {
        // Keep original behavior: enable Continue when 'G' is visible
        if (continueButton != null && currentPasscode.Contains("G"))
            continueButton.SetActive(true);
    }

    // ==================== PUBLIC UI HOOKS ====================

    /// <summary>Buttons call this: Option 0 -> PushOption(0), Option 1 -> PushOption(1)</summary>
    public void PushOption(int optionIndex)
    {
        // ignore clicks if selection panel not visible (during feedback)
        if (passwordSelectionPanel != null && !passwordSelectionPanel.activeSelf) return;

        if (optionIndex == correctOptionIndex) HandleCorrect();
        else HandleIncorrect();
    }

    /// <summary>Reveal Code button: toggles cover visibility</summary>
    public void OnRevealCode()
    {
        coversHiddenViaReveal = !coversHiddenViaReveal;

        if (codeCoverGroup == null) return;

        if (coversHiddenViaReveal)
        {
            // Hide entire cover group
            StopBlink();
            codeCoverGroup.SetActive(false);
        }
        else
        {
            // Show group and re-apply visibility rules (only active + already-solved covers visible)
            codeCoverGroup.SetActive(true);
            RefreshAllCoverVisibility();
            StartBlinkFor(letterNum); // resume blinking on the current slot
        }
    }

    // ==================== ROUND FLOW ====================

    private void NextRound()
    {
        RefillQueuesIfNeeded();

        // One strong + one weak (consumed from queues)
        var strong = strongQ.Dequeue();
        var weak = weakQ.Dequeue();

        // Random placement of strong
        correctOptionIndex = rng.Next(0, 2);

        // Re-enable Auto Size so TMP recalculates for new texts
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

        // Hide any previous wrong banner
        if (wrongPanel != null && wrongPanel.activeSelf)
            wrongPanel.SetActive(false);

        // After TMP lays out, sync both labels to the smaller auto-size
        StartCoroutine(SyncOptionFontSizesNextFrame());
    }

    private void HandleCorrect()
    {
        // Reveal next character by replacing the first underscore
        if (passwordResultText != null)
        {
            var regex = new Regex(Regex.Escape("_"));
            currentPasscode = regex.Replace(currentPasscode, passcode[letterNum].ToString(), 1);
            passwordResultText.text = currentPasscode;
        }

        // Strike reset on correct
        wrongCount = 0;

        // Covers: solidify revealed slot; make only next slot visible+blinking
        SolidifyCover(letterNum);

        bool finished = (letterNum >= passcode.Length - 1);

        if (!finished)
        {
            letterNum++;
            ShowCorrectFeedbackThen(() =>
            {
                // After feedback: prepare next slot cover (only that one visible)
                InitCoversHiddenExceptActive(letterNum);
                NextRound();
            });
        }
        else
        {
            // Last letter revealed
            ShowCorrectFeedbackThen(() =>
            {
                StopBlink();
                // Optional: leave selection hidden or enable Continue/etc.
                if (passwordSelectionPanel != null) passwordSelectionPanel.SetActive(false);
            });
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
                wrongCount = 0;
                // Start over from slot 0
                InitCoversHiddenExceptActive(0);
            }
            // After feedback (either normal wrong or after reset), show a brand-new pair
            NextRound();
        });
    }

    // ==================== FEEDBACK PANELS ====================

    private void ShowCorrectFeedbackThen(Action onDone)
    {
        // Hide selection during feedback
        if (passwordSelectionPanel != null) passwordSelectionPanel.SetActive(false);

        if (correctPanel != null)
        {
            correctPanel.SetActive(true);
            StartCoroutine(HidePanelAfter(correctPanel, correctPanelDuration, onDone));
        }
        else
        {
            // If no panel is set, just continue immediately
            onDone?.Invoke();
            if (passwordSelectionPanel != null) passwordSelectionPanel.SetActive(true);
        }
    }

    private void ShowWrongFeedbackThen(bool willReset, Action onDone)
    {
        // Hide selection during feedback
        if (passwordSelectionPanel != null) passwordSelectionPanel.SetActive(false);

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
                // After wrong feedback, re-show selection panel and continue
                if (passwordSelectionPanel != null) passwordSelectionPanel.SetActive(true);
                onDone?.Invoke();
            }));
        }
        else
        {
            // No wrong panel assigned; proceed immediately
            if (passwordSelectionPanel != null) passwordSelectionPanel.SetActive(true);
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
        if (panel != null) panel.SetActive(false);

        onDone?.Invoke();

        // If this was the correct panel, bring selection back (unless finished)
        if (panel == correctPanel && passwordSelectionPanel != null)
        {
            // Selection will be turned on by caller if needed (e.g., when not finished)
            passwordSelectionPanel.SetActive(true);
        }
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

    /// <summary>
    /// Make all covers invisible *except* the active index, which blinks.
    /// Already solved slots stay visible and solid.
    /// </summary>
    private void InitCoversHiddenExceptActive(int activeIndex)
    {
        if (codeCoverGroup == null) return;

        codeCoverGroup.SetActive(!coversHiddenViaReveal); // honor the Reveal toggle

        // Solidify all previous (revealed) covers, hide future ones
        for (int i = 0; i < codeCoverSlots.Count; i++)
        {
            var cg = codeCoverSlots[i];
            if (cg == null) continue;

            if (i < activeIndex)            // already revealed slots -> visible & solid
            {
                cg.gameObject.SetActive(!coversHiddenViaReveal);
                cg.alpha = 1f;
            }
            else if (i == activeIndex)      // current slot -> visible & blinking
            {
                cg.gameObject.SetActive(!coversHiddenViaReveal);
                cg.alpha = 1f; // will start blinking
            }
            else                            // future slots -> invisible (inactive)
            {
                cg.gameObject.SetActive(false);
            }
        }

        // Start blinking the active slot (if covers are shown)
        if (!coversHiddenViaReveal) StartBlinkFor(activeIndex);
        else StopBlink();
    }

    private void RefreshAllCoverVisibility()
    {
        if (codeCoverGroup == null) return;

        // When covers are shown, previous slots solid, current visible (blinking), future hidden
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
                // alpha will be animated by blinking
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
        if (coversHiddenViaReveal) return;
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
               codeCoverGroup != null && codeCoverGroup.activeSelf &&
               !coversHiddenViaReveal)
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
        if (index < 0 || index >= codeCoverSlots.Count) return;
        var cg = codeCoverSlots[index];
        if (cg != null)
        {
            cg.gameObject.SetActive(!coversHiddenViaReveal);
            cg.alpha = 1f;
        }
        if (activeBlinkIndex == index) StopBlink();
    }

    // ==================== CRYPTEXT RESET ====================

    private void ResetPasscodeProgress()
    {
        letterNum = 0;

        var underscores = new string('_', passcode.Length);
        currentPasscode = underscores;

        if (passwordResultText != null)
            passwordResultText.text = currentPasscode;

        if (continueButton != null)
            continueButton.SetActive(false);

        // Restart covers from slot 0 (invisible for future slots)
        InitCoversHiddenExceptActive(0);
    }

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
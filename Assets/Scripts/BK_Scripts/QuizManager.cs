using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuizManagerTF : MonoBehaviour
{
    [Header("Data")]
    public QuestionData questionSet;
    [Range(1, 50)] public int questionsPerRun = 8;

    [Header("UI")]
    public GameObject questionPanel;      // NEW: container for question UI (text + T/F buttons)
    public TMP_Text questionText;
    public Button trueButton;
    public Button falseButton;

    public Button nextSceneButton;        // only active after success
    public GameObject correctPanel;       // shows briefly on correct answers
    public GameObject wrongPanel;         // shows briefly on wrong answers

    private PanelOperator po;

    [Header("Navigation")]
    public SceneMovement sceneMovement;   // assign your existing SceneMovement (for LoadNextScene())

    // --- Runtime ---
    private List<int> runOrder = new List<int>();
    private int currentIndex = 0;

    // --- Save ---
    [System.Serializable]
    private class SaveData
    {
        public bool quizCompleted;
        public int rewardCode;  // or string, if you prefer
        public bool hideAnswerButtons;
    }

    private SaveData save = new SaveData();
    private string SavePath => Path.Combine(Application.persistentDataPath, "quiz_state.json");

    private bool hasStartedFirstRun = false;

    private void Start()
    {
        po = FindObjectOfType<PanelOperator>(true);

        SetPanel(correctPanel, false);
        SetPanel(wrongPanel, false);
        // Do NOT force questionPanel here; PanelOperator controls sceneObjects/questionPanel
        // SetPanel(questionPanel, true);   // ← remove

        nextSceneButton.gameObject.SetActive(false);

        LoadState();

        // If the quiz was already completed, show the code and stop here
        if (save.quizCompleted || save.hideAnswerButtons)
        {
            save.quizCompleted = true;
            save.hideAnswerButtons = true;
            ShowRewardCode();
            return;
        }

        // Start a gate that waits for the first time *no* panels are active
        StartCoroutine(WaitForPanelsThenStartOnce());
    }

    private System.Collections.IEnumerator WaitForPanelsThenStartOnce()
    {
        // Let all Awake/Start finish (PanelOperator sets initial visibility here)
        yield return null;

        var po = FindObjectOfType<PanelOperator>(true);
        if (po == null)
        {
            // No PanelOperator? Start immediately.
            BeginNewRun();
            ShowQuestion();
            hasStartedFirstRun = true;
            yield break;
        }

        // Wait until ALL panels are inactive (first time only)
        while (true)
        {
            bool anyActive = false;
            for (int i = 0; i < po.isPanelActiveArray.Length; i++)
            {
                if (po.isPanelActiveArray[i]) { anyActive = true; break; }
            }
            if (!anyActive) break;  // instruction/hint dismissed the first time
            yield return null;
        }

        if (!hasStartedFirstRun)
        {
            hasStartedFirstRun = true;
            // PanelOperator will have unhidden sceneObjects (your QuestionPanel) already
            BeginNewRun();
            ShowQuestion();
        }
    }


    // -------------------
    // CORE QUIZ BEHAVIOR
    // -------------------

    private void BeginNewRun()
    {

        SetAnswerButtonsVisible(true);  // ensure buttons are back for new attempts
        EnableAnswerButtons(true);

        currentIndex = 0;
        runOrder.Clear();

        int total = questionSet.questions.Length;
        if (total < questionsPerRun)
        {
            Debug.LogWarning($"[QuizManagerTF] Not enough questions in the set (have {total}, need {questionsPerRun}). Using all available questions this run.");
            questionsPerRun = total;
        }

        // Build pool and Fisher-Yates shuffle
        var pool = new List<int>(total);
        for (int i = 0; i < total; i++) pool.Add(i);
        for (int i = 0; i < pool.Count; i++)
        {
            int r = Random.Range(i, pool.Count);
            (pool[i], pool[r]) = (pool[r], pool[i]);
        }
        for (int i = 0; i < questionsPerRun; i++)
            runOrder.Add(pool[i]);

        // Reset panels for a fresh run
        SetPanel(correctPanel, false);
        SetPanel(wrongPanel, false);
        //SetPanel(questionPanel, true);
        EnableAnswerButtons(true);
    }

    private void ShowQuestion()
    {
        int qIndex = runOrder[currentIndex];
        var q = questionSet.questions[qIndex];
        questionText.text = q.questionText;

        // Question panel visible; feedback panels hidden
        SetPanel(correctPanel, false);
        SetPanel(wrongPanel, false);
        //SetPanel(questionPanel, true);

        EnableAnswerButtons(true);
    }

    public void OnTruePressed() => PlayerAnswer(true);
    public void OnFalsePressed() => PlayerAnswer(false);

    private void PlayerAnswer(bool playerAnswer)
    {
        // Prevent double clicks while feedback shows
        EnableAnswerButtons(false);

        int qIndex = runOrder[currentIndex];
        bool isCorrect = (playerAnswer == questionSet.questions[qIndex].correctAnswer);

        if (isCorrect)
        {
            StartCoroutine(HandleCorrectThenAdvance());
        }
        else
        {
            StartCoroutine(HandleWrongThenRestart());
        }
    }

    private IEnumerator HandleCorrectThenAdvance()
    {
        // Hide question via PanelOperator
        if (po != null) po.HideSceneObjects();
        else SetPanel(questionPanel, false);

        SetPanel(correctPanel, true);
        yield return new WaitForSeconds(0.6f);
        SetPanel(correctPanel, false);

        currentIndex++;

        if (currentIndex >= runOrder.Count)
        {
            OnQuizPassed();
        }
        else
        {
            // Show question via PanelOperator
            if (po != null) po.UnhideSceneObjects();
            else SetPanel(questionPanel, true);

            ShowQuestion();
        }
    }

    private IEnumerator HandleWrongThenRestart()
    {
        // Hide question via PanelOperator
        if (po != null) po.HideSceneObjects();
        else SetPanel(questionPanel, false);

        SetPanel(wrongPanel, true);
        yield return new WaitForSeconds(0.8f);
        SetPanel(wrongPanel, false);

        BeginNewRun();

        // Show question via PanelOperator
        if (po != null) po.UnhideSceneObjects();
        else SetPanel(questionPanel, true);

        ShowQuestion();
    }

    private void OnQuizPassed()
    {
        if (!save.quizCompleted)
        {
            save.quizCompleted = true;
            save.rewardCode = Random.Range(10000, 99999);
            save.hideAnswerButtons = true;
            SaveState();
        }

        ShowRewardCode();
    }

    private void ShowRewardCode()
    {
        questionText.text = $"Your code is:\n<size=48><b>{save.rewardCode:D5}</b></size>";

        // Only the question text is needed to show the code in the question panel area
        SetPanel(correctPanel, false);
        SetPanel(wrongPanel, false);
        SetPanel(questionPanel, true);     // keep questionPanel active to display the code text

        //EnableAnswerButtons(false);
        SetAnswerButtonsVisible(false);
        nextSceneButton.gameObject.SetActive(true);
    }

    // -------------------
    // SAVE / LOAD
    // -------------------

    private void SaveState()
    {
        try
        {
            var json = JsonUtility.ToJson(save, true);
            File.WriteAllText(SavePath, json);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[QuizManagerTF] Save failed: {ex.Message}");
        }
    }

    private void LoadState()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                var json = File.ReadAllText(SavePath);
                save = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[QuizManagerTF] Load failed: {ex.Message}");
            save = new SaveData();
        }
    }

    // Called by the Next button (do not auto-navigate on answers)
    public void SaveAndGoNext()
    {
        SaveState();

        if (sceneMovement != null)
            sceneMovement.LoadNextScene();
        else
            Debug.LogWarning("[QuizManagerTF] SceneMovement not assigned.");
    }

    // -------------------
    // Small helpers
    // -------------------

    private void EnableAnswerButtons(bool enable)
    {
        if (trueButton != null) trueButton.interactable = enable;
        if (falseButton != null) falseButton.interactable = enable;
    }

    private void SetPanel(GameObject panel, bool visible)
    {
        if (panel != null) panel.SetActive(visible);
    }

    private void SetAnswerButtonsVisible(bool visible)
    {
        if (trueButton != null)
        {
            trueButton.gameObject.SetActive(visible);
            trueButton.interactable = visible;
        }
        if (falseButton != null)
        {
            falseButton.gameObject.SetActive(visible);
            falseButton.interactable = visible;
        }
    }
}

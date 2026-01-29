using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class DirectionalLockManagerChoice : MonoBehaviour
{
    [Header("Config")]
    [Tooltip("Grid will be size x size (4 or 5 recommended).")]
    public int gridSize = 5;

    [Tooltip("Saved to persistentDataPath so returning to this scene keeps the same puzzle.")]
    public string saveFileName = "directional_lock_state.json";

    public QuestionBank questionBank;

    [Header("Prompt Mix (Path tiles only)")]
    [Range(0f, 1f)]
    [Tooltip("For MEDIUM: 0.40 is a good default. Controls how often SAFER questions appear on the solution path.")]
    public float saferRateOnPath = 0.40f;

    [Tooltip("Minimum number of SAFER prompts on the path (if available).")]
    public int minSaferOnPath = 2;

    [Header("Behavior")]
    [Tooltip("If true, player can click an already-answered tile to change their answer.")]
    public bool allowReAnswer = true;

    [Header("Grid UI")]
    [Tooltip("CanvasGroup on GridRoot. Used to disable grid interaction when ChoicePanel is open.")]
    public CanvasGroup gridCanvasGroup;

    [Tooltip("The GridLayoutGroup container where cell prefabs will be instantiated.")]
    public Transform gridContainer;

    public DirectionalLockCell cellPrefab;

    [Header("Choice Panel UI")]
    [Tooltip("Panel that appears when a tile is clicked.")]
    public GameObject choicePanel;

    [Tooltip("TMP_Text that displays the selected tile question.")]
    public TMP_Text choiceQuestionText;

    public Button safeButton;
    public Button unsafeButton;

    [Header("Icons")]
    public Sprite iconSafe;   // ✔
    public Sprite iconUnsafe; // ✖

    [Header("Optional - Navigation")]
    [Tooltip("If assigned, Next/Prev calls can be routed through SceneMovement.")]
    public SceneMovement sceneMovement; // Your script has LoadNextScene/LoadPreviousScene [1](https://univoftulsa-my.sharepoint.com/personal/bridget-kurr_utulsa_edu1/Documents/Microsoft%20Copilot%20Chat%20Files/SceneMovement.cs)

    // -------------------- Runtime State --------------------
    private DirectionalLockState state;
    private readonly List<DirectionalLockCell> cells = new();
    private int pendingIndex = -1;

    private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

    // -------------------- Unity Lifecycle --------------------
    private void Awake()
    {
        if (safeButton != null) safeButton.onClick.AddListener(() => ResolveChoice(true));
        if (unsafeButton != null) unsafeButton.onClick.AddListener(() => ResolveChoice(false));

        if (choicePanel != null) choicePanel.SetActive(false);
        SetGridInteractable(true);
    }

    private void Start()
    {
        LoadOrCreateState();
        BuildGrid();
        RefreshGridVisuals();
    }

    private void OnEnable()
    {
        // If we come back to the scene and UI is enabled later, refresh visuals.
        if (state != null && cells.Count > 0)
            RefreshGridVisuals();
    }

    // -------------------- Public API for Panel Overlay System --------------------
    /// <summary>
    /// Call this when Instructions/Hints open so we don't leave the grid disabled
    /// or a question half-open. (Used by an improved PanelOperator or any overlay UI.)
    /// </summary>
    public void ForceCloseChoicePanelAndEnableGrid()
    {
        pendingIndex = -1;
        if (choicePanel != null) choicePanel.SetActive(false);
        SetGridInteractable(true);
        RefreshGridVisuals();
    }

    // -------------------- Tile Click Flow --------------------
    public void OnCellClicked(int index)
    {
        if (state == null) return;

        // Start/End are labels only (no question)
        if (index == state.startIndex || index == state.endIndex)
            return;

        // If already answered and re-answer disabled, ignore
        bool answered = state.playerMark[index] != -1;
        if (answered && !allowReAnswer)
            return;

        pendingIndex = index;

        // Populate question text
        if (choiceQuestionText != null)
            choiceQuestionText.text = state.prompts[index];

        // Show choice panel and disable grid interaction
        if (choicePanel != null) choicePanel.SetActive(true);
        SetGridInteractable(false);
    }

    private void ResolveChoice(bool choseSafe)
    {
        if (state == null) return;
        if (pendingIndex < 0) return;

        // Store player answer: 1 SAFE, 0 UNSAFE
        state.playerMark[pendingIndex] = choseSafe ? 1 : 0;

        // Reveal the tile (fog disappears after answering)
        state.revealed[pendingIndex] = true;

        pendingIndex = -1;

        // Hide choice panel and re-enable grid
        if (choicePanel != null) choicePanel.SetActive(false);
        SetGridInteractable(true);

        SaveState();
        RefreshGridVisuals();
    }

    // -------------------- Grid UI Helpers --------------------
    private void SetGridInteractable(bool on)
    {
        if (gridCanvasGroup == null) return;

        gridCanvasGroup.interactable = on;
        gridCanvasGroup.blocksRaycasts = on;

        // Optional: subtle dim when disabled
        gridCanvasGroup.alpha = on ? 1f : 0.85f;
    }

    private void BuildGrid()
    {
        // Clear existing children
        for (int i = gridContainer.childCount - 1; i >= 0; i--)
            Destroy(gridContainer.GetChild(i).gameObject);

        cells.Clear();

        int total = state.gridSize * state.gridSize;
        for (int i = 0; i < total; i++)
        {
            var cell = Instantiate(cellPrefab, gridContainer);
            cell.Init(this, i);
            cells.Add(cell);
        }
    }

    private void RefreshGridVisuals()
    {
        if (state == null) return;

        int total = state.gridSize * state.gridSize;

        for (int i = 0; i < total; i++)
        {
            bool isStart = (i == state.startIndex);
            bool isEnd = (i == state.endIndex);

            // Labels
            if (isStart) cells[i].SetLabel("START");
            else if (isEnd) cells[i].SetLabel("END");
            else cells[i].SetLabel("");

            // Tile answered?
            bool answered = state.playerMark[i] != -1;

            // Fog stays on until SAFE/UNSAFE selected
            bool showFog = !(isStart || isEnd) && !answered;
            cells[i].SetFog(showFog);

            // Mark icons appear after answering
            if (!isStart && !isEnd && answered)
            {
                bool safe = state.playerMark[i] == 1;
                cells[i].SetMark(safe ? iconSafe : iconUnsafe, true);
            }
            else
            {
                cells[i].SetMark(null, false);
            }

            // Interactable rules:
            // - Start/End not clickable
            // - When choice panel open, grid is blocked by CanvasGroup anyway, but keep consistent
            bool clickable = !isStart && !isEnd;
            if (!allowReAnswer && answered) clickable = false;

            cells[i].SetInteractable(clickable);
        }
    }

    // -------------------- Save / Load --------------------
    private void LoadOrCreateState()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            state = JsonUtility.FromJson<DirectionalLockState>(json);

            // Basic sanity fallback if something changed
            if (state == null || state.gridSize <= 0)
            {
                Debug.LogWarning("[DirectionalLock] Save file invalid; generating new puzzle.");
                state = GenerateNew(gridSize);
                SaveState();
            }
            return;
        }

        state = GenerateNew(gridSize);
        SaveState(); // write immediately so returning keeps same puzzle
    }

    private void SaveState()
    {
        try
        {
            string json = JsonUtility.ToJson(state, true);
            File.WriteAllText(SavePath, json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DirectionalLock] Save failed: {ex.Message}");
        }
    }

    // -------------------- Generation --------------------
    private DirectionalLockState GenerateNew(int size)
    {
        var s = new DirectionalLockState();
        s.gridSize = size;

        // Seed stored for reproducibility/debugging
        s.seed = (int)DateTime.Now.Ticks;
        Random.InitState(s.seed);

        int total = size * size;

        // Choose START and a far END
        s.startIndex = Random.Range(0, total);
        s.endIndex = PickFarIndex(s.startIndex, size);

        // Create a single path and compute expected code (for CodeCheck scene)
        s.solutionPath = GeneratePath(s.startIndex, s.endIndex, size);
        s.expectedCode = ComputeCodeFromPath(s.solutionPath, size);

        // Allocate per-cell lists
        s.prompts = new List<string>(new string[total]);
        s.isSafeSolution = new List<bool>(new bool[total]);
        s.revealed = new List<bool>(new bool[total]);
        s.playerMark = new List<int>(new int[total]);

        for (int i = 0; i < total; i++)
        {
            s.revealed[i] = false;
            s.playerMark[i] = -1; // unanswered
        }

        // Start/End visible
        s.revealed[s.startIndex] = true;
        s.revealed[s.endIndex] = true;

        // Determine solution set
        var pathSet = new HashSet<int>(s.solutionPath);
        for (int i = 0; i < total; i++)
            s.isSafeSolution[i] = pathSet.Contains(i);

        // Build pools: Unsafe, Safe, Safer (Safer counts as SAFE for student)
        var unsafePool = new List<QuestionBank.Q>();
        var safePool = new List<QuestionBank.Q>();   // "Clearly safe"
        var saferPool = new List<QuestionBank.Q>();  // "Safer/tricky safe"

        foreach (var q in questionBank.questions)
        {
            switch (q.bucket)
            {
                case QuestionBank.Bucket.Unsafe: unsafePool.Add(q); break;
                case QuestionBank.Bucket.Safe: safePool.Add(q); break;
                case QuestionBank.Bucket.Safer: saferPool.Add(q); break;
            }
        }

        Shuffle(unsafePool);
        Shuffle(safePool);
        Shuffle(saferPool);

        // Decide which PATH tiles get SAFER prompts
        var pathTiles = new List<int>(s.solutionPath);
        pathTiles.Remove(s.startIndex);
        pathTiles.Remove(s.endIndex);

        int targetSafer = Mathf.RoundToInt(pathTiles.Count * saferRateOnPath);
        targetSafer = Mathf.Max(targetSafer, minSaferOnPath);
        targetSafer = Mathf.Min(targetSafer, pathTiles.Count);
        targetSafer = Mathf.Min(targetSafer, saferPool.Count);

        Shuffle(pathTiles);
        var saferTileSet = new HashSet<int>();
        for (int k = 0; k < targetSafer; k++)
            saferTileSet.Add(pathTiles[k]);

        // Assign prompts
        int unsafeIdx = 0, safeIdx = 0, saferIdx = 0;

        for (int i = 0; i < total; i++)
        {
            if (i == s.startIndex || i == s.endIndex)
            {
                s.prompts[i] = ""; // labels only
                continue;
            }

            if (pathSet.Contains(i))
            {
                // On the correct path => SAFE for student (mix safe/safer)
                if (saferTileSet.Contains(i) && saferPool.Count > 0)
                    s.prompts[i] = saferPool[saferIdx++ % saferPool.Count].text;
                else if (safePool.Count > 0)
                    s.prompts[i] = safePool[safeIdx++ % safePool.Count].text;
                else if (saferPool.Count > 0) // fallback
                    s.prompts[i] = saferPool[saferIdx++ % saferPool.Count].text;
                else
                    s.prompts[i] = "[Missing SAFE prompt]";
            }
            else
            {
                // Off path => UNSAFE
                if (unsafePool.Count > 0)
                    s.prompts[i] = unsafePool[unsafeIdx++ % unsafePool.Count].text;
                else
                    s.prompts[i] = "[Missing UNSAFE prompt]";
            }
        }

        return s;
    }

    // Pick an end index far from start (Manhattan distance heuristic)
    private int PickFarIndex(int start, int size)
    {
        int sr = start / size, sc = start % size;
        int total = size * size;

        int best = start;
        int bestD = -1;

        for (int t = 0; t < 30; t++)
        {
            int cand = Random.Range(0, total);
            int cr = cand / size, cc = cand % size;
            int d = Mathf.Abs(sr - cr) + Mathf.Abs(sc - cc);
            if (d > bestD)
            {
                bestD = d;
                best = cand;
            }
        }

        // fallback if it somehow matches start
        return best == start ? (start + total / 2) % total : best;
    }

    // Randomized DFS path generation (self-avoiding)
    private List<int> GeneratePath(int start, int end, int size)
    {
        for (int attempt = 0; attempt < 250; attempt++)
        {
            var path = new List<int>();
            var visited = new HashSet<int>();
            if (DFS(start, end, size, visited, path))
            {
                path.Reverse();
                return path;
            }
        }

        // fallback (rare)
        return new List<int> { start, end };
    }

    private bool DFS(int cur, int goal, int size, HashSet<int> visited, List<int> path)
    {
        visited.Add(cur);

        if (cur == goal)
        {
            path.Add(cur);
            return true;
        }

        var neigh = Neighbors(cur, size);
        Shuffle(neigh);

        foreach (var n in neigh)
        {
            if (visited.Contains(n)) continue;
            if (DFS(n, goal, size, visited, path))
            {
                path.Add(cur);
                return true;
            }
        }

        return false;
    }

    private List<int> Neighbors(int idx, int size)
    {
        var res = new List<int>();
        int r = idx / size;
        int c = idx % size;

        if (r > 0) res.Add(idx - size);
        if (r < size - 1) res.Add(idx + size);
        if (c > 0) res.Add(idx - 1);
        if (c < size - 1) res.Add(idx + 1);

        return res;
    }

    private string ComputeCodeFromPath(List<int> path, int size)
    {
        var chars = new List<char>();

        for (int i = 0; i < path.Count - 1; i++)
        {
            int from = path[i];
            int to = path[i + 1];

            int fr = from / size, fc = from % size;
            int tr = to / size, tc = to % size;

            if (tr == fr - 1 && tc == fc) chars.Add('U');
            else if (tr == fr + 1 && tc == fc) chars.Add('D');
            else if (tr == fr && tc == fc - 1) chars.Add('L');
            else if (tr == fr && tc == fc + 1) chars.Add('R');
        }

        return new string(chars.ToArray());
    }

    private void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // -------------------- Optional: public getters for CodeCheck debugging --------------------
    public string GetExpectedCode() => state != null ? state.expectedCode : "";
}
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class DirectionalLockManagerChoice : MonoBehaviour
{
    [Header("Config")]
    public int gridSize = 5;
    public string saveFileName = "directional_lock_state.json";
    public QuestionBank questionBank;

    [Header("Visual/Interaction")]
    [Tooltip("If true, the grid is hidden while the choice panel is active. If false, grid stays visible but non-interactable.")]
    public bool hideGridWhenChoicePanelOpen = true;

    [Header("Grid UI")]
    public CanvasGroup gridCanvasGroup;       // Add CanvasGroup to GridRoot
    public Transform gridContainer;           // GridContainer with GridLayoutGroup
    public DirectionalLockCell cellPrefab;

    [Header("Choice Panel UI")]
    public GameObject choicePanel;
    public TMP_Text choiceQuestionText;
    public Button safeButton;
    public Button unsafeButton;

    [Header("Code UI")]
    public TMP_Text playerCodeText;           // shows derived code from player's current SAFE marks
    public TMP_Text playerCodeStatusText;     // optional: shows (no path / ambiguous / unique)

    [Header("Icons")]
    public Sprite iconSafe;                   // ✔
    public Sprite iconUnsafe;                 // ✖

    // -------- Runtime state --------
    private DirectionalLockState state;
    private readonly List<DirectionalLockCell> cells = new();
    private int pendingIndex = -1;

    private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

    private void Awake()
    {
        safeButton.onClick.AddListener(() => ResolveChoice(true));
        unsafeButton.onClick.AddListener(() => ResolveChoice(false));

        choicePanel.SetActive(false);
        SetGridActiveVisible(true, true);
    }

    private void Start()
    {
        LoadOrCreateState();
        BuildGrid();
        RefreshGridVisuals();
        RecomputeAndDisplayPlayerCode();
    }

    // Call this from your overlay panel system when instructions/hints open
    // so we don't leave the puzzle in an input-locked state. [2](https://univoftulsa-my.sharepoint.com/personal/bridget-kurr_utulsa_edu1/Documents/Microsoft%20Copilot%20Chat%20Files/SceneMovement.cs)
    public void ForceCloseChoicePanelAndEnableGrid()
    {
        pendingIndex = -1;
        choicePanel.SetActive(false);
        SetGridActiveVisible(true, true);
    }

    // -------- Tile click -> ChoicePanel --------
    public void OnCellClicked(int index)
    {
        if (index == state.startIndex || index == state.endIndex)
            return;

        pendingIndex = index;
        choiceQuestionText.text = state.prompts[index];

        // Disable/hide grid; show choice panel
        SetGridActiveVisible(false, !hideGridWhenChoicePanelOpen);
        choicePanel.SetActive(true);
    }

    private void ResolveChoice(bool choseSafe)
    {
        if (pendingIndex < 0) return;

        int idx = pendingIndex;
        pendingIndex = -1;

        // record mark
        state.playerMark[idx] = choseSafe ? 1 : 0;

        // reveal tile once answered
        state.revealed[idx] = true;

        // hide choice panel; re-enable/show grid
        choicePanel.SetActive(false);
        SetGridActiveVisible(true, true);

        SaveState();
        RefreshGridVisuals();
        RecomputeAndDisplayPlayerCode();
    }

    private void SetGridActiveVisible(bool active, bool visible)
    {
        if (gridCanvasGroup == null) return;

        gridCanvasGroup.interactable = active;
        gridCanvasGroup.blocksRaycasts = active;
        gridCanvasGroup.alpha = visible ? 1f : 0f;
    }

    // -------- Grid build/refresh --------
    private void BuildGrid()
    {
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
        int total = state.gridSize * state.gridSize;

        for (int i = 0; i < total; i++)
        {
            bool isStart = (i == state.startIndex);
            bool isEnd = (i == state.endIndex);

            // labels
            if (isStart) cells[i].SetLabel("START");
            else if (isEnd) cells[i].SetLabel("END");
            else cells[i].SetLabel("");

            // answered?
            bool answered = state.playerMark[i] != -1;

            // fog visible until answered (start/end never fogged)
            bool fog = !(isStart || isEnd) && !answered;
            cells[i].SetFog(fog);

            // mark icon after answered
            if (!isStart && !isEnd && answered)
            {
                bool safe = state.playerMark[i] == 1;
                cells[i].SetMark(safe ? iconSafe : iconUnsafe, true);
            }
            else
            {
                cells[i].SetMark(null, false);
            }

            // any non-start/end can be clicked (your requirement)
            cells[i].SetInteractable(!isStart && !isEnd);
        }
    }

    // -------- Player-derived code (from their SAFE marks) --------
    private void RecomputeAndDisplayPlayerCode()
    {
        // Build graph nodes: START + END + all player SAFE tiles
        int n = state.gridSize * state.gridSize;
        bool[] included = new bool[n];
        included[state.startIndex] = true;
        included[state.endIndex] = true;

        for (int i = 0; i < n; i++)
            if (state.playerMark[i] == 1) included[i] = true;

        // Determine if START->END connected, and whether graph has cycles in that component.
        var parent = new Dictionary<int, int>();
        bool connected = TryBuildParentTreeAndDetectCycle(included, state.startIndex, state.endIndex, state.gridSize, parent, out bool hasCycle);

        if (!connected)
        {
            SetPlayerCodeUI("(no path)", "No SAFE path from START to END yet.");
            state.playerDerivedCode = "";
            state.playerCodeAmbiguous = false;
            SaveState();
            return;
        }

        if (hasCycle)
        {
            // multiple possible routes exist
            SetPlayerCodeUI("(ambiguous)", "Multiple routes exist (cycle). Make your SAFE choices more precise.");
            state.playerDerivedCode = "";
            state.playerCodeAmbiguous = true;
            SaveState();
            return;
        }

        // Unique path exists (tree -> exactly one simple path)
        List<int> path = ReconstructPath(parent, state.startIndex, state.endIndex);
        string code = ComputeCodeFromPath(path, state.gridSize);

        SetPlayerCodeUI(code, "Unique path found from your SAFE tiles.");
        state.playerDerivedCode = code;
        state.playerCodeAmbiguous = false;
        SaveState();
    }

    private void SetPlayerCodeUI(string code, string status)
    {
        if (playerCodeText != null)
            playerCodeText.text = code; //$"Code: {code}";

        if (playerCodeStatusText != null)
            playerCodeStatusText.text = status;
    }

    // BFS/DFS from start building a parent tree; detect cycles in reachable component.
    private bool TryBuildParentTreeAndDetectCycle(
        bool[] included, int start, int end, int size,
        Dictionary<int, int> parent, out bool hasCycle)
    {
        hasCycle = false;
        parent.Clear();

        var visited = new bool[included.Length];
        var stack = new Stack<int>();
        stack.Push(start);
        visited[start] = true;

        while (stack.Count > 0)
        {
            int u = stack.Pop();

            foreach (int v in Neighbors(u, size))
            {
                if (!included[v]) continue;

                if (!visited[v])
                {
                    visited[v] = true;
                    parent[v] = u;
                    stack.Push(v);
                }
                else
                {
                    // If visited and v isn't the parent of u, we found a back-edge -> cycle
                    if (parent.TryGetValue(u, out int pu))
                    {
                        if (v != pu) hasCycle = true;
                    }
                }
            }
        }

        return visited[end];
    }

    private List<int> ReconstructPath(Dictionary<int, int> parent, int start, int end)
    {
        var path = new List<int>();
        int cur = end;
        path.Add(cur);

        while (cur != start)
        {
            if (!parent.ContainsKey(cur))
                break; // safety
            cur = parent[cur];
            path.Add(cur);
        }

        path.Reverse();
        return path;
    }

    // -------- Save/Load --------
    private void LoadOrCreateState()
    {
        if (File.Exists(SavePath))
        {
            state = JsonUtility.FromJson<DirectionalLockState>(File.ReadAllText(SavePath));
            return;
        }

        state = GenerateNew(gridSize);
        SaveState();
    }

    private void SaveState()
    {
        try
        {
            File.WriteAllText(SavePath, JsonUtility.ToJson(state, true));
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DirectionalLock] Save failed: {ex.Message}");
        }
    }

    // -------- TRUE puzzle generation: ONE induced safe path, everything else unsafe --------
    private DirectionalLockState GenerateNew(int size)
    {
        var s = new DirectionalLockState();
        s.gridSize = size;
        s.seed = (int)DateTime.Now.Ticks;
        Random.InitState(s.seed);

        int total = size * size;

        s.startIndex = Random.Range(0, total);
        s.endIndex = PickFarIndex(s.startIndex, size);

        // *** key: induced path generation to prevent shortcut adjacencies ***
        s.solutionPath = GenerateInducedPath(s.startIndex, s.endIndex, size);

        // expectedCode from true path (used by CodeCheck) [1](https://univoftulsa-my.sharepoint.com/personal/bridget-kurr_utulsa_edu1/Documents/Microsoft%20Copilot%20Chat%20Files/PanelOperator.cs)
        s.expectedCode = ComputeCodeFromPath(s.solutionPath, size);

        // Allocate arrays
        s.prompts = new List<string>(new string[total]);
        s.isSafeSolution = new List<bool>(new bool[total]);
        s.revealed = new List<bool>(new bool[total]);
        s.playerMark = new List<int>(new int[total]);

        for (int i = 0; i < total; i++)
        {
            s.revealed[i] = false;
            s.playerMark[i] = -1;
            s.isSafeSolution[i] = false;
        }

        s.revealed[s.startIndex] = true;
        s.revealed[s.endIndex] = true;

        // Safe tiles = exactly the induced path (including endpoints)
        var pathSet = new HashSet<int>(s.solutionPath);
        foreach (int idx in pathSet)
            s.isSafeSolution[idx] = true;

        // Build pools: SAFE (safe+safer) vs UNSAFE (everything else)
        var unsafePool = new List<QuestionBank.Q>();
        var safePool = new List<QuestionBank.Q>();
        var saferPool = new List<QuestionBank.Q>();

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

        // Decide which path tiles (excluding endpoints) get SAFER prompts
        var pathTiles = new List<int>(s.solutionPath);
        pathTiles.Remove(s.startIndex);
        pathTiles.Remove(s.endIndex);
        Shuffle(pathTiles);

        int targetSafer = Mathf.Min(saferPool.Count, Mathf.Clamp(Mathf.RoundToInt(pathTiles.Count * 0.40f), 0, pathTiles.Count));
        // guarantee at least 1 SAFER if available and path long enough
        if (saferPool.Count > 0 && pathTiles.Count >= 3) targetSafer = Mathf.Max(targetSafer, 1);

        var saferTileSet = new HashSet<int>();
        for (int k = 0; k < targetSafer; k++)
            saferTileSet.Add(pathTiles[k]);

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
                // On true path => SAFE question (mix safe/safer)
                if (saferTileSet.Contains(i) && saferPool.Count > 0)
                    s.prompts[i] = saferPool[saferIdx++ % saferPool.Count].text;
                else if (safePool.Count > 0)
                    s.prompts[i] = safePool[safeIdx++ % safePool.Count].text;
                else if (saferPool.Count > 0)
                    s.prompts[i] = saferPool[saferIdx++ % saferPool.Count].text;
                else
                    s.prompts[i] = "[Missing SAFE prompt]";
            }
            else
            {
                // Everything else must be UNSAFE
                if (unsafePool.Count > 0)
                    s.prompts[i] = unsafePool[unsafeIdx++ % unsafePool.Count].text;
                else
                    s.prompts[i] = "[Missing UNSAFE prompt]";
            }
        }

        // Optional fields if you added them:
        s.playerDerivedCode = "";
        s.playerCodeAmbiguous = false;

        return s;
    }

    // --- Induced path generation: avoids shortcut adjacencies ---
    private List<int> GenerateInducedPath(int start, int end, int size)
    {
        for (int attempt = 0; attempt < 600; attempt++)
        {
            var path = new List<int>();
            var set = new HashSet<int>();

            if (BuildInducedPathDFS(start, end, size, path, set))
                return path;
        }

        // fallback (should be rare): use simple DFS path without induced constraint
        return GenerateSimplePath(start, end, size);
    }

    private bool BuildInducedPathDFS(int cur, int end, int size, List<int> path, HashSet<int> set)
    {
        path.Add(cur);
        set.Add(cur);

        if (cur == end)
            return true;

        var neigh = Neighbors(cur, size);
        Shuffle(neigh);

        foreach (int nxt in neigh)
        {
            if (set.Contains(nxt)) continue;

            // induced constraint: nxt cannot touch any existing path node except cur
            bool touchesOther = false;
            foreach (int adj in Neighbors(nxt, size))
            {
                if (set.Contains(adj) && adj != cur)
                {
                    touchesOther = true;
                    break;
                }
            }
            if (touchesOther) continue;

            if (BuildInducedPathDFS(nxt, end, size, path, set))
                return true;
        }

        // backtrack
        set.Remove(cur);
        path.RemoveAt(path.Count - 1);
        return false;
    }

    // fallback simple DFS (non-induced)
    private List<int> GenerateSimplePath(int start, int end, int size)
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

    // --- helpers ---
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
            if (d > bestD) { bestD = d; best = cand; }
        }
        return best == start ? (start + total / 2) % total : best;
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
            chars.Add(DirectionChar(path[i], path[i + 1], size));
        return new string(chars.ToArray());
    }

    private char DirectionChar(int from, int to, int size)
    {
        int fr = from / size, fc = from % size;
        int tr = to / size, tc = to % size;

        if (tr == fr - 1 && tc == fc) return 'U';
        if (tr == fr + 1 && tc == fc) return 'D';
        if (tr == fr && tc == fc - 1) return 'L';
        return 'R';
    }

    private void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
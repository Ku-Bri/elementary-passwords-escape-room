
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Scene-level state manager for the word search.
/// - Saves per-character selection states keyed by CharObject.CharId
/// - Recomputes per-word completion on load and updates visuals
/// - Handles shared letters across multiple words safely
/// - Adds robust diagnostics and fallbacks for missing arrays/IDs
/// Works with Unity's JsonUtility (no Dictionary in save data).
/// </summary>
public class WordSearchStateManager : MonoBehaviour
{
    [Header("Hierarchy Roots")]
    [Tooltip("Parent Transform that contains the word 'empties' (each with a WordChecker).")]
    [SerializeField] private Transform charListParent;

    [Header("Save Settings")]
    [Tooltip("File name under Application.persistentDataPath. Use different names per puzzle if needed.")]
    [SerializeField] private string saveFileName = "wordsearch_default.json";

    private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

    // Reverse index: for each CharObject, which WordChecker(s) include it?
    private readonly Dictionary<CharObject, List<WordChecker>> charToWords = new();

    #region Unity Lifecycle

    private void Awake()
    {
        if (charListParent == null)
        {
            Debug.LogError("[WordSearchStateManager] charListParent is not assigned. Please assign the CharList Transform in the Inspector.");
        }

        BuildReverseIndex();
        ValidateSetup();
    }

    private void Start()
    {

        StartCoroutine(DelayedLoad());
    }

    private IEnumerator DelayedLoad()
    {
        yield return null;
        LoadGame();
    }

    #endregion

    #region Public API

    /// <summary>
    /// Save the current puzzle state to JSON.
    /// </summary>
    [ContextMenu("Save Now")]
    public void SaveGame()
    {
        if (charListParent == null)
        {
            Debug.LogError("[WordSearchStateManager] Cannot save: charListParent is not set.");
            return;
        }

        WordSearchSaveData data = new WordSearchSaveData();

        // 1) Save per-character selection state (by stable CharId)
        var allChars = GetAllCharObjects();
        int missingIdCount = 0;

        foreach (var ch in allChars)
        {
            if (ch == null) continue;

            string id = ch.CharId; // comes from your CharObject (GUID assigned in editor)
            if (string.IsNullOrEmpty(id))
            {
                missingIdCount++;
                continue;
            }

            data.charStates.Add(new CharStateEntry
            {
                id = id,
                isSelected = ch.isCharSelected
            });
        }

        // 2) Store which words are currently complete (derived from state)
        int wordsCount = 0, wordsCompleted = 0;
        foreach (Transform wordEmpty in charListParent)
        {
            var checker = wordEmpty.GetComponent<WordChecker>();
            if (checker == null) continue;

            var charsForWord = GetCharsForWord(wordEmpty, checker).ToList();
            if (charsForWord.Count == 0) continue;

            wordsCount++;

            bool isCompleted = true;
            foreach (var c in charsForWord)
            {
                if (c == null || !c.isCharSelected) { isCompleted = false; break; }
            }

            if (isCompleted)
            {
                data.completedWords.Add(wordEmpty.name);
                wordsCompleted++;
            }
        }

        string json = JsonUtility.ToJson(data, prettyPrint: true);
        try
        {
            File.WriteAllText(SavePath, json);
            Debug.Log($"[WordSearchStateManager] Saved → {SavePath}");
            Debug.Log($"[WordSearchStateManager] Diagnostics: charsFound={allChars.Count}, savedEntries={data.charStates.Count}, missingIds={missingIdCount}, wordsFound={wordsCount}, wordsCompleted={wordsCompleted}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[WordSearchStateManager] Failed to write save file. Exception: {ex}");
        }
    }

    /// <summary>
    /// Load the puzzle state from JSON and re-apply to the scene.
    /// Safe to call on first entry (no file yet).
    /// </summary>
    [ContextMenu("Load Now")]
    public void LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            // First time in scene → nothing to apply; keep defaults.
            Debug.Log("[WordSearchStateManager] No save file found (first time or deleted).");
            return;
        }

        string json;
        try
        {
            json = File.ReadAllText(SavePath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[WordSearchStateManager] Failed to read save file. Exception: {ex}");
            return;
        }

        WordSearchSaveData data = null;
        try
        {
            data = JsonUtility.FromJson<WordSearchSaveData>(json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[WordSearchStateManager] Failed to parse save JSON. Exception: {ex}");
        }

        if (data == null)
        {
            Debug.LogWarning("[WordSearchStateManager] No data to load (parsed null).");
            return;
        }

        // 1) Build ID → CharObject map for fast lookup
        var allChars = GetAllCharObjects();
        var idMap = BuildIdMap(allChars); // key: CharId, value: CharObject

        // 2) Apply per-character selection states and base colors
        int applied = 0, missing = 0;
        foreach (var entry in data.charStates)
        {
            if (entry == null || string.IsNullOrEmpty(entry.id)) continue;

            if (!idMap.TryGetValue(entry.id, out var ch) || ch == null)
            {
                missing++;
                continue;
            }

            ch.isCharSelected = entry.isSelected;
            if (entry.isSelected) ch.TextSelectedColor();
            else ch.TextOriginalColor();

            applied++;
        }

        // 3) Recompute per-word completion and update visuals (green color + strike-through)
        int wordsComplete = 0;
        foreach (Transform wordEmpty in charListParent)
        {
            var checker = wordEmpty.GetComponent<WordChecker>();
            if (checker == null) continue;

            var charsForWord = GetCharsForWord(wordEmpty, checker).ToList();
            if (charsForWord.Count == 0) continue;

            bool isCompleted = true;
            foreach (var c in charsForWord)
            {
                if (c == null || !c.isCharSelected) { isCompleted = false; break; }
            }

            if (isCompleted)
            {
                foreach (var c in charsForWord)
                {
                    if (c == null) continue;
                    c.TextCompletedColor();
                    // DO NOT set interactable here; handle shared-letter logic globally below.
                }

                checker.wordOnList?.LineEnabled();
                wordsComplete++;
            }
        }

        // 4) Finally, recompute interactable state globally (shared-letter-aware)
        RecomputeInteractability();

        Debug.Log($"[WordSearchStateManager] State restored. Diagnostics: charsInScene={allChars.Count}, loadedEntries={data.charStates.Count}, appliedToScene={applied}, missingInScene={missing}, wordsCompleted={wordsComplete}");
    }

    /// <summary>
    /// Deletes the current save file. Useful during development/testing.
    /// </summary>
    [ContextMenu("Delete Save")]
    public void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            try
            {
                File.Delete(SavePath);
                Debug.Log($"[WordSearchStateManager] Deleted save at {SavePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WordSearchStateManager] Failed to delete save file. Exception: {ex}");
            }
        }
        else
        {
            Debug.Log("[WordSearchStateManager] No save file to delete.");
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor-only: Fix any missing CharIds in the scene (for existing CharObjects that didn't run OnValidate yet).
    /// </summary>
    [ContextMenu("Fix Missing Char IDs (Editor only)")]
    public void FixMissingCharIdsInScene()
    {
        int fixedCount = 0;
        foreach (var ch in GetAllCharObjects())
        {
            if (ch == null) continue;

            var so = new SerializedObject(ch);
            var idProp = so.FindProperty("charId");
            if (idProp != null && string.IsNullOrEmpty(idProp.stringValue))
            {
                idProp.stringValue = GUID.Generate().ToString();
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(ch);
                fixedCount++;
            }
        }
        if (fixedCount > 0)
            Debug.Log($"[WordSearchStateManager] Fixed {fixedCount} missing CharIds in scene.");
        else
            Debug.Log("[WordSearchStateManager] No missing CharIds found.");
    }
#endif

    /// <summary>
    /// Recalculates whether each character button should be interactable,
    /// considering that a shared letter should only lock when ALL words that use it are complete.
    /// </summary>
    public void RecomputeInteractability()
    {
        // Ensure we have a valid reverse index
        if (charToWords.Count == 0) BuildReverseIndex();

        var allChars = GetAllCharObjects();

        foreach (var ch in allChars)
        {
            if (ch == null || ch._button == null)
                continue;

            if (!charToWords.TryGetValue(ch, out var wordsUsingChar) || wordsUsingChar == null || wordsUsingChar.Count == 0)
            {
                // Character is not part of any tracked word; keep it interactable
                ch._button.interactable = true;
                continue;
            }

            bool allWordsThatUseCharAreComplete = true;

            foreach (var w in wordsUsingChar)
            {
                if (w == null)
                {
                    allWordsThatUseCharAreComplete = false;
                    break;
                }

                var charsForWord = GetCharsForWord(w.transform, w).ToList();
                if (charsForWord.Count == 0)
                {
                    allWordsThatUseCharAreComplete = false;
                    break;
                }

                bool wordComplete = true;
                foreach (var c in charsForWord)
                {
                    if (c == null || !c.isCharSelected) { wordComplete = false; break; }
                }

                if (!wordComplete)
                {
                    allWordsThatUseCharAreComplete = false;
                    break;
                }
            }

            // Disable the char button only if ALL words that use it are complete.
            ch._button.interactable = !allWordsThatUseCharAreComplete;

            // If we disable, make sure the color is "completed" green for clarity.
            if (!ch._button.interactable)
            {
                ch.TextCompletedColor();
            }
        }
    }

    #endregion

    #region Internal Helpers

    /// <summary>
    /// Build a reverse index of CharObject -> WordChecker(s) that include it.
    /// Uses checker.characters[] if provided; otherwise falls back to scanning children.
    /// </summary>
    private void BuildReverseIndex()
    {
        charToWords.Clear();

        if (charListParent == null) return;

        foreach (Transform wordEmpty in charListParent)
        {
            var checker = wordEmpty.GetComponent<WordChecker>();
            if (checker == null) continue;

            foreach (var ch in GetCharsForWord(wordEmpty, checker))
            {
                if (ch == null) continue;

                if (!charToWords.ContainsKey(ch))
                    charToWords[ch] = new List<WordChecker>();

                // Avoid duplicate entries for the same (word, char) pair
                if (!charToWords[ch].Contains(checker))
                    charToWords[ch].Add(checker);
            }
        }
    }

    /// <summary>
    /// Returns all CharObjects used by this word.
    /// If WordChecker.characters is missing/empty, it falls back to scanning the wordEmpty’s children.
    /// </summary>
    private IEnumerable<CharObject> GetCharsForWord(Transform wordEmpty, WordChecker checker)
    {
        if (checker != null && checker.characters != null && checker.characters.Length > 0)
        {
            foreach (var c in checker.characters)
                if (c != null) yield return c;
            yield break;
        }

        // Fallback: scan for CharObject under this word empty
        var found = wordEmpty.GetComponentsInChildren<CharObject>(includeInactive: true);
        foreach (var c in found)
            if (c != null) yield return c;
    }

    /// <summary>
    /// Collect all unique CharObjects across all words.
    /// </summary>
    private List<CharObject> GetAllCharObjects()
    {
        var set = new HashSet<CharObject>();

        if (charListParent == null) return set.ToList();

        foreach (Transform wordEmpty in charListParent)
        {
            var checker = wordEmpty.GetComponent<WordChecker>();
            if (checker == null) continue;

            foreach (var c in GetCharsForWord(wordEmpty, checker))
            {
                if (c != null) set.Add(c);
            }
        }

        return set.ToList();
    }

    private Dictionary<string, CharObject> BuildIdMap(List<CharObject> allChars)
    {
        var dict = new Dictionary<string, CharObject>();

        foreach (var ch in allChars)
        {
            if (ch == null) continue;

            string id = ch.CharId;
            if (!string.IsNullOrEmpty(id) && !dict.ContainsKey(id))
                dict.Add(id, ch);
        }

        return dict;
    }

    /// <summary>
    /// Sanity checks at startup to help catch wiring issues.
    /// </summary>
    private void ValidateSetup()
    {
        if (charListParent == null)
        {
            Debug.LogError("[WordSearchStateManager] ValidateSetup: charListParent is NULL.");
            return;
        }

        int wordEmpties = 0;
        int wordsWithChars = 0;
        int totalChars = 0;
        int missingId = 0;

        foreach (Transform wordEmpty in charListParent)
        {
            var checker = wordEmpty.GetComponent<WordChecker>();
            if (checker == null) continue;

            wordEmpties++;

            var charsForWord = GetCharsForWord(wordEmpty, checker).ToList();
            if (charsForWord.Count > 0) wordsWithChars++;
            totalChars += charsForWord.Count;

            foreach (var c in charsForWord)
            {
                if (c == null) continue;
                if (string.IsNullOrEmpty(c.CharId)) missingId++;
            }
        }

        Debug.Log($"[WordSearchStateManager] Validate: wordEmpties={wordEmpties}, wordsWithChars={wordsWithChars}, totalCharsLinked={totalChars}, charsMissingId={missingId}");
        if (missingId > 0)
        {
#if UNITY_EDITOR
            Debug.LogWarning("[WordSearchStateManager] Some CharObjects have missing IDs. Use context menu 'Fix Missing Char IDs (Editor only)' on this component.");
#else
            Debug.LogWarning("[WordSearchStateManager] Some CharObjects have missing IDs (cannot auto-fix at runtime).");
#endif
        }
    }

    #endregion
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class SaveSystem
{
    // Use the filename GameManager already cleans up (Editor + Build).
    // Change this if you want saves to persist across app restarts (and remove it from GameManager cleanup).
    private const string FileName = "wordsearch_default.json";

    private static string FilePath =>
        Path.Combine(Application.persistentDataPath, FileName);

    /// <summary>Collects state from the current scene and writes it to JSON.</summary>
    public static void SaveWordSearch()
    {
        var allChars = Object.FindObjectsOfType<CharObject>(true);
        var save = new WordSearchSave
        {
            chars = allChars.Select(c => new CharSave
            {
                id = c.CharId,
                selected = c.isCharSelected,
                interactable = (c._button != null) ? c._button.interactable : true
            }).ToList()
        };
        // Find PanelOperator to capture panel state
        var po = Object.FindObjectsOfType<PanelOperatorMulti>(true).FirstOrDefault();
        if (po != null)
        {
            for (int i = 0; i < po.isPanelActiveArray.Length; i++)
            {
                if (po.isPanelActiveArray[i])
                {
                    save.activePanelIndex = i;
                    break;
                }
            }
        }

        var json = JsonUtility.ToJson(save, prettyPrint: true);
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
        File.WriteAllText(FilePath, json);
#if UNITY_EDITOR
        Debug.Log($"[SaveSystem] Saved wordsearch to: {FilePath}\n{json}");
#endif
    }

    /// <summary>Reads JSON (if present) and applies to scene. Returns true if data was loaded.</summary>
    public static bool LoadWordSearch()
    {
        if (!File.Exists(FilePath)) return false;

        var json = File.ReadAllText(FilePath);
        var data = JsonUtility.FromJson<WordSearchSave>(json);
        if (data == null || data.chars == null) return false;

        // Fast lookup by id
        var dict = new Dictionary<string, CharSave>(data.chars.Count);
        foreach (var c in data.chars)
        {
            if (!string.IsNullOrEmpty(c.id) && !dict.ContainsKey(c.id))
                dict.Add(c.id, c);
        }

        // Apply character states + visuals
        var allChars = Object.FindObjectsOfType<CharObject>(true);
        foreach (var ch in allChars)
        {
            if (dict.TryGetValue(ch.CharId, out var s))
            {
                ch.isCharSelected = s.selected;

                // Visuals derived from logical state:
                if (s.interactable)
                {
                    if (ch.isCharSelected) ch.TextSelectedColor();
                    else ch.TextOriginalColor();
                }
                else
                {
                    // Part of a completed word
                    ch.TextCompletedColor();
                }

                if (ch._button != null) ch._button.interactable = s.interactable;
            }
            else
            {
                // No saved entry → leave defaults; re-apply current visuals
                if (ch.isCharSelected) ch.TextSelectedColor();
                else ch.TextOriginalColor();
            }
        }

        // Re-check words to re-enable strikethrough + finalize completed visuals
        var allWords = Object.FindObjectsOfType<WordChecker>(true);
        foreach (var w in allWords)
            w.IsWordComplete();

        // Restore panel state
        var po = Object.FindObjectsOfType<PanelOperator>(true).FirstOrDefault();
        if (po != null)
        {
            // Close all panels first
            for (int j = 0; j < po.panelArray.Length; j++)
            {
                po.SetPanelInactive(j);
            }

            if (data.activePanelIndex >= 0 && data.activePanelIndex < po.panelArray.Length)
            {
                // Reopen the correct panel
                po.SetPanelActive(data.activePanelIndex);
            }
            else
            {
                // No panel was open -> show puzzle normally
                po.UnhideSceneObjects();
            }
        }

#if UNITY_EDITOR
        Debug.Log($"[SaveSystem] Loaded wordsearch from: {FilePath}\n{json}");
#endif
        return true;
    }

    public static void ClearWordSearch()
    {
        if (File.Exists(FilePath)) File.Delete(FilePath);
#if UNITY_EDITOR
        Debug.Log($"[SaveSystem] Cleared save at: {FilePath}");
#endif
    }
}

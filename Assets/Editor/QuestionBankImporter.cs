#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class QuestionBankImporter : EditorWindow
{
    private TextAsset sourceText;
    private QuestionBank targetBank;
    private bool clearExisting = true;

    [MenuItem("Tools/Directional Lock/Import Question List")]
    public static void ShowWindow()
    {
        GetWindow<QuestionBankImporter>("QuestionBank Importer");
    }

    void OnGUI()
    {
        GUILayout.Label("Import Questions into QuestionBank", EditorStyles.boldLabel);

        sourceText = (TextAsset)EditorGUILayout.ObjectField("Source Text File", sourceText, typeof(TextAsset), false);
        targetBank = (QuestionBank)EditorGUILayout.ObjectField("Target QuestionBank", targetBank, typeof(QuestionBank), false);

        clearExisting = EditorGUILayout.Toggle("Clear Existing Questions", clearExisting);

        EditorGUILayout.Space();

        if (GUILayout.Button("Import"))
        {
            if (sourceText == null || targetBank == null)
            {
                EditorUtility.DisplayDialog("Missing Field", "Assign both a Source TextAsset and a Target QuestionBank.", "OK");
                return;
            }

            ImportNow(sourceText.text, targetBank, clearExisting);
        }
    }

    private static void ImportNow(string text, QuestionBank bank, bool clear)
    {
        if (clear)
            bank.questions.Clear();

        // Normalize line endings
        text = text.Replace("\r\n", "\n").Replace("\r", "\n");

        // Detect sections by headings
        QuestionBank.Bucket current = QuestionBank.Bucket.Safe; // default if no heading is found

        // Matches lines like "1. Question text"
        Regex numbered = new Regex(@"^\s*\d+\.\s*(.+?)\s*$");

        foreach (var rawLine in text.Split('\n'))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            // Headings
            string upper = line.ToUpperInvariant();
            if (upper.Contains("UNSAFE"))
            {
                current = QuestionBank.Bucket.Unsafe;
                continue;
            }
            if (upper.Contains("SAFER"))
            {
                current = QuestionBank.Bucket.Safer;
                continue;
            }
            if (upper.Contains("CLEARLY SAFE") || (upper.Contains("SAFE") && !upper.Contains("UNSAFE") && !upper.Contains("SAFER")))
            {
                current = QuestionBank.Bucket.Safe;
                continue;
            }

            // Ignore divider lines like "---"
            if (line.StartsWith("---")) continue;

            // Pull numbered questions
            var match = numbered.Match(line);
            if (!match.Success) continue;

            string qText = match.Groups[1].Value.Trim();

            // Remove trailing double spaces etc
            qText = Regex.Replace(qText, @"\s+", " ");

            bank.questions.Add(new QuestionBank.Q
            {
                text = qText,
                bucket = current
            });
        }

        EditorUtility.SetDirty(bank);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("Import Complete",
            $"Imported {bank.questions.Count} questions into {bank.name}.", "OK");
    }
}
#endif
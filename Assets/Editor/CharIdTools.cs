#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class CharIdTools
{
    [MenuItem("Tools/WordSearch/Fix Char IDs (Active Scene)")]
    public static void FixCharIdsInActiveScene()
    {
        var chars = Object.FindObjectsOfType<CharObject>(true);
        var seen = new HashSet<string>();
        int fixedCount = 0;

        Undo.IncrementCurrentGroup();
        foreach (var ch in chars)
        {
            var so = new SerializedObject(ch);
            var idProp = so.FindProperty("charId");

            bool needsNew = string.IsNullOrEmpty(idProp.stringValue) || seen.Contains(idProp.stringValue);
            if (needsNew)
            {
                Undo.RecordObject(ch, "Assign New CharId");
                idProp.stringValue = System.Guid.NewGuid().ToString("N"); // hyphen-less
                so.ApplyModifiedProperties();
                fixedCount++;
            }

            seen.Add(idProp.stringValue);
            EditorUtility.SetDirty(ch);
        }
        Debug.Log($"[CharIdTools] Fixed {fixedCount} CharObject IDs. Total Chars: {chars.Length}, Unique IDs: {seen.Count}");
    }
}
#endif
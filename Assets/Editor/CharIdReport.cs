#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class CharIdReport
{
    [MenuItem("Tools/WordSearch/Report Char ID Duplicates (Active Scene)")]
    public static void Report()
    {
        var chars = Object.FindObjectsOfType<CharObject>(true);
        var groups = chars.GroupBy(c => c.CharId)
                          .Where(g => !string.IsNullOrEmpty(g.Key) && g.Count() > 1)
                          .ToList();

        if (groups.Count == 0)
        {
            Debug.Log("[CharIdReport] No duplicate CharIds found.");
            return;
        }

        foreach (var g in groups)
        {
            Debug.Log($"[CharIdReport] ID '{g.Key}' appears {g.Count()} times:");
            foreach (var c in g)
                Debug.Log($"  - {GetPath(c.transform)}", c.gameObject);
        }
    }

    private static string GetPath(Transform t)
    {
        var path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
#endif
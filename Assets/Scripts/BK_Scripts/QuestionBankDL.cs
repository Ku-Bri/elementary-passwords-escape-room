using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DirectionalLock/QuestionBank")]
public class QuestionBank : ScriptableObject
{
    public enum Bucket
    {
        Unsafe = 0,
        Safe = 1,
        Safer = 2 // still "SAFE" for students, but trickier prompts
    }

    [System.Serializable]
    public class Q
    {
        [TextArea(2, 4)]
        public string text;

        public Bucket bucket;

        // Students only choose SAFE vs UNSAFE:
        public bool IsSafeForStudent => bucket != Bucket.Unsafe;
    }

    public List<Q> questions = new List<Q>();
}
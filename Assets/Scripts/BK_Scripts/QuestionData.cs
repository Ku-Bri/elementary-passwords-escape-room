using UnityEngine;

[System.Serializable]
public class TrueFalsQuestion
{
    [TextArea(2, 5)] public string questionText;
    public bool correctAnswer;
}

[CreateAssetMenu(fileName = "QuestionSet", menuName = "Quiz/TrueFalseQuestionSet")]
public class QuestionData : ScriptableObject
{
    public TrueFalsQuestion[] questions;
}
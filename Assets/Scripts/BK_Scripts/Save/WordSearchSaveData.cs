
using System;
using System.Collections.Generic;

[Serializable]
public class WordSearchSaveData
{
    // Use a list because Unity's JsonUtility does not serialize Dictionary
    public List<CharStateEntry> charStates = new List<CharStateEntry>();

    // Optional: store which words were completed (by the empty word GameObject name)
    public List<string> completedWords = new List<string>();
}

[Serializable]
public class CharStateEntry
{
    public string id;        // CharObject.CharId
    public bool isSelected;  // true if the char is currently selected
}

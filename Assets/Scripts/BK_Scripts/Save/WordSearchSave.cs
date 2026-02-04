using System;
using System.Collections.Generic;

[Serializable]
public class CharSave
{
    public string id;
    public bool selected;      // maps to CharObject.isCharSelected
    public bool interactable;  // maps to Button.interactable (false => completed)
}

[Serializable]
public class WordSearchSave
{
    public List<CharSave> chars = new List<CharSave>();

    public int activePanelIndex = -1; // -1 means no panel open
}

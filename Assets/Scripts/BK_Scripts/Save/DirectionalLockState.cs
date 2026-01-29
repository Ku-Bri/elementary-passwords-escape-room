using System;
using System.Collections.Generic;

[Serializable]
public class DirectionalLockState
{
    public int gridSize;
    public int seed;

    public int startIndex;
    public int endIndex;

    // Correct path from start -> end inclusive (indices in order)
    public List<int> solutionPath = new List<int>();

    // The "directional code" derived from solutionPath (e.g., "URRDL")
    public string expectedCode;

    // Per-cell prompts (same length as gridSize*gridSize). Start/End will be empty.
    public List<string> prompts = new List<string>();

    // True for tiles on the solution path (including endpoints, but manager treats endpoints as labels)
    public List<bool> isSafeSolution = new List<bool>();

    // Fog-of-war: whether each cell has been revealed
    public List<bool> revealed = new List<bool>();

    // Player marks: -1 blank, 0 marked UNSAFE, 1 marked SAFE
    public List<int> playerMark = new List<int>();
}
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(GridLayoutGroup))]
public class AutoGridCellSizer : MonoBehaviour
{
    public int gridSize = 5;         // 4 or 5
    public float extraPadding = 0f;  // optional additional breathing room

    private GridLayoutGroup grid;
    private RectTransform rt;

    void Awake()
    {
        grid = GetComponent<GridLayoutGroup>();
        rt = GetComponent<RectTransform>();
    }

    void OnEnable() => Recalculate();
    void Start() => Recalculate();

    void OnRectTransformDimensionsChange()
    {
        // Called when the rect changes (screen resize, layout updates, etc.)
        Recalculate();
    }

    void LateUpdate()
    {
        // In ExecuteAlways / Editor, layout updates can happen after change events.
        // This keeps it stable without heavy cost.
        Recalculate();
    }

    private void Recalculate()
    {
        if (grid == null || rt == null) return;

        // Force fixed columns
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = gridSize;

        float width = rt.rect.width;
        float height = rt.rect.height;

        // If size isn't ready yet, bail (prevents zero/negative sizing → overlap)
        if (width <= 1f || height <= 1f) return;

        // Subtract GridLayoutGroup padding + spacing
        float usableW = width - grid.padding.left - grid.padding.right - extraPadding * 2f;
        float usableH = height - grid.padding.top - grid.padding.bottom - extraPadding * 2f;

        float totalSpacingW = grid.spacing.x * (gridSize - 1);
        float totalSpacingH = grid.spacing.y * (gridSize - 1);

        usableW -= totalSpacingW;
        usableH -= totalSpacingH;

        // Prevent negative/zero usable area
        if (usableW <= 1f || usableH <= 1f) return;

        float cell = Mathf.Floor(Mathf.Min(usableW / gridSize, usableH / gridSize));
        cell = Mathf.Max(cell, 10f); // safety floor

        grid.cellSize = new Vector2(cell, cell);
    }
}
using UnityEngine;

public class CellIconScaler : MonoBehaviour
{
    public RectTransform markIcon;
    [Range(0.1f, 0.8f)] public float iconPercent = 0.35f;

    private RectTransform rt;

    void Awake() => rt = GetComponent<RectTransform>();

    void OnRectTransformDimensionsChange()
    {
        if (rt == null || markIcon == null) return;

        float size = rt.rect.width * iconPercent;
        markIcon.sizeDelta = new Vector2(size, size);
    }
}
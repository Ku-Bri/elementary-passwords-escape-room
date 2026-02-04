using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DirectionalLockCell : MonoBehaviour
{
    [Header("UI")]
    public Button button;
    public Image background;
    public TMP_Text label;          // START/END text
    public Image fogOverlay;        // shown until answered
    public Image markIcon;          // ✔ or ✖

    [HideInInspector] public int index;

    private DirectionalLockManagerChoice manager;

    public void Init(DirectionalLockManagerChoice mgr, int idx)
    {
        manager = mgr;
        index = idx;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => manager.OnCellClicked(index));
    }

    public void SetInteractable(bool on)
    {
        if (button != null) button.interactable = on;
    }

    public void SetLabel(string txt)
    {
        if (label != null) label.text = txt;
    }

    public void SetFog(bool showFog)
    {
        if (fogOverlay != null) fogOverlay.gameObject.SetActive(showFog);
    }

    public void SetMark(Sprite sprite, bool visible)
    {
        if (markIcon == null) return;

        markIcon.sprite = sprite;
        markIcon.gameObject.SetActive(visible);
    }
}
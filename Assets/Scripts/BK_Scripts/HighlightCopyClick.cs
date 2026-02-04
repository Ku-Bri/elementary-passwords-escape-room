using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_InputField))]
public class HighlightCopyClick : MonoBehaviour,
    IPointerDownHandler, IPointerClickHandler, ISelectHandler
{
    [Header("Behavior")]
    [Tooltip("Select all + copy as soon as you press down.")]
    public bool selectAllAndCopyOnPointerDown = true;

    [Tooltip("Also select all when the field receives focus (keyboard/gamepad navigation).")]
    public bool selectAllOnFocus = true;

    [Tooltip("Keep highlight visible until user clicks somewhere else.")]
    public bool keepHighlightedUntilClickElsewhere = true;

    [Header("Visuals")]
    [Tooltip("Selection highlight color.")]
    public Color selectionColor = new Color(0.2f, 0.7f, 1f, 0.35f);

    [Tooltip("Caret color (alpha 0 = invisible).")]
    public Color caretColor = new Color(1f, 1f, 1f, 0f);

    [Header("Copied! Flash")]
    [Tooltip("Optional TMP_Text to show a COPIED! message (recommended).")]
    public TMP_Text copiedLabel;
    public TextMeshProUGUI codeLabel;

    [Tooltip("Message to show when copied.")]
    public string copiedMessage = "COPIED!";

    [Tooltip("How long to show the COPIED! message (seconds).")]
    public float copiedFlashSeconds = 0.8f;

    [Tooltip("If true, fades the message out instead of hard on/off.")]
    public bool fadeCopiedMessage = true;

    private TMP_InputField input;
    private Coroutine selectRoutine;
    private Coroutine copiedRoutine;

    private void Awake()
    {
        input = GetComponent<TMP_InputField>();

        // Make this behave like a copy-only box
        input.readOnly = true;
        input.interactable = true;

        // Appearance
        input.selectionColor = selectionColor;
        input.caretColor = caretColor;
        input.caretWidth = 0;

        // Avoid copying TMP rich-text tags if any
        if (input.textComponent != null)
            input.textComponent.richText = false;

        // Initialize copied label state
        if (copiedLabel != null)
        {
            copiedLabel.gameObject.SetActive(false);
        }
    }

    // Most consistent: fires immediately on mouse down / touch begin
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!selectAllAndCopyOnPointerDown) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;

        Focus();
        SelectAllSafe();
        CopyToClipboard();

        if (keepHighlightedUntilClickElsewhere)
            eventData.Use();
    }

    // Some UI setups fire Click without Down; keep as backup
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        Focus();
        SelectAllSafe();
        CopyToClipboard();

        if (keepHighlightedUntilClickElsewhere)
            eventData.Use();
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (!selectAllOnFocus) return;
        SelectAllSafe();
    }

    private void Focus()
    {
        EventSystem.current?.SetSelectedGameObject(gameObject);
        input.ActivateInputField();
    }

    private void SelectAllSafe()
    {
        if (selectRoutine != null) StopCoroutine(selectRoutine);
        selectRoutine = StartCoroutine(SelectAllNextFrame());
    }

    private IEnumerator SelectAllNextFrame()
    {
        // Wait one frame so TMP has applied focus/activation
        yield return null;

        int len = (input.text != null) ? input.text.Length : 0;

        // Public TMP API (avoids TMP_InputField.SelectAll() accessibility)
        input.selectionAnchorPosition = 0;
        input.selectionFocusPosition = len;
        input.caretPosition = len;
        input.ForceLabelUpdate();
    }

    private void CopyToClipboard()
    {
        GUIUtility.systemCopyBuffer = input.text;
        //Debug.Log($"[CopyBox] Copied to clipboard: {input.text}");

        FlashCopiedMessage();
    }

    private void FlashCopiedMessage()
    {
        if (copiedLabel == null) return;

        if (copiedRoutine != null) StopCoroutine(copiedRoutine);
        copiedRoutine = StartCoroutine(CopiedMessageRoutine());
    }

    private IEnumerator CopiedMessageRoutine()
    {
        copiedLabel.text = copiedMessage;
        codeLabel.gameObject.SetActive(false);
        copiedLabel.gameObject.SetActive(true);

        if (!fadeCopiedMessage)
        {
            yield return new WaitForSeconds(copiedFlashSeconds);
            copiedLabel.gameObject.SetActive(false);
            codeLabel.gameObject.SetActive(true);
            yield break;
        }

        // Fade out over copiedFlashSeconds
        Color c = copiedLabel.color;
        float t = 0f;

        // Ensure fully visible at start
        copiedLabel.color = new Color(c.r, c.g, c.b, 1f);

        while (t < copiedFlashSeconds)
        {
            t += Time.unscaledDeltaTime; // unaffected by Time.timeScale
            float a = Mathf.Lerp(1f, 0f, t / copiedFlashSeconds);
            copiedLabel.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        copiedLabel.gameObject.SetActive(false);
        codeLabel.gameObject.SetActive(true);

        // Restore original alpha (so next time starts from visible)
        copiedLabel.color = new Color(c.r, c.g, c.b, 1f);
    }

    /// <summary>
    /// Optional: hook this to a UI "Copy" button too.
    /// </summary>
    public void CopyNow()
    {
        CopyToClipboard();
    }
}
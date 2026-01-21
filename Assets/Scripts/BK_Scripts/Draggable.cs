using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private bool isOverTarget = false;

    public TextMeshProUGUI website;
    public TextMeshProUGUI username;
    public TextMeshProUGUI password;

    public PasswordManager passwordManager;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData) 
    {
        GetComponent<RectTransform>().SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform.parent.GetComponent<RectTransform>(),
            eventData.position, eventData.pressEventCamera, out position);

        rectTransform.localPosition = position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isOverTarget)
        {
            passwordManager.Populate(website.text,  username.text, password.text);
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PasswordManager"))
        {
            isOverTarget = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("PasswordManager"))
        {
            isOverTarget = false;
        }
    }
}

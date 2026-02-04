using UnityEngine;
using UnityEngine.EventSystems;

public class ForwardPointerToParent : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.pointerDownHandler);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.pointerClickHandler);
    }
}

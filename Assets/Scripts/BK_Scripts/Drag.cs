using UnityEngine;
using UnityEngine.EventSystems;

public interface IDrag
{
    void OnCurrentDrag();
}
public class Drag : MonoBehaviour, IDragHandler, IPointerUpHandler
{
    [SerializeField] GameObject objectToInteractWith;
    private IDrag onDrag;

    [SerializeField]
    private bool collided = false;

    // Start is called before the first frame update
    void Start()
    {
        onDrag = objectToInteractWith.GetComponent<IDrag>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        onDrag.OnCurrentDrag();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Entered OnPointerUp()");
        if (collided)
        {
            Debug.Log("Inside PointerUp if collided");
            Destroy(this.gameObject);
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("Collision Detected");
        if (other.gameObject.CompareTag("PasswordManager"))
        {
            collided = true;
            //Debug.Log("collided set to true");
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        //Debug.Log("Collision Exited");
        if (other.gameObject.CompareTag("PasswordManager"))
        {
            collided = false;
            //Debug.Log("collided set to false");
        }
    }
}

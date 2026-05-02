using UnityEngine;

public abstract class WorldItem : MonoBehaviour
{
    //change this if you want to adjust the scale of world items without changing the held scale
    private const float WORLD_SCALE_MULTIPLIER = 1f;

    [SerializeField] protected ItemData itemData;
    [SerializeField] protected Vector3 heldScale;

    protected Interactable interactable;
    public bool isHeld;

    protected virtual void Start()
    {
        interactable = GetComponent<Interactable>();

        if (interactable == null)
        {
            Debug.LogError("WorldItem requires an Interactable component!");
            return;
        }

        interactable.OnInteract.AddListener(PickUp);

        heldScale = transform.localScale;
        if (!isHeld && transform.parent == null)
        {
            transform.localScale = heldScale * WORLD_SCALE_MULTIPLIER;
        }
            
    }

    protected virtual void PickUp(GameObject picker)
    {
        Inventory inventory = picker.GetComponent<Inventory>();

        if (inventory != null)
        {
            if (inventory.AddItem(itemData))
            {
                //isHeld = true;
                Destroy(gameObject);
            }
        }
    }

    public void SetToWorldScale()
    {
        transform.localScale = heldScale * WORLD_SCALE_MULTIPLIER;
    }
}
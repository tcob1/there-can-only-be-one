using UnityEngine;

public abstract class WorldItem : MonoBehaviour
{
    [SerializeField] protected ItemData itemData;

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
}
using UnityEngine;

public class WorldItem : MonoBehaviour
{
    public ItemData itemData;

    private Interactable interactable;

    void Start()
    {
        interactable = GetComponent<Interactable>();
        if (interactable == null)
        {
            Debug.LogError("WorldItem requires an Interactable component!");
            return;
        }

        interactable.OnInteract.AddListener(PickUp);
    }

    public void PickUp(GameObject picker)
    {
        Inventory inventory = picker.GetComponent<Inventory>();
        if (inventory != null)
        {
            // Don't destroy the item if it can't be added to the inventory!
            if (inventory.AddItem(itemData))
            {
                Destroy(gameObject);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractions : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private float interactionRange = 3f;

    private InputAction interactAction;
    private InputAction dropAction;

    private Inventory inventory;

    private int layerMask;

    void Start()
    {
        interactAction = InputSystem.actions.FindAction("Interact");
        interactAction.started += ctx => SendInteractionRay();
        interactAction.Enable();

        dropAction = InputSystem.actions.FindAction("Drop");
        dropAction.started += ctx => DropFirstInventorySlot();
        dropAction.Enable();

        inventory = player.GetComponent<Inventory>();

        layerMask = LayerMask.GetMask("Interactable");
    }

    void SendInteractionRay()
    {
        Ray interactionRay = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(interactionRay, out RaycastHit hitInfo, interactionRange, layerMask))
        {
            Interactable interactable = hitInfo.collider.GetComponent<Interactable>();
            interactable?.Interact(player);
        }
    }

    void DropFirstInventorySlot()
    {
        if (inventory != null && !inventory.IsEmpty())
        {
            inventory.DropItem(0, player.transform.position + player.transform.forward);
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerInteractions : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private float interactionRange = 3f;

    private InputAction interactAction;
    private InputAction dropAction;
    private InputAction attackAction;

    private Inventory inventory;

    private int layerMask;

    private HashSet<Interactable> hoveredInteractables = new HashSet<Interactable>();

    void Start()
    {
        interactAction = InputSystem.actions.FindAction("Interact");
        interactAction.started += ctx => SendInteractionRay();
        interactAction.Enable();

        dropAction = InputSystem.actions.FindAction("Drop");
        dropAction.started += ctx => DropFirstInventorySlot();
        dropAction.Enable();

        attackAction = InputSystem.actions.FindAction("Attack");
        attackAction.started += ctx => UseEquippedItem();
        attackAction.Enable();

        inventory = player.GetComponent<Inventory>();
        layerMask = LayerMask.GetMask("Interactable") | LayerMask.GetMask("InteractableHover");
        inventory.EquipSlot(0);
    }

    void Update()
    {
        UpdateHovered();

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            inventory.EquipSlot(0);

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            inventory.EquipSlot(1);

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            inventory.EquipSlot(2);

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
            inventory.EquipSlot(3);

        if (Keyboard.current.digit5Key.wasPressedThisFrame)
            inventory.EquipSlot(4);
    }

    private void UseEquippedItem()
    {
        GameObject equippedItem = inventory.GetEquippedItem();
        if (equippedItem == null) return; 

        //if item has usable class, attack
        Usable usable = equippedItem.GetComponent<Usable>();
        usable?.Attack(); 
    }

    private void UpdateHovered()
    {
        if (GetRaycastHit(out RaycastHit hitInfo))
        {
            Interactable interactable = hitInfo.collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                if (!hoveredInteractables.Contains(interactable))
                {

                    hoveredInteractables.Add(interactable);
                    interactable.OnHoverEnter();
                }
            }
        }

        // Remove hover from interactables that are no longer hit
        hoveredInteractables.RemoveWhere(interactable =>
        {
            if (interactable == null) return true; // Remove destroyed interactables
            if (!GetRaycastHit(out RaycastHit hitInfo) || hitInfo.collider.GetComponent<Interactable>() != interactable)
            {
                interactable.OnHoverExit();
                return true;
            }
            return false;
        });
    }

    private void SendInteractionRay()
    {
        if (GetRaycastHit(out RaycastHit hitInfo))
        {
            Interactable interactable = hitInfo.collider.GetComponent<Interactable>();
            interactable?.Interact(player);
        }
    }

    private bool GetRaycastHit(out RaycastHit hitInfo)
    {
        Ray interactionRay = new Ray(transform.position, transform.forward);
        return Physics.Raycast(interactionRay, out hitInfo, interactionRange, layerMask);
    }

    void DropFirstInventorySlot()
    {
        if (inventory != null && !inventory.IsEmpty())
        {
            inventory.DropItem(player.transform.position + player.transform.forward);
        }
    }
}

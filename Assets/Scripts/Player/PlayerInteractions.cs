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

    [SerializeField] private Fists fists;

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

        if (equippedItem != null)
        {
            Weapon usable = equippedItem.GetComponent<Weapon>();
            if (usable != null)
            {
                usable.Attack(gameObject);
                return;
            }
        }
        if (fists != null && fists.gameObject != null)
        {
            fists.Attack(gameObject);
        }
    }

    //update the list of hovered interactables based on raycast results, and call hover highlight, text methods
    private void UpdateHovered()
    {
        Interactable currentInteractable = null;

        if (GetRaycastHit(out RaycastHit hitInfo))
            currentInteractable = hitInfo.collider.GetComponent<Interactable>();

        if (currentInteractable != null)
        {
            if (!hoveredInteractables.Contains(currentInteractable))
            {
                hoveredInteractables.Add(currentInteractable);
                currentInteractable.OnHoverEnter();
            }

            //if the interactable has states, pass the current state to GetHoverText so it can return state-specific text
            StatefulInteractable statefulInteractable = currentInteractable.GetComponentInParent<StatefulInteractable>();
            string state = statefulInteractable?.GetCurrentState();
            string hoverText = currentInteractable.GetHoverText(player, state);
            if (!string.IsNullOrEmpty(hoverText))
                UIManager.Instance.ShowInteractionText(hoverText);
            else
                UIManager.Instance.HideInteractionText();
        }
        else
        {
            UIManager.Instance.HideInteractionText();
        }

        hoveredInteractables.RemoveWhere(interactable =>
        {
            if (interactable == null) return true;
            if (interactable != currentInteractable)
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

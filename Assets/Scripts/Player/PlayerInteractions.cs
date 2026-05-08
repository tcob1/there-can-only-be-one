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
    private InputAction scrollAction;
    private InputAction slot1Action, slot2Action, slot3Action, slot4Action, slot5Action;

    private Inventory inventory;
    [SerializeField] private TimetravelerInputs timetravelerInputs;

    private int layerMask;

    private Interactable currentHovered = null;

    [SerializeField] private Fists fists;

    void Start()
    {
        inventory = player.GetComponent<Inventory>();

        //actions
        interactAction = InputSystem.actions.FindAction("Interact");
        interactAction.started += ctx => SendInteractionRay();
        interactAction.Enable();

        dropAction = InputSystem.actions.FindAction("Drop");
        dropAction.started += ctx => DropFirstInventorySlot();
        dropAction.Enable();

        attackAction = InputSystem.actions.FindAction("Attack");
        attackAction.started += ctx => UseEquippedItem();
        attackAction.Enable();

        layerMask = LayerMask.GetMask("Interactable") | LayerMask.GetMask("InteractableHover");

        //navigating inventory
        scrollAction = InputSystem.actions.FindAction("ScrollSlot");
        scrollAction.Enable();

        slot1Action = InputSystem.actions.FindAction("Slot1");
        slot1Action.started += ctx => inventory.EquipSlot(0);
        slot1Action.Enable();

        slot2Action = InputSystem.actions.FindAction("Slot2");
        slot2Action.started += ctx => inventory.EquipSlot(1);
        slot2Action.Enable();

        slot3Action = InputSystem.actions.FindAction("Slot3");
        slot3Action.started += ctx => inventory.EquipSlot(2);
        slot3Action.Enable();

        slot4Action = InputSystem.actions.FindAction("Slot4");
        slot4Action.started += ctx => inventory.EquipSlot(3);
        slot4Action.Enable();

        slot5Action = InputSystem.actions.FindAction("Slot5");
        slot5Action.started += ctx => inventory.EquipSlot(4);
        slot5Action.Enable();

        inventory.EquipSlot(0);
    }

    void OnEnable()
    {
        interactAction?.Enable();
        dropAction?.Enable();
        attackAction?.Enable();
        scrollAction?.Enable();
        slot1Action?.Enable();
        slot2Action?.Enable();
        slot3Action?.Enable();
        slot4Action?.Enable();
        slot5Action?.Enable();
    }

    void OnDisable()
    {
        interactAction?.Disable();
        dropAction?.Disable();
        attackAction?.Disable();
        scrollAction?.Disable();
        slot1Action?.Disable();
        slot2Action?.Disable();
        slot3Action?.Disable();
        slot4Action?.Disable();
        slot5Action?.Disable();
    }

    void Update()
    {
        UpdateHovered();
        HandleScroll();
    }

    private void HandleScroll()
    {
        //no scroll if controlling time travel
        if (timetravelerInputs.chargingTT) return;

        float scroll = scrollAction.ReadValue<Vector2>().y;
        if (scroll == 0)
        {
            return;
        }

        int newSlot = inventory.activeSlotIndex + (scroll < 0 ? -1 : 1);
        //wraparound logic: if newSlot goes below 0 or above max index, wrap it around to the other end of the inventory
        newSlot = (newSlot + inventory.itemSlots.Length) % inventory.itemSlots.Length;
        inventory.EquipSlot(newSlot);
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
        // Unity == null catches destroyed objects
        if (currentHovered == null)
        {
            currentHovered = null; // clear the stale reference
            UIManager.Instance.HideInteractionText();
        }

        Interactable newInteractable = null;

        if (GetRaycastHit(out RaycastHit hitInfo))
            newInteractable = hitInfo.collider.GetComponent<Interactable>();

        if (newInteractable != currentHovered)
        {
            if (currentHovered != null)
            {
                currentHovered.OnHoverExit();
                UIManager.Instance.HideInteractionText();
            }

            currentHovered = newInteractable;

            if (currentHovered != null)
                currentHovered.OnHoverEnter();
        }

        if (currentHovered != null)
        {
            StatefulInteractable stateful = currentHovered.GetComponentInParent<StatefulInteractable>();
            string state = stateful?.GetCurrentState();
            string hoverText = currentHovered.GetHoverText(player, state);
            if (!string.IsNullOrEmpty(hoverText))
                UIManager.Instance.ShowInteractionText(hoverText);
            else
                UIManager.Instance.HideInteractionText();
        }
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

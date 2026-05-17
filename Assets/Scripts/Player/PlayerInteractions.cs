using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using static EventIndicatorUI;

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
    [SerializeField] private EventIndicatorUI eventIndicatorUI;

    private int layerMask;

    private Interactable currentHovered = null;

    [SerializeField] private Fists fists;


    private void OnInteractStarted(InputAction.CallbackContext ctx) => SendInteractionRay();
    private void OnDropStarted(InputAction.CallbackContext ctx) => DropFirstInventorySlot();
    private void OnAttackStarted(InputAction.CallbackContext ctx) => UseEquippedItem();
    private void OnSlot1Started(InputAction.CallbackContext ctx) => inventory.EquipSlot(0);
    private void OnSlot2Started(InputAction.CallbackContext ctx) => inventory.EquipSlot(1);
    private void OnSlot3Started(InputAction.CallbackContext ctx) => inventory.EquipSlot(2);
    private void OnSlot4Started(InputAction.CallbackContext ctx) => inventory.EquipSlot(3);
    private void OnSlot5Started(InputAction.CallbackContext ctx) => inventory.EquipSlot(4);
    void Start()
    {
        inventory = player.GetComponent<Inventory>();
        layerMask = LayerMask.GetMask("Interactable") | LayerMask.GetMask("InteractableHover");

        interactAction = InputSystem.actions.FindAction("Interact");
        interactAction.started += OnInteractStarted;
        interactAction.Enable();

        dropAction = InputSystem.actions.FindAction("Drop");
        dropAction.started += OnDropStarted;
        dropAction.Enable();

        attackAction = InputSystem.actions.FindAction("Attack");
        attackAction.started += OnAttackStarted;
        attackAction.Enable();

        scrollAction = InputSystem.actions.FindAction("ScrollSlot");
        scrollAction.Enable();

        slot1Action = InputSystem.actions.FindAction("Slot1");
        slot1Action.started += OnSlot1Started;
        slot1Action.Enable();

        slot2Action = InputSystem.actions.FindAction("Slot2");
        slot2Action.started += OnSlot2Started;
        slot2Action.Enable();

        slot3Action = InputSystem.actions.FindAction("Slot3");
        slot3Action.started += OnSlot3Started;
        slot3Action.Enable();

        slot4Action = InputSystem.actions.FindAction("Slot4");
        slot4Action.started += OnSlot4Started;
        slot4Action.Enable();

        slot5Action = InputSystem.actions.FindAction("Slot5");
        slot5Action.started += OnSlot5Started;
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

    void OnDestroy()
    {
        if (interactAction != null) interactAction.started -= OnInteractStarted;
        if (dropAction != null) dropAction.started -= OnDropStarted;
        if (attackAction != null) attackAction.started -= OnAttackStarted;
        if (slot1Action != null) slot1Action.started -= OnSlot1Started;
        if (slot2Action != null) slot2Action.started -= OnSlot2Started;
        if (slot3Action != null) slot3Action.started -= OnSlot3Started;
        if (slot4Action != null) slot4Action.started -= OnSlot4Started;
        if (slot5Action != null) slot5Action.started -= OnSlot5Started;
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
        {
            Interactable interactable = hitInfo.collider.GetComponent<Interactable>();
            if (interactable != null && interactable.enabled)
                newInteractable = interactable;
        }

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
        // try logging an event before normal interactions
        BeaconInstance beacon = eventIndicatorUI.GetLoggableBeacon();
        if (beacon != null)
        {
            EventLogger.Instance.Log(new LoggedEvent(
                beacon.description,
                beacon.spawnGameTime,
                beacon.worldPosition
            ));
            eventIndicatorUI.RemoveBeacon(beacon);
            return;
        }

        // normal interaction
        if (GetRaycastHit(out RaycastHit hitInfo))
        {
            Interactable interactable = hitInfo.collider.GetComponent<Interactable>();
            if (interactable != null && interactable.enabled)
            {
                interactable.Interact(player);
            }

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

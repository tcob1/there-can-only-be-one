using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GuardState : MobileInteractable
{
    private GuardNav guardNav;
    private HealthManager healthManager;
    private Inventory inventory;

    void Start()
    {
        guardNav = GetComponent<GuardNav>();
        healthManager = GetComponent<HealthManager>();
        inventory = GetComponent<Inventory>();

        StateRegistry.Instance.Register(this);
    }

    public override Dictionary<string, object> GetState()
    {
        base.SetValue("Target", guardNav.player);
        base.SetValue("TrackingState", guardNav.currentGuardState);
        base.SetValue("Health", healthManager.health);
        // Deep copy so saved snapshots aren't affected by later changes
        InventorySlot[] inventoryCopy = inventory.itemSlots
            .Select(slot => new InventorySlot(slot.itemData, slot.quantity))
            .ToArray();
        base.SetValue("Inventory", inventoryCopy);
        base.SetValue("ActiveSlotIndex", inventory.activeSlotIndex);
        return base.GetState();
    }

    public override void SetState(Dictionary<string, object> newState)
    {
        base.SetState(newState);
        guardNav.player = base.GetValue<GameObject>("Target");
        guardNav.currentGuardState = base.GetValue<GuardNav.GuardState>("TrackingState");
        healthManager.health = base.GetValue<float>("Health");
        inventory.itemSlots = base.GetValue<InventorySlot[]>("Inventory");
        inventory.activeSlotIndex = base.GetValue<int>("ActiveSlotIndex");

        // Re-equip so the held world object matches restored state
        inventory.EquipSlot(inventory.activeSlotIndex);
    }
}

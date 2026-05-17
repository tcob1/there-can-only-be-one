using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;

public class GuardState : MobileInteractable
{
    private GuardNav guardNav;
    private HealthManager healthManager;
    private Inventory inventory;
    private NavMeshAgent agent;

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



    void Start()
    {
        guardNav = GetComponent<GuardNav>();
        healthManager = GetComponent<HealthManager>();
        inventory = GetComponent<Inventory>();
        agent = GetComponent<NavMeshAgent>();
        StateRegistry.Instance.Register(this);
    }

    protected override void ApplyPosition(Vector3 position)
    {
        if (agent != null)
            agent.Warp(position);
        else
            transform.position = position;
    }

    public override string GetCurrentState()
    {
        return guardNav.currentGuardState.ToString();
    }
}

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class NpcState : MobileInteractable
{
    private NpcNav npcNav;
    private NavMeshAgent agent;

    void Start()
    {
        npcNav = GetComponent<NpcNav>();
        agent = GetComponent<NavMeshAgent>();
        StateRegistry.Instance.Register(this);
    }

    public override Dictionary<string, object> GetState()
    {
        base.SetValue("NPCState", npcNav.currentNPCState.ToString());
        return base.GetState();
    }

    public override void SetState(Dictionary<string, object> newState)
    {
        base.SetState(newState);
        if (System.Enum.TryParse(base.GetValue<string>("NPCState"), out NpcNav.NPCState parsedState))
            npcNav.currentNPCState = parsedState;
    }

    protected override void ApplyPosition(Vector3 position)
    {
        if (agent != null && agent.enabled)
            agent.Warp(position);
        else
            transform.position = position;
    }

    public override string GetCurrentState() => npcNav.currentNPCState.ToString();
}
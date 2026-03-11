using UnityEngine;
using System.Collections.Generic;

public class MobileInteractable : StatefulInteractable
{
    private void OnEnable()
    {
        TimeHub.onSecond += NewChange;
    }

    private void OnDisable()
    {
        TimeHub.onSecond -= NewChange;
    }

    protected void NewChange()
    {
        if (TimeHub.Instance != null)
        {
            TimeHub.Instance.logChange(this);
        }
    }

    public override Dictionary<string, object> GetState()
    {
        base.SetValue("Position", transform.position);
        base.SetValue("Rotation", transform.rotation);
        return base.GetState();
    }

    public override void SetState(Dictionary<string, object> newState)
    {
        base.SetState(newState);
        transform.position = base.GetValue<Vector3>("Position");
        transform.rotation = base.GetValue<Quaternion>("Rotation");
    }
}

using UnityEngine;
using System.Collections.Generic;

public class TestingBlock : StatefulInteractable
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created


    // Update is called once per frame

    void Start()
    {
        StateRegistry.Instance.Register(this);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == this.gameObject)
                {
                    transform.position += new Vector3(.1f, 0f, 0f);

                    //TimeHub.StateChange stateChange = new TimeHub.StateChange();
                    TimeHub.Instance.logChange(this);
                }
            }
        }
    }

    public override Dictionary<string, object> GetState()
    {
        base.SetValue("Position", transform.position);
        return base.GetState();
    }

    public override void SetState(Dictionary<string, object> newState)
    {
        base.SetState(newState);
        transform.position = base.GetValue<Vector3>("Position");
    }

    public void pickedUp()
    {
        gameObject.SetActive(false);
        TimeHub.Instance.logChange(this);
    }
}

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CubeSorter : MobileInteractable
{

    public float speed = .25f;
    public float REACH = .25f;

    private GameObject target;
    private GameObject inventory;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inventory = null;
        FindNewTarget();
        StateRegistry.Instance.Register(this);
    }

    void FixedUpdate()
    {
        if (!inventory && target)
        {
            // return;
            transform.LookAt(target.transform.position);
            transform.position += transform.forward * speed;
            if (Vector3.Distance(transform.position, target.transform.position) <= REACH)
            {
                target.GetComponent<TestingBlock>().pickedUp();
                FindNewTarget();
            }
        }

    }

    private void FindNewTarget()
    {
        List<GameObject> cubes = GameObject.FindGameObjectsWithTag("Cube").ToList();
        if (cubes.Count == 0)
        {
            target = null;
            return;
        }

        target = cubes[0];
        foreach (GameObject cube in cubes)
        {
            if (Vector3.Distance(transform.position, cube.transform.position) < Vector3.Distance(transform.position, target.transform.position))
            {
                target = cube;
            }
        }
    }

    public override Dictionary<string, object> GetState()
    {
        base.SetValue("Target", target);
        return base.GetState();
    }

    public override void SetState(Dictionary<string, object> newState)
    {
        base.SetState(newState);
        target = base.GetValue<GameObject>("Target");
    }
}

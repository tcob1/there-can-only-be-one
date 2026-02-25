using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Usable : Weapon
{
    void Update()
    {
        if (isHeld)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Attack();
            }
        }
    }

    public override void Attack()
    {
        if (!CanUse())
        {
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 1.5f))
        {
            HealthManager victimHealth = hit.collider.GetComponent<HealthManager>();
            if (victimHealth != null)
            {
                victimHealth.TakeDamage(GetDamage());
            }
        }

        lastUseTime = Time.time;
    }
}

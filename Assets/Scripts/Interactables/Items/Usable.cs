using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Usable : Weapon
{
    public override void Attack()
    {
        if (!CanUse())
        {
            return;
        }

        lastUseTime = Time.time;
    }
}

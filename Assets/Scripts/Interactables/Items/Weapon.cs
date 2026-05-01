using System;
using UnityEngine;

public abstract class Weapon : WorldItem
{
    public float damage;
    public float cooldown;
    protected float lastUseTime;
    public event Action<Weapon, GameObject> OnAttack;

    //attack requires a param user to know if player is using or guard is using
    public void Attack(GameObject user)
    {
        if (!CanUse()) return;

        lastUseTime = Time.time;

        CustomAttack();
        OnAttack?.Invoke(this, user); 
    }

    public abstract void CustomAttack();

    public float GetDamage() => damage;

    protected bool CanUse() => Time.time - lastUseTime >= cooldown;

    public float GetRemainingCooldown()
    {
        if (CanUse())
        {
            return 0f;
        }

        return cooldown - (Time.time - lastUseTime);
    }
}


using UnityEngine;

public abstract class Weapon : WorldItem
{
    public float damage;
    public float cooldown;
    protected float lastUseTime;

    public abstract void Attack();

    public float GetDamage() => damage;

    protected bool CanUse() => Time.time - lastUseTime >= cooldown;
}


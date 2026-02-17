using UnityEngine;

public abstract class Weapon : Holdable
{
    public float damage;
    public float cooldown;
    protected float lastUseTime;

    public abstract void Use();

    public float GetDamage() => damage;

    protected bool CanUse() => Time.time - lastUseTime >= cooldown;
}


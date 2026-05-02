using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CooldownBar : MonoBehaviour
{
    [SerializeField] private Image bar;

    private Weapon weapon;
    private Coroutine cooldownRoutine;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void Bind(Weapon newWeapon)
    {
        //unbind from prev weapon
        if (weapon != null)
            weapon.OnAttack -= HandleAttack;

        weapon = newWeapon;

        // bind to new weapon
        if (weapon != null)
        {
            Debug.Log("Binding cooldown bar to weapon: " + weapon.name);
            weapon.OnAttack += HandleAttack;
        }
            
    }

    private void HandleAttack(Weapon w, GameObject user)
    {
        if (user.transform.root.tag != "Player") return;

        if (cooldownRoutine != null)
        {
            StopCoroutine(cooldownRoutine);
        }

        gameObject.SetActive(true);
        cooldownRoutine = StartCoroutine(DisplayCooldown());
    }

    private IEnumerator DisplayCooldown()
    {
        float total = weapon.cooldown;

        while (weapon.GetRemainingCooldown() > 0f)
        {
            float remaining = weapon.GetRemainingCooldown();

            float percent = remaining / total;
            bar.fillAmount = percent;

            yield return null;
        }

        gameObject.SetActive(false);
        bar.fillAmount = 1f;
    }

    private void OnDestroy()
    {
        if (weapon != null)
            weapon.OnAttack -= HandleAttack;
    }

}
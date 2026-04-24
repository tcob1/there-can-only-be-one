using UnityEngine;

public class Gun : Weapon
{
    [SerializeField] private float range = 100f;
    private Transform firePoint;

    //Effects
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private ParticleSystem shootPS;

    //Recoil
    public float recoilAmount = 5f;
    public float recoilRecoverySpeed = 10f;

    private int layerMask;

    public GuardNav guard;

    protected override void Start()
    {
        base.Start();
        layerMask = ~LayerMask.GetMask("Interactable");
        firePoint = GameManager.Instance.playerAttackPosition;
    }

    void Update()
    {
        if (isHeld)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Attack();
            }
        } else if (guard != null)
        {
            if (guard.currentGuardState == GuardNav.GuardState.Shooting)
            {
                Attack();
            }
        }

    }

    private void LateUpdate()
    {
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, Time.deltaTime * recoilRecoverySpeed);
    }

    public override void Attack()
    {
        if (!CanUse())
        {
            return;
        }

        SFXManager.Instance.PlaySFX("shoot");
        shootPS.Play();

        if (Physics.Raycast(firePoint.position, firePoint.forward, out RaycastHit hit, range, layerMask))
        {

            ParticleSystem effect = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal)).GetComponent<ParticleSystem>();
            effect.GetComponent<BulletImpact>().SetTarget(hit.collider.gameObject);
            effect.Play();

            HealthManager victimHealth = hit.collider.GetComponent<HealthManager>();
            if (victimHealth != null)
            {
                victimHealth.TakeDamage(GetDamage());
            }
        }

        ApplyRecoil();

        lastUseTime = Time.time;

        // Trigger global event for player shooting (for guard reactions, etc.)
        GlobalEvents.Instance.TriggerPlayerShoot(firePoint.position);
    }

    private void ApplyRecoil()
    {
        // Random horizontal + vertical kick
        float x = Random.Range(-0.5f, 0.5f);
        float y = recoilAmount;

        // Apply gun model recoil
        transform.localRotation *= Quaternion.Euler(-y, x, 0);
    }
}

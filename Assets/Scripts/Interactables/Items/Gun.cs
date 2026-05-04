using UnityEngine;

public class Gun : Weapon
{
    [SerializeField] private float range = 100f;
    private Transform attackPosition;

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

        attackPosition = new GameObject("AttackPosition").transform;

        //for precise shoot pos
        if (transform.root.CompareTag("Player"))
        {
            Camera cam = transform.root.GetComponentInChildren<Camera>();
            attackPosition.SetParent(cam.transform);
            attackPosition.localPosition = Vector3.zero;
        }
        else
        {
            attackPosition.SetParent(transform.root);
            attackPosition.localPosition = new Vector3(0, 0f, 0.6f);
        }

    }

    void Update()
    {
        if (guard != null)
        {
            if (guard.currentGuardState == GuardNav.GuardState.Shooting)
            {
                Attack(gameObject);
            }
        }

    }

    private void LateUpdate()
    {
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, Time.deltaTime * recoilRecoverySpeed);
    }

    public override void CustomAttack()
    {

        SFXManager.Instance.PlaySFX("shoot");
        shootPS.Play();

        if (Physics.Raycast(attackPosition.position, transform.parent.forward, out RaycastHit hit, range, layerMask))
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


        // Trigger global event for player shooting (for guard reactions, etc.)
        GlobalEvents.Instance.TriggerPlayerShoot(attackPosition.position);

    }

    private void ApplyRecoil()
    {
        // Random horizontal + vertical kick
        float x = Random.Range(-0.5f, 0.5f);
        float y = recoilAmount;

        // Apply gun model recoil
        transform.localRotation *= Quaternion.Euler(-y, x, 0);
    }

    void OnDestroy()
    {
        if (attackPosition != null)
        {
            Destroy(attackPosition.gameObject);
        }
            
    }
}

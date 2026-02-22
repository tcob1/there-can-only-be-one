using UnityEngine;

public class Gun : Weapon
{
    [SerializeField] private float range = 100f;
    [SerializeField] private Transform firePoint;

    //Effects
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private ParticleSystem shootPS;

    //Recoil
    public float recoilAmount = 5f;
    public float recoilRecoverySpeed = 10f;
    private Vector3 currentRecoil;
    private Vector3 recoilVelocity;

    void Update()
    {
        if (isHeld)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("SSHSHSHSHOOOOOT");
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

        if (Physics.Raycast(firePoint.position, firePoint.forward, out RaycastHit hit, range))
        {
            Debug.Log("Hit: " + hit.collider.name);

            ParticleSystem effect = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal)).GetComponent<ParticleSystem>();
            effect.Play();
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

        currentRecoil += new Vector3(-y, x, 0);

        // Apply gun model recoil
        transform.localRotation *= Quaternion.Euler(-y, x, 0);
    }
}

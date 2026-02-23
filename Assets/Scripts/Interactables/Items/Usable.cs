using UnityEngine;

public class Usable : Weapon
{
    [SerializeField] private float range = 2f;
    [SerializeField] private float sphereRadius = 0.5f;

    [SerializeField] private float recoilAmount = 25f;
    [SerializeField] private float recoilRecoverySpeed = 10f;

    [SerializeField] private GameObject hitEffectPrefab;

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
            return;

        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            sphereRadius,
            direction,
            range
        );

        foreach (RaycastHit hit in hits)
        {
            Debug.Log("Sword hit: " + hit.collider.name);
            ParticleSystem effect = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal)).GetComponent<ParticleSystem>();
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration + effect.main.startLifetime.constantMax);
        }

        ApplyRecoil();
        lastUseTime = Time.time;
    }

    private void LateUpdate()
    {
        if (isHeld)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, Time.deltaTime * recoilRecoverySpeed);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, Time.deltaTime * recoilRecoverySpeed);
        }

    }

    private void ApplyRecoil()
    {
        // Random horizontal + vertical kick
        float x = Random.Range(-0.5f, 0.5f);
        float y = recoilAmount;

        transform.localPosition += transform.localRotation * Vector3.forward * 0.3f;
        transform.localRotation *= Quaternion.Euler(-y, x, 0);
    }
}
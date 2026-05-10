using UnityEngine;

public class Usable : Weapon
{
    [SerializeField] private float range = 2f;
    [SerializeField] private float sphereRadius = 0.5f;
    [SerializeField] private float recoilAmount = 25f;
    [SerializeField] private float recoilRecoverySpeed = 10f;
    [SerializeField] private GameObject hitEffectPrefab;
    protected LayerMask layerMask;

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;

    protected override void Start()
    {
        base.Start();
        layerMask = ~LayerMask.GetMask("InteractableDetector", "Player");

        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
    }

    public override void CustomAttack()
    {

        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            sphereRadius,
            direction,
            range,
            layerMask
        );

        foreach (RaycastHit hit in hits)
        {
            Debug.Log(gameObject.name + "hit" + hit.collider.name);
            ParticleSystem effect = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal)).GetComponent<ParticleSystem>();
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration + effect.main.startLifetime.constantMax);
            HealthManager victimHealth = hit.collider.GetComponent<HealthManager>();
            if (victimHealth != null)
            {
                victimHealth.TakeDamage(GetDamage());
            }
        }

        ApplyRecoil();
    }

    private void LateUpdate()
    {
        if (isHeld)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalLocalPosition, Time.deltaTime * recoilRecoverySpeed);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, originalLocalRotation, Time.deltaTime * recoilRecoverySpeed);
        }
    }

    private void ApplyRecoil()
    {
        transform.localPosition = originalLocalPosition;
        transform.localRotation = originalLocalRotation;

        float x = Random.Range(-0.5f, 0.5f);
        float y = recoilAmount;
        transform.localPosition += Vector3.forward * 0.3f;
        transform.localRotation *= Quaternion.Euler(-y, x, 0);
    }
}
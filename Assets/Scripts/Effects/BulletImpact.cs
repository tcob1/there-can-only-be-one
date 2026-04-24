using UnityEngine;

public class BulletImpact : MonoBehaviour
{

    [SerializeField] private float lifetime = 5f;

    private GameObject target;

    public void SetTarget(GameObject t) => target = t;

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Destroy(gameObject, lifetime);

    }
}

using UnityEngine;

public class BulletImpact : MonoBehaviour
{

    [SerializeField] private float lifetime = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}

using UnityEngine;
using System;

public class GlobalEvents : MonoBehaviour
{
    public static GlobalEvents Instance;

    public event Action<Vector3> OnPlayerShoot;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void TriggerPlayerShoot(Vector3 position)
    {
        OnPlayerShoot?.Invoke(position);
    }
}

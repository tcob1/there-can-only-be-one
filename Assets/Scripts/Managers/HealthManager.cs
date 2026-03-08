using UnityEngine;

public class HealthManager : MonoBehaviour
{
    [SerializeField] float health = 30.0f;

    public void TakeDamage(float amount)
    {
        health -= amount;
        Debug.Log(gameObject.name + " took " + amount + " damage. Health is now " + health);
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {

        if (gameObject.tag == "Player")
        {
            GameManager.Instance.EndGame();
        }
        else
        {
            Destroy(gameObject);
        }

    }
}

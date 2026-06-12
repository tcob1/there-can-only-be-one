using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public float health = 30.0f;
    public Inventory inv;

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (inv != null)
        {
            inv.DropAllPlayer();
        }

        if (gameObject.tag == "Player")
        {
            GameManager.Instance.EndGame();

        }
        else
        {
            // Don't destroy, just disable to keep timeline intact
            gameObject.SetActive(false);
        }

    }
}

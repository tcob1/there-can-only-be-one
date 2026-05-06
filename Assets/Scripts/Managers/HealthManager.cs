using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public float health = 30.0f;
    public Inventory inv;

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
        if (inv != null)
        {
            Debug.Log("Player dropped all items on death");
            inv.DropAll();
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

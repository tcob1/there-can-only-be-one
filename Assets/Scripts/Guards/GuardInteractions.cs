using UnityEngine;

public class GuardInteractions : MonoBehaviour
{
    public GameObject guard;
    public Inventory inv;

    void OnTriggerEnter(Collider collision)
    {
        Debug.Log("Guard collided with " + collision.gameObject.name);
        GameObject parent = collision.gameObject.transform.parent?.gameObject;
        Door door = parent?.GetComponent<Door>();

        if (door)
        {
            inv.GuardEquipByName("Simple Key");
        }

        door?.TryOpen(guard);
    }
}

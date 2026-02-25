using UnityEngine;

public class GuardInteractions : MonoBehaviour
{
    void OnTriggerEnter(Collider collision)
    {
        Debug.Log("Guard collided with " + collision.gameObject.name);
        GameObject parent = collision.gameObject.transform.parent?.gameObject;
        Door door = parent?.GetComponent<Door>();
        door?.TryOpen();
    }
}

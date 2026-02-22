using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public UnityEvent<GameObject> OnInteract;

    void Start()
    {
        OnInteract ??= new UnityEvent<GameObject>();
    }

    public void Interact(GameObject interactor)
    {
        OnInteract.Invoke(interactor);
    }
}

using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public UnityEvent<GameObject> OnInteract;

    void Start()
    {
        OnInteract ??= new UnityEvent<GameObject>();
        gameObject.layer = LayerMask.NameToLayer("Interactable");
    }

    public void OnHoverEnter()
    {
        gameObject.layer = LayerMask.NameToLayer("InteractableHover");
    }

    public void OnHoverExit()
    {
        gameObject.layer = LayerMask.NameToLayer("Interactable");
    }

    public void Interact(GameObject interactor)
    {
        OnInteract.Invoke(interactor);
    }
}

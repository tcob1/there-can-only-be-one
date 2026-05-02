using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public UnityEvent<GameObject> OnInteract;
    [SerializeField] private string hoverText;


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

    public virtual string GetHoverText()
    {
        if (hoverText == null)
        {
            Debug.LogWarning($"Interactable '{gameObject.name}' has no default hover text set.");
            return null;
        }
        return hoverText;
    }

    public void SetHoverText(string text)
    {
        hoverText = text;
    }
}

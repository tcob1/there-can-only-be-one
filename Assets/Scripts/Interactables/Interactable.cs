using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public UnityEvent<GameObject> OnInteract;
    [SerializeField] private string hoverText;
    [SerializeField] private List<HoverTextRule> hoverTextRules;


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


    public string GetHoverText(GameObject interactor = null, string state = null)
    {

        if (hoverTextRules != null)
        {
            Inventory inventory = interactor?.GetComponent<Inventory>();

            foreach (HoverTextRule rule in hoverTextRules)
            {
                bool stateMatch = string.IsNullOrEmpty(rule.state) || rule.state == state;
                bool itemMatch = rule.requiredItem == null ||
                                (inventory != null && inventory.HasItem(rule.requiredItem));

                if (stateMatch && itemMatch)
                {
                    return rule.hoverText;
                }
            }
        }
        return hoverText;
    }

    public void SetHoverText(string text)
    {
        hoverText = text;
    }
}

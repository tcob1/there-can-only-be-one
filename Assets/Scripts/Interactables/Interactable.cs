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
        //Debug.Log($"GetHoverText called on {gameObject.name} | state: {state} | interactor: {interactor?.name}");

        if (hoverTextRules != null)
        {
            Inventory inventory = interactor?.GetComponent<Inventory>();

            foreach (HoverTextRule rule in hoverTextRules)
            {
                bool stateMatch = string.IsNullOrEmpty(rule.state) || rule.state == state;
                bool itemMatch = rule.requiredItem == null ||
                                (inventory != null && inventory.HasItem(rule.requiredItem));

                //Debug.Log($"Rule: state={rule.state} item={rule.requiredItem?.name} text={rule.hoverText} | stateMatch={stateMatch} itemMatch={itemMatch}");

                if (stateMatch && itemMatch)
                {
                    //Debug.Log($"Matched rule: {rule.hoverText}");
                    return rule.hoverText;
                }
            }
        }

        //Debug.Log($"No rule matched, returning default: {hoverText}");
        return hoverText;
    }

    public void SetHoverText(string text)
    {
        hoverText = text;
    }
}

using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class Safe : StatefulInteractable
{
    public enum SafeState
    {
        Closed,
        Open
    }

    private static readonly string StateKey = "SafeState";

    public UnityEvent OnInitialized;
    public UnityEvent OnOpen;
    public UnityEvent OnClose;

    public SafeState State { get; private set; }

    [SerializeField] private Interactable interactable;
    [SerializeField] private SafeState initialState = SafeState.Closed;
    [SerializeField] private string keyItemName = "Simple Key";
    [SerializeField] private Animator animator;

    void Start()
    {
        OnInitialized ??= new UnityEvent();
        OnOpen ??= new UnityEvent();
        OnClose ??= new UnityEvent();

        State = initialState;
        StateRegistry.Instance.Register(this);

        interactable.OnInteract.AddListener(TryOpenWithKey);
        UpdateHoverText();
        OnInitialized.Invoke();
    }

    public override Dictionary<string, object> GetState()
    {
        base.SetValue(StateKey, State.ToString());
        return base.GetState();
    }

    public override void SetState(Dictionary<string, object> newState)
    {
        base.SetState(newState);
        if (System.Enum.TryParse(base.GetValue<string>(StateKey), out SafeState parsed))
        {
            State = parsed;
        }
    }

    public override string GetCurrentState() => State.ToString();

    public override void UpdateHoverText()
    {
        switch (State)
        {
            case SafeState.Closed:
                //interactable.SetHoverText("Open Safe");
                break;
            case SafeState.Open:
                interactable.SetHoverText("");
                break;
        }
    }

    public void TryOpen()
    {
        if (State != SafeState.Closed) return;
        State = SafeState.Open;
        UpdateHoverText();
        animator.SetTrigger("SpinOpen");
        //SFXManager.Instance.PlaySFX("SafeOpen");
        OnOpen.Invoke();
        GameManager.Instance.WinGame();
    }

    public void TryOpenWithKey(GameObject interactor)
    {
        Inventory inventory = interactor.GetComponent<Inventory>();
        if (inventory != null && inventory.HasItem(keyItemName))
        {
            TryOpen();
        }
        else
        {
            //SFXManager.Instance.PlaySFX("DoorInvalid");
        }
    }
}
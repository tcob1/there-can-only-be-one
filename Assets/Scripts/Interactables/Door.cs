using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class Door : StatefulInteractable
{
    public enum DoorState
    {
        Open,
        Closed,
        Locked
    }

    private static readonly string StateKey = "DoorState";

    public UnityEvent OnInitialized;

    public UnityEvent OnOpen;
    public UnityEvent OnClose;
    public UnityEvent OnLock;
    public UnityEvent OnUnlock;

    public DoorState State { get; private set; }

    [SerializeField] private Interactable interactable;
    [SerializeField] private DoorState initialState = DoorState.Closed;
    [SerializeField] private string keyItemName = "Key";

    void Start()
    {
        OnInitialized ??= new UnityEvent();

        OnOpen ??= new UnityEvent();
        OnClose ??= new UnityEvent();
        OnLock ??= new UnityEvent();
        OnUnlock ??= new UnityEvent();

        State = initialState;

        StateRegistry.Instance.Register(this);

        interactable.OnInteract.AddListener(TryToggleWithKey);

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
        if (System.Enum.TryParse(base.GetValue<string>(StateKey), out DoorState parsedState))
        {
            State = parsedState;
        }
    }

    public void TryOpen()
    {
        if (State != DoorState.Closed)
            return;
        State = DoorState.Open;
        OnOpen.Invoke();
    }

    public void TryClose()
    {
        if (State != DoorState.Open)
            return;
        State = DoorState.Closed;
        OnClose.Invoke();
    }

    public void TryLock()
    {
        if (State != DoorState.Closed)
            return;
        State = DoorState.Locked;
        OnLock.Invoke();
    }

    public void TryLockWithKey(GameObject interactor)
    {
        Inventory inventory = interactor.GetComponent<Inventory>();
        if (inventory != null && inventory.HasItem(keyItemName))
        {
            TryLock();
        }
    }

    public void TryUnlock()
    {
        if (State != DoorState.Locked)
            return;
        State = DoorState.Closed;
        OnUnlock.Invoke();
    }

    public void TryUnlockWithKey(GameObject interactor)
    {
        Inventory inventory = interactor.GetComponent<Inventory>();
        if (inventory != null && inventory.HasItem(keyItemName))
        {
            TryUnlock();
        }
    }

    public void TryToggle()
    {
        if (State == DoorState.Open)
        {
            TryClose();
        }
        else if (State == DoorState.Closed)
        {
            TryOpen();
        }
    }

    public void TryToggleWithKey(GameObject interactor)
    {
        if (State == DoorState.Open)
        {
            TryClose();
        }
        else if (State == DoorState.Closed)
        {
            TryOpen();
        }
        else if (State == DoorState.Locked)
        {
            TryUnlockWithKey(interactor);
        }
    }
}

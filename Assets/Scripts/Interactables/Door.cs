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

    [SerializeField] private Interactable moveInteractable;
    [SerializeField] private Interactable lockInteractable;
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

        lockInteractable.OnInteract.AddListener(TryToggleLockWithKey);
        moveInteractable.OnInteract.AddListener(TryToggleMove);

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
        if (System.Enum.TryParse(base.GetValue<string>(StateKey), out DoorState parsedState))
        {
            State = parsedState;
        }
    }

    public override void UpdateHoverText()
    {
        switch (State)
        {
            case DoorState.Open:
                moveInteractable.SetHoverText("Close(E)");
                lockInteractable.SetHoverText("Lock(Need Key)");
                break;
            case DoorState.Closed:
                moveInteractable.SetHoverText("Open(E)");
                lockInteractable.SetHoverText("Lock(Need Key)");
                break;
            case DoorState.Locked:
                moveInteractable.SetHoverText("Locked");
                lockInteractable.SetHoverText("Unlock(Need Key)");
                break;
        }
    }

    public void TryOpen()
    {
        if (State != DoorState.Closed)
        {
            Debug.Log("Cannot open door: " + State);
            SFXManager.Instance.PlaySFX("DoorInvalid");
            return;
        }
        State = DoorState.Open;
        UpdateHoverText();
        SFXManager.Instance.PlaySFX("DoorSwing");
        OnOpen.Invoke();
    }

    public void TryClose()
    {
        if (State != DoorState.Open)
        {
            return;
        }
        State = DoorState.Closed;
        UpdateHoverText();
        SFXManager.Instance.PlaySFX("DoorSwing");
        OnClose.Invoke();
    }

    public void TryLock()
    {
        if (State != DoorState.Closed)
            return;
        State = DoorState.Locked;
        UpdateHoverText();
        SFXManager.Instance.PlaySFX("DoorLock");
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
        UpdateHoverText();
        SFXManager.Instance.PlaySFX("DoorLock");
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

    public void TryToggleMove(GameObject interactor)
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
            SFXManager.Instance.PlaySFX("DoorInvalid");
        }
    }

    public void TryToggleLockWithKey(GameObject interactor)
    {
        if (State == DoorState.Closed)
        {
            TryLockWithKey(interactor);
        }
        else if (State == DoorState.Locked)
        {
            TryUnlockWithKey(interactor);
        }
    }
}

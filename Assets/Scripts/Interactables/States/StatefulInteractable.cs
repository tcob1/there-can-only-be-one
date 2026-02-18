using UnityEngine;
using System.Collections.Generic;

public class StatefulInteractable : MonoBehaviour, IStateful
{
    [SerializeField] private string uniqueID;
    protected Dictionary<string, object> state = new Dictionary<string, object>();

    public string GetUniqueID() => uniqueID;

    protected void SetValue<T>(string key, T value)
    {
        state[key] = value;
    }

    protected T GetValue<T>(string key, T defaultValue = default)
    {
        if (state.TryGetValue(key, out object value) && value is T typedValue)
        {
            return typedValue;
        }
        return defaultValue;
    }

    public virtual Dictionary<string, object> GetState()
    {
        return new Dictionary<string, object>(state);
    }

    public virtual void SetState(Dictionary<string, object> newState)
    {
        state = new Dictionary<string, object>(newState);
    }
}

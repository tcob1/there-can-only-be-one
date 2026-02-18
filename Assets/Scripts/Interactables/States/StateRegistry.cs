using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StateRegistry : MonoBehaviour
{
    public static StateRegistry Instance;

    private Dictionary<string, IStateful> interactables = new Dictionary<string, IStateful>();

    void Awake()
    {
        // TODO: Clean up during scene transitions?
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        StartCoroutine(DebugPrintStates());
    }

    public void Register(IStateful interactable)
    {
        string id = interactable.GetUniqueID();
        if (!interactables.ContainsKey(id))
        {
            interactables[id] = interactable;
        }
        else
        {
            Debug.LogWarning($"Interactable with ID {id} is already registered.");
        }
        Debug.Log("Registered interactable with ID: " + id);
    }

    public void Unregister(IStateful interactable)
    {
        string id = interactable.GetUniqueID();
        if (interactables.ContainsKey(id))
        {
            interactables.Remove(id);
        }
        else
        {
            Debug.LogWarning($"Interactable with ID {id} is not registered.");
        }
    }

    public Dictionary<string, Dictionary<string, object>> GetAllStates()
    {
        Dictionary<string, Dictionary<string, object>> allStates = new Dictionary<string, Dictionary<string, object>>();
        foreach (var kvp in interactables)
        {
            allStates[kvp.Key] = kvp.Value.GetState();
        }
        return allStates;
    }

    public void SetAllStates(Dictionary<string, Dictionary<string, object>> allStates)
    {
        foreach (var kvp in allStates)
        {
            if (interactables.TryGetValue(kvp.Key, out IStateful interactable))
            {
                interactable.SetState(kvp.Value);
            }
            else
            {
                Debug.LogWarning($"No interactable found with ID {kvp.Key} to set state.");
            }
        }
    }

    public IEnumerator DebugPrintStates()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);

            Debug.Log("=== Current Interactable States ===");
            var allStates = GetAllStates();

            foreach (var kvp in allStates)
            {
                Debug.Log($"ID: {kvp.Key}");
                foreach (var stateKvp in kvp.Value)
                {
                    Debug.Log($"  {stateKvp.Key}: {stateKvp.Value}");
                }
            }

            Debug.Log($"Total interactables: {allStates.Count}");
        }
    }
}

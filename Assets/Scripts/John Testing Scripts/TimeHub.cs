using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;
using System;

public class TimeHub : MonoBehaviour
{
    public static TimeHub Instance;
    public TextMeshProUGUI clock;

    // measured in seconds
    private long time;
    public int START_TIME = 10000;
    public int FIXED_UPDATE_RATE = 10;
    private int subsecondCounter = 0;

    public delegate void OnSecond();
    public static event OnSecond onSecond;

    public struct StateChange
    {
        public Dictionary<string, object> state;
        public long timeStamp;

        public StateChange(Dictionary<string, object> state, long timeStamp)
        {
            this.state = state;
            this.timeStamp = timeStamp;
        }
    }

    private Dictionary<string, Stack<StateChange>> timeline;
    private long goalTime = 0;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        time = START_TIME;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        Time.fixedDeltaTime = 1 / (float)FIXED_UPDATE_RATE;

        onSecond += () => updateClock(time);

        timeline = new Dictionary<string, Stack<StateChange>>();

        Dictionary<string, Dictionary<string, object>> existing = StateRegistry.Instance.GetAllStates();
        foreach (KeyValuePair<string, Dictionary<string, object>> kvp in existing)
        {
            AddNewState(kvp.Key, kvp.Value);
        }

        StateRegistry.Instance.OnRegister += AddNewObject;
    }

    void FixedUpdate()
    {
        if (goalTime != 0 && goalTime > time)
        {
            //clock.enabled = false;
            Time.fixedDeltaTime = 1 / ((float)FIXED_UPDATE_RATE * 10000);

        }
        else
        {
            Time.fixedDeltaTime = 1 / ((float)FIXED_UPDATE_RATE);
            goalTime = 0;
        }

        subsecondCounter++;
        if (subsecondCounter >= FIXED_UPDATE_RATE)
        {
            onSecond?.Invoke();
            subsecondCounter = 0;
            time += 1;
        }
    }

    private void AddNewObject(IStateful obj)
    {
        string id = obj.GetUniqueID();
        if (!timeline.ContainsKey(id))
        {
            AddNewState(id, obj.GetState());
        }
    }

    private void AddNewState(string id, Dictionary<string, object> state)
    {
        if (!timeline.ContainsKey(id))
        {
            timeline.Add(id, new Stack<StateChange>(new[] { new StateChange(state, time) }));
        }
    }

    public long getTime()
    {
        return time;
    }

    public void timeChange(int newTime)
    {
        if (newTime > 0) timeForewards(newTime);
        else if (newTime < 0) timeBackwards(-newTime);
    }

    public void timeForewards(int newTime)
    {
        //print("Traveled " + newTime + " seconds forewards");
        goalTime = time + newTime;
    }

    public void timeBackwards(int newTime)
    {
        //print("Traveled " + newTime + " seconds backwards");
        time = Math.Max(time - newTime, START_TIME);

        Dictionary<string, Dictionary<string, object>> currStates = StateRegistry.Instance.GetAllStates();

        foreach (KeyValuePair<string, Dictionary<string, object>> kvp in currStates)
        {
            string id = kvp.Key;
            Stack<StateChange> stateChanges = timeline[id];
            // ^1 = last element
            while (stateChanges.Peek().timeStamp > time)
            {
                if (stateChanges.Count == 1)
                {
                    Debug.LogWarning($"Stopping state change removal at initial state; likely traveled back past limit or this object was instantiated late.");
                }
                else if (stateChanges.Count == 0)
                {
                    Debug.LogWarning($"Timeline is Empty, how tf did you manage to do that?");
                }
                else
                {
                    stateChanges.Pop();
                }
            }

            StateRegistry.Instance.SetSingleState(id, stateChanges.Peek().state);
        }
    }

    public void logChange(IStateful obj)
    {
        string id = obj.GetUniqueID();
        // Time + 1 is important here because the state applied in the current
        // instant should be considered taken into effect in any moments after
        // the current time, not before the current time.
        // Sequential logs within one second will also properly override each
        // other, as this list will remain sorted in chronological order with
        // the most recent state change at the end of the list.
        StateChange stateChange = new StateChange(obj.GetState(), time + 1);
        if (timeline.ContainsKey(id))
        {
            timeline[id].Push(stateChange);
            //print($"Change Logged: {id} changed at time {stateChange.timeStamp}");

        }
        else Debug.LogWarning($"Interactable with ID {id} is not in the timeline.");

    }

    public void printTime(long time)
    {
        long sec, min, hour, day;

        day = time / (60 * 60 * 24);
        time -= day * 60 * 60 * 24;

        hour = time / (60 * 60);
        time -= hour * 60 * 60;

        min = time / (60);
        time -= min * 60;

        sec = time;

        print(day + ":" + hour + ":" + min + ":" + sec);
    }

    public void updateClock(long time)
    {
        long sec, min, hour, day;

        day = time / (60 * 60 * 24);
        time -= day * 60 * 60 * 24;

        hour = time / (60 * 60);
        time -= hour * 60 * 60;

        min = time / (60);
        time -= min * 60;

        sec = time;

        clock.text = "Day " + day + "\n" + hour + " : " + min + " : " + sec;
    }

    public long nextSec(long time)
    {
        time = time / FIXED_UPDATE_RATE;
        time++;
        time = time * FIXED_UPDATE_RATE;

        return time;
    }
}

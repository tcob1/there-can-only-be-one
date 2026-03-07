using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;


public class TimeHub : MonoBehaviour
{
    public static TimeHub Instance;
    public TextMeshProUGUI clock;
    private long time;
    public int START_TIME = 1000000;
    public int FIXED_UPDATE_RATE = 10;

    private Camera renderer;

    public delegate void OnSecond();              
    public static event OnSecond onSecond;

    public struct StateChange {

        public Dictionary<string, object> state;
        public long timeStamp;

        public StateChange(Dictionary<string, object> state, long timeStamp) {
            this.state = state;
            this.timeStamp = timeStamp;
        }
    }

    private Dictionary<string, List<StateChange>> timeline;
    private long goalTime = 0;


    void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("START");
        renderer = Camera.main;
        renderer.enabled = true;
        time = START_TIME;
        Time.fixedDeltaTime = 1 / (float) FIXED_UPDATE_RATE;

        Dictionary<string, Dictionary<string, object>> nullStates = StateRegistry.Instance.GetAllStates();

        timeline = new Dictionary<string, List<StateChange>>();
        //Debug.Log("Initializing timeline:");    
        foreach (KeyValuePair<string, Dictionary<string, object>> kvp in nullStates) {
            //Debug.Log(kvp.Key + " " + kvp.Value);
            timeline.Add(kvp.Key, new List<StateChange> {new StateChange(kvp.Value, 0)});
        }


    }


    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (goalTime != 0 && goalTime > time)
        {
            //renderer.enabled = false;
            Time.fixedDeltaTime = 1 / ((float) FIXED_UPDATE_RATE * 100);
        } else
        {
            Time.fixedDeltaTime = 1 / ((float) FIXED_UPDATE_RATE);
            renderer.enabled = true;
            goalTime = 0;
        }

        long prevTime = time;
        time++;
        if (((long) (time/FIXED_UPDATE_RATE) != (long) (prevTime/FIXED_UPDATE_RATE)) && onSecond != null) onSecond();
        //print(time);
        //printTime(time);
    }

    public long getTime(){
        return time;
    }

    public void timeChange(int newTime){
        if (newTime > 0) timeForewards(newTime);
        else if (newTime < 0) timeBackwards(-newTime);
    }

    public void timeForewards(int newTime){
        print("Traveled " + newTime + " seconds forewards");
        newTime*=FIXED_UPDATE_RATE;
        goalTime = (time + newTime) - (time + newTime)%FIXED_UPDATE_RATE; //Makes sure new time is multiple of a second
    }

    public void timeBackwards(int newTime){
        print("Traveled " + newTime + " seconds backwards");
        newTime*=FIXED_UPDATE_RATE;
        time = (time - newTime) - (time - newTime)%FIXED_UPDATE_RATE; //Makes sure new time is multiple of a second

        Dictionary<string, Dictionary<string, object>> currStates = StateRegistry.Instance.GetAllStates();

        foreach (KeyValuePair<string, Dictionary<string, object>> kvp in currStates) {
            string id = kvp.Key;
            List<StateChange> stateChanges = timeline[id];
            while (stateChanges[0].timeStamp > time) {
                stateChanges.RemoveAt(0);
                if (stateChanges.Count == 0) {
                    Debug.LogWarning($"Timeline is Empty, how tf did you manage to do that?");
                }
            }

            StateRegistry.Instance.SetSingleState(id, stateChanges[0].state);
        }
    }

    public void logChange(IStateful obj) {
        string id = obj.GetUniqueID();
        StateChange stateChange = new StateChange(obj.GetState(), nextSec(getTime()));
        if (timeline.ContainsKey(id)){
            timeline[id].Insert(0, stateChange);
            print($"Change Logged: {id} changed at time {stateChange.timeStamp}");

        } else Debug.LogWarning($"Interactable with ID {id} is not in the timeline.");
        
    }

    public void printTime(long time) {
        long sec, min, hour, day;
        time = (int) (time/FIXED_UPDATE_RATE);
        //print(time);

        day = time/(60*60*24);
        time -= day*60*60*24;

        hour = time/(60*60);
        time -= hour*60*60;

        min = time/(60);
        time -= min*60;

        sec = time;

        print(day + ":" + hour + ":" + min + ":" + sec);
    }

    public void updateClock(long time) {
        long sec, min, hour, day;
        time = (int) (time/FIXED_UPDATE_RATE);
        //print(time);

        day = time/(60*60*24);
        time -= day*60*60*24;

        hour = time/(60*60);
        time -= hour*60*60;

        min = time/(60);
        time -= min*60;

        sec = time;

        clock.text = "Day " + day + "\n" + hour + " : " + min + " : " + sec;
    }

    public long nextSec(long time)
    {
        time = time/FIXED_UPDATE_RATE;
        time++;
        time = time*FIXED_UPDATE_RATE;
        
        return time;
    }
}


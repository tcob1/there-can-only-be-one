using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;


public class TimeHub : MonoBehaviour
{
    public static TimeHub Instance;
    public TextMeshProUGUI clock;
    private long time;
    public int START_TIME = 1000000;
    public int FIXED_UPDATE_RATE = 10;



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
        time = START_TIME;
        Time.fixedDeltaTime = (1/ (float) FIXED_UPDATE_RATE);

    }


    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        time++;
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
        time += newTime;
    }

    public void timeBackwards(int newTime){
        print("Traveled " + newTime + " seconds backwards");
        newTime*=FIXED_UPDATE_RATE;
        time -= newTime;
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
}


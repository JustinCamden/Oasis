using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock : MonoBehaviour {
    public static Clock worldClock = null;

    [SerializeField]
    public float timeMultiplier = 60f;
    [SerializeField]
    float elapsedTimeSinceLastSecond;
    [SerializeField]
    private float mSeconds;
    [SerializeField]
    private float mMinutes;
    [SerializeField]
    private float mHours;

    public delegate void OnDayReset();
    public OnDayReset onDayReset;

    public float seconds
    {
        get { return mSeconds; }
    }

    public float minutes
    {
        get { return mMinutes; }
    }

    public float hours
    {
        get { return mHours; }
    }
    [SerializeField]
    private float mTimePercent;
    [SerializeField]
    private float secondsPerDay = 86400f;

    public float timePercent
    {
        get { return mTimePercent; }
    }

    private void Awake()
    {
        if (worldClock == null)
        {
            worldClock = this;
        }
        else if (worldClock != this)
        {
            Destroy(gameObject);
        }
    }

    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        elapsedTimeSinceLastSecond += Time.deltaTime * timeMultiplier;
        while (elapsedTimeSinceLastSecond >= 1f)
        {
            elapsedTimeSinceLastSecond -= 1f;
            mSeconds += 1.0f;
        }
        while (seconds >= 60f)
        {
            mMinutes += 1f;
            mSeconds -= 60f;
        }
        while (minutes >= 60f)
        {
            mHours += 1f;
            mMinutes -= 60f;
        }

        while (mHours >= 24f)
        {
            mHours -= 24f;
            onDayReset();
        }
        mTimePercent = CalcTimePercent(mSeconds, mMinutes, mHours);
    }

    public static float CalcTimePercent(float inSeconds, float inMinutes, float inHours)
    {
        float tempSeconds = CalcSeconds(inSeconds, inMinutes, inHours);
        return (tempSeconds / Clock.worldClock.secondsPerDay);
    }

    public static float CalcSeconds(float inSeconds, float inMinutes, float inHours)
    {
        float currSeconds = inSeconds;
        float currMinutes = inMinutes;
        currMinutes += inHours * 60f;
        currSeconds += currMinutes * 60f;
        return currSeconds;
    }
}


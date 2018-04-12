using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock : MonoBehaviour {
    [SerializeField]
    public float realSecondsPerSecond = 0.016f;
    [SerializeField]
    float startTime;
    [SerializeField]
    float elapsedTimeSinceLastSecond;
    [SerializeField]
    private float mSeconds;
    [SerializeField]
    private float mMinutes;
    [SerializeField]
    private float mHours;

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
    private float maxTime;

    public float timePercent
    {
        get { return mTimePercent; }
    }

    // Use this for initialization
    void Start () {
        startTime = Time.time;

        maxTime = realSecondsPerSecond * 60f * 60f * 24f;
	}
	
	// Update is called once per frame
	void Update () {
        elapsedTimeSinceLastSecond += Time.deltaTime;
        while (elapsedTimeSinceLastSecond >= realSecondsPerSecond)
        {
            elapsedTimeSinceLastSecond -= realSecondsPerSecond;
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
            mSeconds -= 60f;
        }

        mTimePercent = Time.time / maxTime;
	}
}


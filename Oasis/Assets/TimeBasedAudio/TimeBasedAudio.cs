using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeBasedAudio : MonoBehaviour {

    public enum PlayingState
    {
        BeforePlaying,
        Playing,
        AfterPlaying
    };

    [SerializeField]
    PlayingState playingState = PlayingState.BeforePlaying;

    [SerializeField]
    Vector3 startHoursMinutesSeconds;

    [SerializeField]
    Vector3 endHoursMinutesSeconds;

    [SerializeField]
    float startTimePercent;

    [SerializeField]
    float endTimePercent;

    [SerializeField]
    AudioSource myAudioSource;

	// Use this for initialization
	void Start () {
        endTimePercent = Clock.CalcTimePercent(endHoursMinutesSeconds.z,
            endHoursMinutesSeconds.y,
            endHoursMinutesSeconds.x);
        startTimePercent = Clock.CalcTimePercent(startHoursMinutesSeconds.z,
            startHoursMinutesSeconds.y,
            startHoursMinutesSeconds.x);
        if (!myAudioSource)
        {
            myAudioSource = GetComponent<AudioSource>();
        }

        Clock.worldClock.onDayReset += ResetAudio;
    }
	
	// Update is called once per frame
	void Update () {
		if (playingState == PlayingState.BeforePlaying &&
            Clock.worldClock.timePercent >= startTimePercent)
        {
            playingState = PlayingState.Playing;
            myAudioSource.Play();
        }
        else if (playingState == PlayingState.Playing &&
            Clock.worldClock.timePercent > endTimePercent)
        {
            playingState = PlayingState.AfterPlaying;
            myAudioSource.Stop();
            myAudioSource.time = 0f;
        }
	}

    public void ResetAudio()
    {
        playingState = PlayingState.BeforePlaying;
        myAudioSource.Stop();
        myAudioSource.time = 0f;
    }
}

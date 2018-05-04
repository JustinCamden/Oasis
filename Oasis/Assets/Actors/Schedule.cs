using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Schedule : MonoBehaviour {

    public enum EventState
    {
        BeforeEvent,
        MovingToEvent,
        ArrivedAtEvent
    };

    [SerializeField]
    private Position[] destinations;

    [SerializeField]
    private Vector3[] startHoursMinutesSeconds;

    [SerializeField]
    private EventState[] eventStates;

    [SerializeField]
    private float[] startTimes;

    [SerializeField]
    private int currEventIndex = 0;

    [SerializeField]
    private NavMeshAgent myNavAgent;

    [SerializeField]
    private float maxDistance = 5f;

    [SerializeField]
    private Vector3 startPosition;

    [SerializeField]
    private float walkingMetersPerSecond = 2f;
    [SerializeField]
    private float accelerationPerSecond = 8f;
    [SerializeField]
    private float dilatedTimeMultiplier = 0.1f;

    // Use this for initialization
    void Start () {

        // Initialize event times and statuses
		for (int i = 0; i < startHoursMinutesSeconds.Length; i++)
        {
            startTimes[i] = Clock.CalcTimePercent(startHoursMinutesSeconds[i].z,
                startHoursMinutesSeconds[i].y,
                startHoursMinutesSeconds[i].x);

            // If this event has not yet started, set the status to before the event
            if (Clock.worldClock.timePercent <= startTimes[i])
            {
                eventStates[i] = EventState.BeforeEvent;
            }

            // If this event is past its start time
            else
            {
                // If this is not the latest event
                if (i + 1 < eventStates.Length - 1)
                {
                    // Check if the start time for the next event is past
                    if (Clock.worldClock.timePercent > startTimes[i + 1])
                    {
                        // If it is then set us to arrived
                        eventStates[i] = EventState.ArrivedAtEvent;
                        currEventIndex++;
                    }                   
                    // If the next event is not passed, then begin moving here
                    else
                    {
                        transform.position = destinations[i].position;
                        eventStates[i] = EventState.BeforeEvent;
                    }
                }
                // If this is the latest event, then begin moving torwards it
                else
                {
                    transform.position = destinations[i].position;
                    eventStates[i] = EventState.BeforeEvent;
                }
            }
        }

        // Grab components
        if (!myNavAgent)
        {
            myNavAgent = GetComponent<NavMeshAgent>();
        }

        Clock.worldClock.onDayReset += ResetSchedule;
        Clock.worldClock.onTimeSpeedChange += UpdateSpeed;
        startPosition = transform.position;
        UpdateSpeed();
	}
	
	// Update is called once per frame
	void Update () {
        if (currEventIndex < startTimes.Length)
        {
            if (Clock.worldClock.timePercent >= startTimes[currEventIndex] &&
                eventStates[currEventIndex] == EventState.BeforeEvent)
            {
                MoveToEvent();
            }
            else if (eventStates[currEventIndex] == EventState.MovingToEvent &&
                Vector3.Distance(transform.position, destinations[currEventIndex].position) <= maxDistance)
            {
                ArriveAtEvent();
            }
        }
	}

    void MoveToEvent()
    {
        myNavAgent.isStopped = false;
        myNavAgent.SetDestination(destinations[currEventIndex].position);
        eventStates[currEventIndex] = EventState.MovingToEvent;
    }

    void ArriveAtEvent()
    {
        myNavAgent.isStopped = true;
        eventStates[currEventIndex] = EventState.ArrivedAtEvent;
        if (currEventIndex < destinations.Length)
        {
            currEventIndex += 1;
        }
    }

    void ResetSchedule()
    {
        for (int i = 0; i < eventStates.Length; i++)
        {
            eventStates[i] = EventState.BeforeEvent;
            currEventIndex = 0;
        }
        myNavAgent.isStopped = false;
        myNavAgent.SetDestination(startPosition);
    }

    void UpdateSpeed()
    {
        if (Clock.worldClock.IsPlayingDialogue())
        {
            myNavAgent.acceleration = Clock.worldClock.timeMultipler * accelerationPerSecond;
            myNavAgent.speed = Clock.worldClock.timeMultipler * walkingMetersPerSecond;
        }
        else
        {
            myNavAgent.acceleration = Clock.worldClock.timeMultipler * accelerationPerSecond * dilatedTimeMultiplier;
            myNavAgent.speed = Clock.worldClock.timeMultipler * walkingMetersPerSecond * dilatedTimeMultiplier;
        }
    }
}

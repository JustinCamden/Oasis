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

	// Use this for initialization
	void Start () {
		for (int i = 0; i < startHoursMinutesSeconds.Length; i++)
        {
            startTimes[i] = Clock.CalcTimePercent(startHoursMinutesSeconds[i].z,
                startHoursMinutesSeconds[i].y,
                startHoursMinutesSeconds[i].x);
            eventStates[i] = EventState.BeforeEvent;
        }

        if (!myNavAgent)
        {
            myNavAgent = GetComponent<NavMeshAgent>();
        }

        Clock.worldClock.onDayReset += ResetSchedule;
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
                Vector3.Distance(transform.position, destinations[currEventIndex].GetPosition()) <= maxDistance)
            {
                ArriveAtEvent();
            }
        }
	}

    void MoveToEvent()
    {
        myNavAgent.isStopped = false;
        myNavAgent.SetDestination(destinations[currEventIndex].GetPosition());
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
    }
}

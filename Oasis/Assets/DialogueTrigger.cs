﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour {
    public enum DialogueState
    {
        BeforeRunning,
        Running,
        AfterRunning
    };


    // Actors and corresponding lines
    public Actor[] actors;
    public AudioClip[] lines;

    // Start time
    [SerializeField]
    private float startHour = 0f;
    [SerializeField]
    private float startMinute = 0f;
    [SerializeField]
    private float startSecond = 0f;

    // End time
    [SerializeField]
    private float endHour = 23f;
    [SerializeField]
    private float endMinute = 59f;
    [SerializeField]
    private float endSecond = 59f;

    // Operational variables
    [SerializeField]
    private bool actorsInRange = false;
    [SerializeField]
    private float startTimePercent;
    [SerializeField]
    private float endTimePercent;
    [SerializeField]
    private DialogueState currDialogeState = DialogueState.BeforeRunning;

    List<Actor> overlappedActors;
    [SerializeField]
    private int runningActors = 0;

    [SerializeField]
    private bool forceActorsToLocations = false;
    [SerializeField]
    private Position[] forcedLocations;

    [SerializeField]
    private AudioSource sceneAudio;

    // Use this for initialization
    void Start () {
        startTimePercent = Clock.CalcTimePercent(startSecond, startMinute, startHour);
        endTimePercent = Clock.CalcTimePercent(endSecond, endMinute, endHour);
        if (Clock.worldClock.timePercent < endTimePercent)
        {
            currDialogeState = DialogueState.BeforeRunning;
        }
        else
        {
            currDialogeState = DialogueState.AfterRunning;
        }
        overlappedActors = new List<Actor>();
        Clock.worldClock.onDayReset += OnDayReset;
	}
	
	// Update is called once per frame
	void Update () {
        if (currDialogeState == DialogueState.BeforeRunning &&
            WithinTimeRange())
        {
            if (forceActorsToLocations)
            {
                for (int i = 0; i < actors.Length; i++)
                {
                    actors[i].transform.position = forcedLocations[i].position;
                    RunDialogue();
                }
            }
            else if (actorsInRange)
            {
                RunDialogue();
            }
        }
        if (currDialogeState == DialogueState.Running)
        {
            if (forceActorsToLocations)
            {
                for (int i = 0; i < actors.Length; i++)
                {
                    actors[i].transform.position = forcedLocations[i].position;
                }
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the actor is already in the list
        Actor overlappedActor = other.gameObject.GetComponent<Actor>();
        if (overlappedActor && !overlappedActors.Contains(overlappedActor))
        {
            // If not, check if it is one of our actors
            foreach (Actor currActor in actors)
            {
                // If it is, add it to the list
                if (overlappedActor == currActor)
                {
                    overlappedActors.Add(overlappedActor);

                    // Check if we have enough actors
                    if (overlappedActors.Count == actors.Length)
                    {
                        // If so, flag us as able to run the dialogue
                        actorsInRange = true;
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Actor exitingActor = other.gameObject.GetComponent<Actor>();
        if (exitingActor && overlappedActors.Contains(exitingActor))
        {
            overlappedActors.Remove(exitingActor);
            actorsInRange = false;
        }
            
    }

    // Check if the time is within the min / max range
    private bool WithinTimeRange()
    {
        return (Clock.worldClock.timePercent >= startTimePercent &&
            Clock.worldClock.timePercent < endTimePercent);
    }

    // Run dialogue
    private void RunDialogue()
    {
        currDialogeState = DialogueState.Running;
        for (int i = 0; i < actors.Length; i++)
        {
            actors[i].RunDialogue(lines[i], this);
            Debug.Log(i + " " + actors[i]);
        }
        runningActors = actors.Length;
        if (sceneAudio)
        {
            sceneAudio.Play();
        }
        Clock.worldClock.AddDialogue();
    }

    // Reset dialogue
    public void EndDialogue()
    {
        runningActors -= 1;
        if (runningActors <= 0)
        {
            currDialogeState = DialogueState.AfterRunning;
        }
        if (sceneAudio)
        {
            sceneAudio.Stop();
            sceneAudio.time = 0f;
        }
        Clock.worldClock.RemoveDialogue();
    }

    void OnDayReset()
    {
        currDialogeState = DialogueState.BeforeRunning;
    }
}

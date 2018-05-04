using System.Collections;
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
    private Quaternion[] forcedRotations;

    [SerializeField]
    private AudioSource sceneAudio;

    [SerializeField]
    private MeshRenderer[] notifyMeshes;

    [SerializeField]
    private TeleportHotSpot myHotSpot;

    // Use this for initialization
    void Start () {

        // Check for audiosource
        if (!sceneAudio)
        {
            sceneAudio = GetComponent<AudioSource>();
        }

        // Get the start and end time percents
        startTimePercent = Clock.CalcTimePercent(startSecond, startMinute, startHour);
        endTimePercent = Clock.CalcTimePercent(endSecond, endMinute, endHour);

        // Set current status as appropriate
        if (Clock.worldClock.timePercent < endTimePercent)
        {
            currDialogeState = DialogueState.BeforeRunning;
        }
        else
        {
            currDialogeState = DialogueState.AfterRunning;
        }

        // Create the list of actors
        overlappedActors = new List<Actor>();

        // Suscribe to the on day reset event
        Clock.worldClock.onDayReset += OnDayReset;

        // Calculate look rotations
        for (int i = 0; i < forcedLocations.Length; i++)
        {
            // Look to the center of the scene
            Vector3 lookDirection = transform.position - forcedLocations[i].position;
            lookDirection.y = 0f;
            forcedRotations[i] = Quaternion.LookRotation(lookDirection);
        }

        // Disable notifies at the start
        foreach (MeshRenderer currRenderer in notifyMeshes)
        {
            currRenderer.enabled = false;
        }

        if (myHotSpot)
        {
            myHotSpot.transform.parent = null;
        }
    }
	
	// Update is called once per frame
	void Update () {

        // If its possible to trigger the scene
        if (currDialogeState == DialogueState.BeforeRunning &&
            WithinTimeRange())
        {
            // If forcing the actors to locations, transport them immediately
            if (forceActorsToLocations)
            {
                for (int i = 0; i < actors.Length; i++)
                {
                    actors[i].transform.position = forcedLocations[i].position;
                    RunDialogue();
                }
            }
            // Otherwise, only trigger if the actors are in range
            else if (actorsInRange)
            {
                RunDialogue();
            }
        }
        if (currDialogeState == DialogueState.Running)
        {
            // If we are forcing the actors to the locations, ensure the position and rotations are accurate
            if (forceActorsToLocations)
            {
                for (int i = 0; i < actors.Length; i++)
                {
                    actors[i].transform.position = forcedLocations[i].position;
                    actors[i].transform.rotation = forcedRotations[i];
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
        // Remove actors from the list as they leave
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
        // Update current dialogue state
        currDialogeState = DialogueState.Running;

        // Have each actor run their lines
        for (int i = 0; i < actors.Length; i++)
        {
            actors[i].RunDialogue(lines[i], this);
        }

        // Update the number of running actors
        runningActors = actors.Length;

        // If scene audio is set, play it
        if (sceneAudio)
        {
            sceneAudio.Play();
        }

        // Enable notify mesh
        foreach (MeshRenderer currRenderer in notifyMeshes)
        {
            currRenderer.enabled = true;
        }

        // Register the beginning of dialogue in the world clock
        Clock.worldClock.AddDialogue();
    }

    // Reset dialogue
    public void EndDialogue()
    {
        // Decreate the number of running actors
        runningActors -= 1;
        
        // End the scene if no more actors are running
        if (runningActors <= 0)
        {
            currDialogeState = DialogueState.AfterRunning;

            // If scene audio was playing, stop it
            if (sceneAudio)
            {
                sceneAudio.Stop();
                sceneAudio.time = 0f;
            }

            // Disable notify
            foreach (MeshRenderer currRenderer in notifyMeshes)
            {
                currRenderer.enabled = false;
            }
        }

        // Decrement the number of running scenes from the world clock
        Clock.worldClock.RemoveDialogue();
    }

    void OnDayReset()
    {
        currDialogeState = DialogueState.BeforeRunning;
    }
}

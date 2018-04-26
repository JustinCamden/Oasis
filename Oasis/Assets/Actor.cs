using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour {

    [SerializeField]
    bool speaking = false;
    [SerializeField]
    AudioSource myAudioSource;
    [SerializeField]
    private DialogueTrigger lastTrigger;

	// Use this for initialization
	void Start () {
		if (!myAudioSource)
        {
            myAudioSource = GetComponent<AudioSource>();
        }
	}
	
	// Update is called once per frame
	void Update () {

        // If we're speaking and our audio source is finished
		if (speaking == true && !myAudioSource.isPlaying)
        {
            // Update state and signal the dialogue trigger
            speaking = false;
            lastTrigger.EndDialogue();
        }
	}

    public void RunDialogue(AudioClip line, DialogueTrigger trigger)
    {
        // Begin speaking and cache the trigger for later
        myAudioSource.clip = line;
        myAudioSource.Play();
        lastTrigger = trigger;
        speaking = true;

        // Look to the center of the scene
        Vector3 lookDirection = trigger.transform.position - transform.position;
        lookDirection.y = 0f;
        Quaternion newLookRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = newLookRotation;
    }
}

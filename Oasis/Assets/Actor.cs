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
		if (speaking == true && !myAudioSource.isPlaying)
        {
            speaking = false;
            lastTrigger.EndDialogue();
        }
	}

    public void RunDialogue(AudioClip line, DialogueTrigger trigger)
    {
        myAudioSource.clip = line;
        myAudioSource.Play();
        lastTrigger = trigger;
        speaking = true;
    }
}

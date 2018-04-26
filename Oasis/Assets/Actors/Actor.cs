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
    public Position snapPoint;
    [SerializeField]
    private Color highlightColor;
    [SerializeField]
    private Color selectedColor;

    bool selected;
    bool selectedQueue;

    List<Renderer> renderers;

	// Use this for initialization
	void Start () {
		if (!myAudioSource)
        {
            myAudioSource = GetComponent<AudioSource>();
        }
        renderers = new List<Renderer>(GetComponentsInChildren<Renderer>());
        Renderer myRender = GetComponent<Renderer>();
        if (myRender)
        {
            renderers.Add(myRender);
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

        // Update highlighting
        if (selectedQueue == false)
        {
            if (selected)
            {
                ActivateHighlight();
            }
            selected = false;
        }
        else
        {
            selectedQueue = false;
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

    public void ActivateHighlight()
    {
        foreach(Renderer currRenderer in renderers)
        {
            foreach (Material currMat in currRenderer.materials)
            {
                currMat.SetColor("_EmissionColor", highlightColor);
            }
        }
    }

    public void ActivateSelection()
    {
        foreach (Renderer currRenderer in renderers)
        {
            foreach (Material currMat in currRenderer.materials)
            {
                currMat.SetColor("_EmissionColor", selectedColor);
            }
        }
    }

    public void DeactivateHighlight()
    {
        foreach (Renderer currRenderer in renderers)
        {
            foreach (Material currMat in currRenderer.materials)
            {
                currMat.SetColor("_EmissionColor", Color.black);
            }
        }

        selected = false;
    }

    public void Select()
    {
        if (!selected)
        {
            ActivateSelection();
            selected = true;
        }
        selectedQueue = true;
    }
}

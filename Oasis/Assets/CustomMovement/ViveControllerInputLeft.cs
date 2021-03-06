﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveControllerInputLeft : MonoBehaviour {

    /// Origin of SteamVR tracking space
    [Tooltip("Origin of the SteamVR tracking space")]
    public Transform OriginTransform;
    /// Origin of the player's head
    [Tooltip("Transform of the player's head")]
    public Transform HeadTransform;

    // Tracked object references
    private SteamVR_TrackedObject trackedObj;
    private SteamVR_TrackedController controller;
    private SteamVR_Controller.Device device
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    // laser pointer variables
    TeleportHotSpot selectedHotspot;
    Actor selectedActor;
    private LineRenderer laserPointer;
    public float laserRange = 1000f;
    public TeleportVive teleporter;

    // List of hotspots
    List<TeleportHotSpot> hotspots;

    // List of actors
    List<Actor> actors;

    public bool FadingIn = false;
    public float TeleportTimeMarker = -1f;


    public TeleportState CurrentTeleportState = TeleportState.None;


    void Start()
    {
        // Get references to components
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        controller = GetComponent<SteamVR_TrackedController>();
        controller.PadClicked += ControllerPadClicked;
        controller.PadUnclicked += ControllerPadReleased;

        // Disable laser to start
        laserPointer = GetComponent<LineRenderer>();
        laserPointer.enabled = false;

        // Populate teleport hotspots
        hotspots = new List<TeleportHotSpot>(FindObjectsOfType<TeleportHotSpot>());
        actors = new List<Actor>(FindObjectsOfType<Actor>());
    }

    // Activate raycasting when pad clicked
    private void ControllerPadClicked(object sender, ClickedEventArgs e)
    {
        // Check if we are currently teleporting
        if (teleporter.CurrentTeleportState == TeleportState.None && CurrentTeleportState == TeleportState.None)
        {
            // If not begin teleportion selection
            foreach (TeleportHotSpot currHotspot in hotspots)
            {
                currHotspot.highlightMesh.enabled = true;
                currHotspot.highlightMesh.material = currHotspot.activeMaterial;
            }

            foreach (Actor currActor in actors)
            {
                currActor.ActivateHighlight();
            }


            CurrentTeleportState = TeleportState.Selecting;
            laserPointer.enabled = true;
        }
    }

    // Deactivate raycasting when pad unclciked
    private void ControllerPadReleased(object sender, ClickedEventArgs e)
    {
        // Check if we are selecting a teleport spot
        if (CurrentTeleportState == TeleportState.Selecting)
        {
            // If so, begin teleportation
            foreach (TeleportHotSpot currHotspot in hotspots)
            {
                currHotspot.highlightMesh.enabled = false;
            }

            foreach (Actor currActor in actors)
            {
                currActor.DeactivateHighlight();
            }

            laserPointer.enabled = false;
            if (selectedHotspot || selectedActor)
            {

                TeleportTimeMarker = Time.time;
                CurrentTeleportState = TeleportState.Teleporting;
                OriginTransform.parent = null;
            }
            else
            {
                CurrentTeleportState = TeleportState.None;
            }
        }
    }
     
    // Update is called once per frame
    void Update()
    {        
        // Draw laser if raycasting
        if (CurrentTeleportState == TeleportState.Selecting)
        {
            // Perform raycast
            laserPointer.SetPosition(0, transform.position);
            RaycastHit hit;
            Ray ray = new Ray(transform.position, transform.forward);

            if (Physics.Raycast(ray, out hit))
            {
                // Update pointer end position
                laserPointer.SetPosition(1, hit.point);

                // Check if we hit a hotspot
                selectedHotspot = hit.collider.gameObject.GetComponent<TeleportHotSpot>();
                if (selectedHotspot)
                {
                    selectedHotspot.Highlight();
                }
                else
                {
                    selectedHotspot = null;
                }

                // If there are no hotspots, check if we hit an actor
                if (!selectedHotspot)
                {
                    selectedActor = hit.collider.gameObject.GetComponent<Actor>();
                    if (selectedActor)
                    {
                        selectedActor.Select();
                    }
                    else
                    {
                        selectedActor = null;
                    }
                }
            }

            else
            {
                // Update pointer end position
                laserPointer.SetPosition(1, transform.position + transform.forward * laserRange);
                selectedHotspot = null;
            }
        }
        else if (CurrentTeleportState == TeleportState.Teleporting)
        {
            if (Time.time - TeleportTimeMarker >= teleporter.TeleportFadeDuration / 2)
            {
                if (FadingIn)
                {
                    // We have finished fading in
                    CurrentTeleportState = TeleportState.None;
                    selectedHotspot = null;
                }
                else
                {
                    // We have finished fading out - time to teleport!
                    Vector3 offset = OriginTransform.position - HeadTransform.position;
                    offset.y = 0;
                    if (selectedHotspot)
                    {
                        OriginTransform.position = selectedHotspot.HotSpotPosition.position + offset;
                    }
                    else if (selectedActor)
                    {
                        OriginTransform.position = selectedActor.snapPoint.position;
                        OriginTransform.SetParent(selectedActor.snapPoint.transform, true);
                    }     
                }

                TeleportTimeMarker = Time.time;
                FadingIn = !FadingIn;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveControllerInputLeft : MonoBehaviour {

    // Tracked object references
    private SteamVR_TrackedObject trackedObj;
    private SteamVR_TrackedController controller;
    private SteamVR_Controller.Device device
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    // laser pointer variables
    bool raycasting;
    TeleportHotSpot hotspot;
    private LineRenderer laserPointer;
    public float laserRange = 1000f;
    


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
    }

    // Activate raycasting when pad clicked
    private void ControllerPadClicked(object sender, ClickedEventArgs e)
    {
        raycasting = true;
        laserPointer.enabled = true;
    }

    // Deactivate raycasting when pad unclciked
    private void ControllerPadReleased(object sender, ClickedEventArgs e)
    {
        raycasting = false;
        laserPointer.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

        // Draw laser if raycasting
        if (raycasting)
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
                hotspot = hit.collider.gameObject.GetComponent<TeleportHotSpot>();
                if (hotspot)
                {
                    Debug.Log("Raycast hit hotspot");
                }
            }

            else
            {
                // Update pointer end position
                laserPointer.SetPosition(1, transform.position + transform.forward * laserRange);
            }
        }
    }
}

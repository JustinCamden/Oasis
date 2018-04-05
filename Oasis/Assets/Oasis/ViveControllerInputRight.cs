using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveControllerInputRight : MonoBehaviour {

    // Tracked object ref
    private SteamVR_TrackedObject trackedObj;
    private SteamVR_TrackedController controller;
    private SteamVR_Controller.Device device
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }


    void Start()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        controller = GetComponent<SteamVR_TrackedController>();
        controller.PadClicked += ControllerPadClicked;
    }

   private void ControllerPadClicked(object sender, ClickedEventArgs e)
    {
        Debug.Log("Clicked");
    }

    // Update is called once per frame
    void Update () {
	}
}

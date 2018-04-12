using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveControllerInputLeft : MonoBehaviour {

    /// Origin of SteamVR tracking space
    [Tooltip("Origin of the SteamVR tracking space")]
    public Transform OriginTransform;
    /// Origin of the player's head
    [Tooltip("Transform of the player's head")]
    public Transform HeadTransform;

    public Transform eyes;

    /// How long, in seconds, the fade-in/fade-out animation should take
    [Tooltip("Duration of the \"blink\" animation (fading in and out upon teleport) in seconds.")]
    public float TeleportFadeDuration = 0.2f;

    /// Material used to render the fade in/fade out quad
    [Tooltip("Material used to render the fade in/fade out quad.")]
    [SerializeField]
    private Material FadeMaterial;
    private Material FadeMaterialInstance;
    private int MaterialFadeID;

    // Tracked object references
    private SteamVR_TrackedObject trackedObj;
    private SteamVR_TrackedController controller;
    private SteamVR_Controller.Device device
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    // laser pointer variables
    public bool raycasting;
    TeleportHotSpot hotspot;
    private LineRenderer laserPointer;
    public float laserRange = 1000f;
    public TeleportVive teleporter;

    // List of hotspots
    List<TeleportHotSpot> hotspots;

    private bool FadingIn = false;
    private float TeleportTimeMarker = -1f;

    private Mesh PlaneMesh;

    public TeleportState CurrentTeleportState = TeleportState.None;


    void Start()
    {
        // Get references to components
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        controller = GetComponent<SteamVR_TrackedController>();
        controller.PadClicked += ControllerPadClicked;
        controller.PadUnclicked += ControllerPadReleased;

        // Standard plane mesh used for "fade out" graphic when you teleport
        // This way you don't need to supply a simple plane mesh in the inspector
        PlaneMesh = new Mesh();
        Vector3[] verts = new Vector3[]
        {
            new Vector3(-1, -1, 0),
            new Vector3(-1, 1, 0),
            new Vector3(1, 1, 0),
            new Vector3(1, -1, 0)
        };
        int[] elts = new int[] { 0, 1, 2, 0, 2, 3 };
        PlaneMesh.vertices = verts;
        PlaneMesh.triangles = elts;
        PlaneMesh.RecalculateBounds();

        // Disable laser to start
        laserPointer = GetComponent<LineRenderer>();
        laserPointer.enabled = false;

        // Populate teleport hotspots
        hotspots = new List<TeleportHotSpot>(FindObjectsOfType<TeleportHotSpot>());

        // Initialize fade mat variables
        if (FadeMaterial != null)
        {
            FadeMaterialInstance = new Material(FadeMaterial);
        }
        MaterialFadeID = Shader.PropertyToID("_Fade");
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

            laserPointer.enabled = false;
            if (hotspot)
            {

                TeleportTimeMarker = Time.time;
                CurrentTeleportState = TeleportState.Teleporting;
                Debug.Log("Teleport time " + TeleportTimeMarker);
            }
            else
            {
                CurrentTeleportState = TeleportState.None;
            }
        }
    }

    void OnPostRender()
    {
        if (CurrentTeleportState == TeleportState.Teleporting)
        {
            // Perform the fading in/fading out animation, if we are teleporting.  This is essentially a triangle wave
            // in/out, and the user teleports when it is fully black.
            float alpha = Mathf.Clamp01((Time.time - TeleportTimeMarker) / (TeleportFadeDuration / 2));
            if (FadingIn)
            {
                alpha = 1 - alpha;
            }

            Matrix4x4 local = Matrix4x4.TRS(Vector3.forward * 0.3f, Quaternion.identity, Vector3.one);
            FadeMaterialInstance.SetPass(0);
            FadeMaterialInstance.SetFloat(MaterialFadeID, alpha);
            Graphics.DrawMeshNow(PlaneMesh, teleporter.gameObject.transform.localToWorldMatrix * local);
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
                hotspot = hit.collider.gameObject.GetComponent<TeleportHotSpot>();
                if (hotspot)
                {
                    hotspot.Highlight();
                }
                else
                {
                    hotspot = null;
                }
            }

            else
            {
                // Update pointer end position
                laserPointer.SetPosition(1, transform.position + transform.forward * laserRange);
                hotspot = null;
            }
        }
        else if (CurrentTeleportState == TeleportState.Teleporting)
        {
            if (Time.time - TeleportTimeMarker >= TeleportFadeDuration / 2)
            {
                Debug.Log("Transition" + Time.time);
                if (FadingIn)
                {
                    // We have finished fading in
                    CurrentTeleportState = TeleportState.None;
                    hotspot = null;
                }
                else
                {
                    // We have finished fading out - time to teleport!
                    Vector3 offset = OriginTransform.position - HeadTransform.position;
                    offset.y = 0;
                    OriginTransform.position = hotspot.HotSpotPosition.transform.position + offset;
                }

                TeleportTimeMarker = Time.time;
                FadingIn = !FadingIn;
            }
        }
    }
}

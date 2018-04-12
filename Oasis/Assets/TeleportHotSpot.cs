using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportHotSpot : MonoBehaviour {

    public Transform HotSpotPosition;
    public bool autoCalculateHotSpotPosition;
    public float height = 1.5f;
    bool highlight;
    bool highlightQue;
    public MeshRenderer highlightMesh;
    public Material activeMaterial;
    public Material highlightedMaterial;

    void Start()
    {
        highlightMesh.enabled = false;

        // Calculate hotspot positon if checked
       if (autoCalculateHotSpotPosition)
        {
            CalculateHotSpotPosition();
        }
    }

    void Update()
    {
        // Handle deactivating the highlight mesh
        if (highlightQue == false)
        {
            if (highlight)
            {
                highlightMesh.material = activeMaterial;
            }
            highlight = false;
        }
        else
        {
            highlightQue = false;
        }
    }


    public void Highlight()
    {
        // Handle activating the highlight mesh
        if (!highlight)
        {
            highlightMesh.material = highlightedMaterial;
        }
        highlight = true;
        highlightQue = true;
    }

    void CalculateHotSpotPosition()
    {
        // Find the new hotspot position throught raycasting
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.up * -1.0f);
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 hitPosition = hit.point;
            hitPosition.y += height;
            HotSpotPosition.transform.position = hitPosition;
        }
    }
}

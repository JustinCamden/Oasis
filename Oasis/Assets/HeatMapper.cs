using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatMapper : MonoBehaviour {

    [SerializeField]
    private GameObject heatMap;
    [SerializeField]
    private Color heatMapColor = Color.red;
    [SerializeField]
    private Material heatmapMaterial;

    [SerializeField]
    private Color[] zoneColors;

    List<GameObject> heatMaps;

    [SerializeField]
    private float spaceBetweenMaps = 2f;
    Vector3 lastPosition;

	// Use this for initialization
	void Start () {
        heatMaps = new List<GameObject>();
        heatmapMaterial = GetComponent<MeshRenderer>().material;
        heatmapMaterial.SetColor("_EmissionColor", heatMapColor);
        lastPosition = transform.position;
        Clock.worldClock.onDayReset += OnDayReset;
	}
	
	// Update is called once per frame
	void Update () {

        Vector3 directionVector = transform.position - lastPosition;
        float distance = directionVector.magnitude;
        if (distance > spaceBetweenMaps)
        {
            directionVector = Vector3.Normalize(directionVector);
            while (distance > spaceBetweenMaps)
            {
                Vector3 spawnPosition = lastPosition;
                spawnPosition += directionVector * distance;
                SpawnHeatMap(spawnPosition);
                distance -= spaceBetweenMaps;
            }
            lastPosition = transform.position;
        }
		
	}

    void SpawnHeatMap(Vector3 spawnPosition)
    {
        GameObject newMap = Instantiate(heatMap, spawnPosition, Quaternion.identity);
        newMap.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", heatMapColor);
        heatMaps.Add(newMap);
    }

    void OnDayReset()
    {
        foreach (GameObject currMap in heatMaps)
        {
            Destroy(currMap, 0.0001f);
        }
        heatMaps.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        HeatmapZone heatZone = other.gameObject.GetComponent<HeatmapZone>();
        if (heatZone && heatZone.zoneNumber < zoneColors.Length - 1)
        {
            heatMapColor = zoneColors[heatZone.zoneNumber];
            heatmapMaterial.SetColor("_EmissionColor", heatMapColor);
        }
    }
}

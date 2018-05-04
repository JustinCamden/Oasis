using UnityEngine;
using System.Collections;

public class AutoIntensity : MonoBehaviour
{

    public Gradient nightDayColor;

    public float maxIntensity = 3f;
    public float minIntensity = 0f;
    public float minPoint = -0.2f;

    public float maxAmbient = 1f;
    public float minAmbient = 0f;
    public float minAmbientPoint = -0.2f;


    public Gradient nightDayFogColor;
    public AnimationCurve fogDensityCurve;
    public float fogScale = 1f;

    public float dayAtmosphereThickness = 0.4f;
    public float nightAtmosphereThickness = 0.87f;

    public Vector3 dayRotateSpeed;
    public Vector3 nightRotateSpeed;

    public float startDeactivationDot = 0.2f;

    float skySpeed = 1;
    float currRotDot = 0;

    Light mainLight;
    Skybox sky;
    Material skyMat;

    public GameObject stars;

    [SerializeField]
    private float dot;

    void Start()
    {

        mainLight = GetComponent<Light>();
        skyMat = RenderSettings.skybox;
        stars.SetActive(false);

    }

    void Update()
    {

        float tRange = 1 - minPoint;
        dot = Mathf.Clamp01((Vector3.Dot(mainLight.transform.forward, Vector3.down) - minPoint) / tRange);
        float i = ((maxIntensity - minIntensity) * dot) + minIntensity;

        if (dot < startDeactivationDot && !stars.activeSelf)
        {
            stars.SetActive(true);
        }
        else if (dot >= startDeactivationDot && stars.activeSelf)
        {
            stars.SetActive(false);
        }

        mainLight.intensity = i;

        tRange = 1 - minAmbientPoint;
        dot = Mathf.Clamp01((Vector3.Dot(mainLight.transform.forward, Vector3.down) - minAmbientPoint) / tRange);
        i = ((maxAmbient - minAmbient) * dot) + minAmbient;
        RenderSettings.ambientIntensity = i;

        mainLight.color = nightDayColor.Evaluate(dot);
        RenderSettings.ambientLight = mainLight.color;

        RenderSettings.fogColor = nightDayFogColor.Evaluate(dot);
        RenderSettings.fogDensity = fogDensityCurve.Evaluate(dot) * fogScale;

        i = ((dayAtmosphereThickness - nightAtmosphereThickness) * dot) + nightAtmosphereThickness;
        skyMat.SetFloat("_AtmosphereThickness", i);

        currRotDot = Clock.worldClock.timePercent * 360f;
        if (currRotDot <= 90f)
        {
            currRotDot += 270f;
        }
        else
        {
            currRotDot -= 90f;
        }
        Vector3 newRotation = new Vector3();
        newRotation.x = currRotDot;
        newRotation.y = 270f;
        transform.eulerAngles = newRotation;


    }
}
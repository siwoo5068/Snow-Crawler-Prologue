using UnityEngine;

public class FogManager : MonoBehaviour
{
    [Header("References")]
    public SurvivalTimer survivalTimer;

    [Header("Outside Fog")]
    public Color fogColor = new Color(0.75f, 0.8f, 0.9f);
    public float outsideFogDensity = 0.08f;

    [Header("Safe Zone (No Fog)")]
    public float safeZoneFogDensity = 0.002f;

    [Header("Transition")]
    public float transitionSpeed = 1.5f;

    private float targetDensity;

    void Start()
    {
        if (survivalTimer == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
                survivalTimer = player.GetComponent<SurvivalTimer>();
        }

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = safeZoneFogDensity;
    }

    void Update()
    {
        if (survivalTimer == null) return;

        targetDensity = survivalTimer.inSafeZone ? safeZoneFogDensity : outsideFogDensity;
        RenderSettings.fogDensity = Mathf.MoveTowards(
            RenderSettings.fogDensity, targetDensity, transitionSpeed * Time.deltaTime);
    }
}

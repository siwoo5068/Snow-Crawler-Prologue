using UnityEngine;
using UnityEngine.UI;

public class AtmosphereManager : MonoBehaviour
{
    [Header("References")]
    public SurvivalTimer survivalTimer;
    public Image frostOverlay;
    public Light environmentLight;
    public CabinComfort cabinComfort;

    [Header("Frost Settings")]
    public float maxFrostAlpha = 0.7f;
    public float frostCurveExponent = 2.5f;
    public float frostFadeSpeed = 2f;
    public float safeZoneMeltSpeed = 3f;

    [Header("Cold Light (Outside)")]
    public Color coldLightColor = new Color(0.55f, 0.65f, 0.85f);
    public float coldIntensity = 0.35f;

    [Header("Warm Light (Safe Zone)")]
    public Color warmLightColor = new Color(1f, 0.82f, 0.45f);
    public float warmIntensity = 1.3f;

    [Header("Light Transition")]
    public float lightTransitionSpeed = 1.8f;

    private float currentFrostAlpha;
    private Color initialLightColor;
    private float initialLightIntensity;

    void Start()
    {
        if (survivalTimer == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
                survivalTimer = player.GetComponent<SurvivalTimer>();
        }

        if (frostOverlay != null)
        {
            frostOverlay.raycastTarget = false;
            SetFrostAlpha(0f);
        }

        if (environmentLight != null)
        {
            initialLightColor = environmentLight.color;
            initialLightIntensity = environmentLight.intensity;
        }

        // ── 겨울 환경 초기화 ──────────────────────────────────────
        InitWinterEnvironment();
    }

    /// <summary>씬 시작 시 겨울 분위기를 확실하게 세팅</summary>
    void InitWinterEnvironment()
    {
        // 흐린 겨울 하늘 (Procedural Skybox)
        var skyShader = Shader.Find("Skybox/Procedural");
        if (skyShader != null)
        {
            var skyMat = new Material(skyShader);
            skyMat.SetFloat("_SunSize", 0.02f);           // 작은 태양 (흐린 날)
            skyMat.SetFloat("_SunSizeConvergence", 1f);
            skyMat.SetFloat("_AtmosphereThickness", 2.5f); // 두꺼운 대기
            skyMat.SetFloat("_Exposure", 0.8f);             // 어둡고 흐린 하늘
            skyMat.SetColor("_SkyTint", new Color(0.55f, 0.60f, 0.70f));       // 회색빛 하늘
            skyMat.SetColor("_GroundColor", new Color(0.85f, 0.88f, 0.92f));   // 눈빛 반사
            RenderSettings.skybox = skyMat;
        }

        // 차가운 앰비언트 라이트
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.45f, 0.50f, 0.60f);

        // Directional Light 겨울 설정
        if (environmentLight != null)
        {
            environmentLight.color = coldLightColor;
            environmentLight.intensity = coldIntensity;
            environmentLight.shadowStrength = 0.4f;
        }
    }

    void Update()
    {
        if (survivalTimer == null) return;

        UpdateFrostOverlay();
        UpdateLighting();
    }

    void UpdateFrostOverlay()
    {
        if (frostOverlay == null) return;

        float targetAlpha;

        if (survivalTimer.inSafeZone)
        {
            targetAlpha = 0f;
            currentFrostAlpha = Mathf.Lerp(currentFrostAlpha, targetAlpha, Time.deltaTime * safeZoneMeltSpeed);
        }
        else
        {
            float danger = 1f - survivalTimer.TimeRatio;
            float curved = Mathf.Pow(danger, frostCurveExponent);
            targetAlpha = curved * maxFrostAlpha;
            currentFrostAlpha = Mathf.Lerp(currentFrostAlpha, targetAlpha, Time.deltaTime * frostFadeSpeed);
        }

        SetFrostAlpha(currentFrostAlpha);
    }

    void SetFrostAlpha(float alpha)
    {
        Color c = frostOverlay.color;
        c.a = alpha;
        frostOverlay.color = c;
    }

    void UpdateLighting()
    {
        if (environmentLight == null) return;

        Color targetColor;
        float targetIntensity;

        if (survivalTimer.inSafeZone)
        {
            float comfort = (cabinComfort != null) ? cabinComfort.ComfortRatio : 0f;
            float comfortBoost = Mathf.Lerp(0.5f, 1f, comfort);
            targetColor = warmLightColor;
            targetIntensity = warmIntensity * comfortBoost;
        }
        else
        {
            targetColor = coldLightColor;
            targetIntensity = coldIntensity;
        }

        float t = Time.deltaTime * lightTransitionSpeed;
        environmentLight.color = Color.Lerp(environmentLight.color, targetColor, t);
        environmentLight.intensity = Mathf.Lerp(environmentLight.intensity, targetIntensity, t);
    }
}

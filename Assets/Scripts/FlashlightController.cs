using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("Flashlight")]
    public Light flashlight;
    public AudioSource clickSound;

    [Header("Settings")]
    public bool startsOn = true;
    public KeyCode toggleKey = KeyCode.F;

    [Header("Light Power")]
    [Tooltip("손전등 밝기 (높을수록 밝음)")]
    public float flashlightIntensity = 3.0f;
    [Tooltip("손전등 도달 거리")]
    public float flashlightRange = 30f;
    [Tooltip("빔 각도 (좁을수록 집중)")]
    public float flashlightSpotAngle = 55f;

    [Header("Battery")]
    public float maxBattery = 100f;
    public float currentBattery;
    public float drainRate = 4.5f;
    public float flickerThreshold = 30f;

    private float _originalIntensity;
    private bool _hasShownWarning;

    void Start()
    {
        if (flashlight == null) flashlight = GetComponentInChildren<Light>();
        if (flashlight != null)
        {
            // 코드에서 손전등 성능 강제 설정 (씬 Inspector 값 덮어씀)
            flashlightIntensity = 3.0f;
            flashlightRange = 30f;
            flashlightSpotAngle = 55f;

            flashlight.type = LightType.Spot;
            flashlight.intensity = flashlightIntensity;
            flashlight.range = flashlightRange;
            flashlight.spotAngle = flashlightSpotAngle;
            flashlight.innerSpotAngle = flashlightSpotAngle * 0.6f;
            flashlight.color = new Color(1f, 0.95f, 0.85f); // 따뜻한 백색
            flashlight.shadows = LightShadows.Soft;

            _originalIntensity = flashlightIntensity;
            flashlight.enabled = startsOn;
        }
        currentBattery = maxBattery;
    }

    void Update()
    {
        if (flashlight == null) return;

        // 배터리 소모
        if (flashlight.enabled)
        {
            currentBattery -= drainRate * Time.deltaTime;
            if (currentBattery <= 0f)
            {
                currentBattery = 0f;
                flashlight.enabled = false;
                if (SubtitleManager.Instance != null)
                    SubtitleManager.Instance.ShowSubtitle("손전등이 완전히 꺼졌다... 앞이 보이지 않아...", 3f, true);
            }
        }

        // 깜빡임
        if (flashlight.enabled)
        {
            if (currentBattery <= flickerThreshold && currentBattery > 0)
            {
                flashlight.intensity = _originalIntensity * Random.Range(0.2f, 1.0f);
                if (!_hasShownWarning)
                {
                    _hasShownWarning = true;
                    if (SubtitleManager.Instance != null)
                        SubtitleManager.Instance.ShowSubtitle("손전등 배터리가 얼마 남지 않았어...", 3f, true);
                }
            }
            else
            {
                flashlight.intensity = _originalIntensity;
            }
        }

        // 토글
        if (Input.GetKeyDown(toggleKey))
        {
            if (currentBattery > 0f)
            {
                flashlight.enabled = !flashlight.enabled;
                if (clickSound != null) clickSound.Play();
            }
            else if (SubtitleManager.Instance != null)
            {
                SubtitleManager.Instance.ShowSubtitle("배터리가 없어서 켤 수 없다.", 2f);
            }
        }
    }

    public void RechargeBattery()
    {
        currentBattery = maxBattery;
        _hasShownWarning = false;
        if (flashlight != null) flashlight.intensity = _originalIntensity;
    }
}

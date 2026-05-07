using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("Flashlight")]
    public Light flashlight;
    public AudioSource clickSound;

    [Header("Settings")]
    public bool startsOn = true;
    public KeyCode toggleKey = KeyCode.F;

    [Header("Battery System")]
    public float maxBattery = 100f;
    public float currentBattery;
    public float drainRate = 1.5f; // 초당 닳는 배터리 양
    public float flickerThreshold = 20f; // 배터리가 이 수치 이하일 때 깜빡임

    private float originalIntensity;
    private bool hasShownWarning = false;

    void Start()
    {
        if (flashlight == null) flashlight = GetComponentInChildren<Light>();
        
        if (flashlight != null)
        {
            originalIntensity = flashlight.intensity;
            flashlight.enabled = startsOn;
        }

        currentBattery = maxBattery;
    }

    void Update()
    {
        // 1. 배터리 소모 로직
        if (flashlight != null && flashlight.enabled)
        {
            currentBattery -= drainRate * Time.deltaTime;

            if (currentBattery <= 0f)
            {
                currentBattery = 0f;
                flashlight.enabled = false; // 배터리 방전 시 강제 종료
                if (SubtitleManager.Instance != null)
                    SubtitleManager.Instance.ShowSubtitle("손전등이 완전히 꺼졌다... 앞이 보이지 않아...", 3f, true);
            }
        }

        // 2. 깜빡임 (Flicker) 연출 및 경고
        if (flashlight != null && flashlight.enabled)
        {
            if (currentBattery <= flickerThreshold && currentBattery > 0)
            {
                // 랜덤하게 밝기가 줄어들며 깜빡거림 (공포 연출)
                float flicker = Random.Range(0.2f, 1.0f);
                flashlight.intensity = originalIntensity * flicker;

                if (!hasShownWarning)
                {
                    hasShownWarning = true;
                    if (SubtitleManager.Instance != null)
                        SubtitleManager.Instance.ShowSubtitle("손전등 배터리가 얼마 남지 않았어...", 3f, true);
                }
            }
            else
            {
                // 배터리가 충분하면 원래 밝기 유지
                flashlight.intensity = originalIntensity;
            }
        }

        // 3. F키 조작 로직
        if (Input.GetKeyDown(toggleKey))
        {
            if (currentBattery > 0f)
            {
                if (flashlight != null) flashlight.enabled = !flashlight.enabled;
                if (clickSound != null) clickSound.Play();
            }
            else
            {
                if (SubtitleManager.Instance != null)
                    SubtitleManager.Instance.ShowSubtitle("배터리가 없어서 켤 수 없다.", 2f);
            }
        }
    }

    // 별장으로 귀환 시 호출하여 배터리 충전
    public void RechargeBattery()
    {
        currentBattery = maxBattery;
        hasShownWarning = false;
        if (flashlight != null) flashlight.intensity = originalIntensity;
    }
}

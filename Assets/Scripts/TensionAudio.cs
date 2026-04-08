using UnityEngine;

public class TensionAudio : MonoBehaviour
{
    [Header("References")]
    public SurvivalTimer survivalTimer;
    public PlayerInventory inventory;

    [Header("Heartbeat")]
    public AudioSource heartbeatSource;
    public float heartbeatFadeInRatio = 0.7f;
    public float minPitch = 0.6f;
    public float maxPitch = 1.5f;
    public float maxHeartbeatVolume = 0.8f;

    [Header("Breathing")]
    public AudioSource breathingSource;
    public float breathingWeightThreshold = 5f;
    public float maxBreathingVolume = 0.5f;

    [Header("Safe Zone Fade")]
    public float fadeOutSpeed = 3f;

    void Start()
    {
        if (survivalTimer == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
                survivalTimer = player.GetComponent<SurvivalTimer>();
        }
        if (inventory == null)
            inventory = GetComponent<PlayerInventory>();

        if (heartbeatSource != null)
        {
            heartbeatSource.loop = true;
            heartbeatSource.volume = 0f;
            heartbeatSource.Play();
        }
        if (breathingSource != null)
        {
            breathingSource.loop = true;
            breathingSource.volume = 0f;
            breathingSource.Play();
        }
    }

    void Update()
    {
        if (survivalTimer == null) return;

        if (survivalTimer.inSafeZone)
        {
            FadeOut();
            return;
        }

        UpdateHeartbeat();
        UpdateBreathing();
    }

    void UpdateHeartbeat()
    {
        if (heartbeatSource == null) return;

        float timeRatio = survivalTimer.TimeRatio;

        if (timeRatio < heartbeatFadeInRatio)
        {
            float danger = 1f - (timeRatio / heartbeatFadeInRatio);
            float targetVolume = danger * maxHeartbeatVolume;
            float targetPitch = Mathf.Lerp(minPitch, maxPitch, danger);

            heartbeatSource.volume = Mathf.Lerp(heartbeatSource.volume, targetVolume, Time.deltaTime * 3f);
            heartbeatSource.pitch = Mathf.Lerp(heartbeatSource.pitch, targetPitch, Time.deltaTime * 3f);
        }
        else
        {
            heartbeatSource.volume = Mathf.Lerp(heartbeatSource.volume, 0f, Time.deltaTime * 2f);
        }
    }

    void UpdateBreathing()
    {
        if (breathingSource == null || inventory == null) return;

        float weight = inventory.TotalWeight;

        if (weight > breathingWeightThreshold)
        {
            float overWeight = weight - breathingWeightThreshold;
            float ratio = Mathf.Clamp01(overWeight / 10f);
            float targetVolume = ratio * maxBreathingVolume;

            breathingSource.volume = Mathf.Lerp(breathingSource.volume, targetVolume, Time.deltaTime * 2f);
        }
        else
        {
            breathingSource.volume = Mathf.Lerp(breathingSource.volume, 0f, Time.deltaTime * 2f);
        }
    }

    void FadeOut()
    {
        if (heartbeatSource != null)
            heartbeatSource.volume = Mathf.Lerp(heartbeatSource.volume, 0f, Time.deltaTime * fadeOutSpeed);
        if (breathingSource != null)
            breathingSource.volume = Mathf.Lerp(breathingSource.volume, 0f, Time.deltaTime * fadeOutSpeed);
    }
}

using UnityEngine;

public class ZoneAudio : MonoBehaviour
{
    [Header("References")]
    public SurvivalTimer survivalTimer;

    [Header("Audio Sources")]
    public AudioSource indoorBGM;
    public AudioSource blizzardAmbient;

    [Header("Fade Settings")]
    public float fadeSpeed = 1.5f;
    public float indoorMaxVolume = 0.4f;
    public float blizzardMaxVolume = 0.7f;

    void Start()
    {
        if (survivalTimer == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
                survivalTimer = player.GetComponent<SurvivalTimer>();
        }

        if (indoorBGM != null)
        {
            indoorBGM.loop = true;
            indoorBGM.playOnAwake = false;
            indoorBGM.volume = indoorMaxVolume;
            if (indoorBGM.clip != null) indoorBGM.Play();
        }

        if (blizzardAmbient != null)
        {
            blizzardAmbient.loop = true;
            blizzardAmbient.playOnAwake = false;
            blizzardAmbient.volume = 0f;
            if (blizzardAmbient.clip != null) blizzardAmbient.Play();
        }
    }

    void Update()
    {
        if (survivalTimer == null) return;

        if (survivalTimer.inSafeZone)
        {
            FadeTo(indoorBGM, indoorMaxVolume);
            FadeTo(blizzardAmbient, 0f);
        }
        else
        {
            FadeTo(indoorBGM, 0f);
            FadeTo(blizzardAmbient, blizzardMaxVolume);
        }
    }

    void FadeTo(AudioSource source, float target)
    {
        if (source == null) return;
        source.volume = Mathf.MoveTowards(source.volume, target, fadeSpeed * Time.deltaTime);
    }
}

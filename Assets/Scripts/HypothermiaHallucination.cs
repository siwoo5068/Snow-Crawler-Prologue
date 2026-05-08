using UnityEngine;

public class HypothermiaHallucination : MonoBehaviour
{
    [Header("References")]
    public SurvivalTimer survivalTimer;
    private Camera _playerCamera;

    [Header("Settings")]
    public float hallucinationThreshold = 15f;

    [Header("FOV Pulse")]
    public float fovPulseSpeed = 5f;
    public float fovPulseAmount = 3f;
    private float _originalFov;

    [Header("Fake Audio")]
    public AudioSource hallucinationAudioSource;
    public AudioClip[] fakeFootsteps;
    public float minAudioInterval = 2f;
    public float maxAudioInterval = 5f;
    private float _audioTimer;

    [Header("Subtitles")]
    public float subtitleInterval = 6f;
    private float _subtitleTimer;

    private readonly string[] _creepySubtitles = {
        "누군가 내 뒤에 있는 것 같아...",
        "갑자기... 몸이 왜 이렇게 따뜻하지?",
        "저기 별장 안에 불빛이 보였는데...",
        "...나를 부르는 목소리가 들려..."
    };

    private bool _isHallucinating;

    void Start()
    {
        if (survivalTimer == null)
            survivalTimer = GetComponent<SurvivalTimer>();

        _playerCamera = GetComponentInChildren<Camera>(true);
        if (_playerCamera == null) _playerCamera = Camera.main;
        if (_playerCamera != null) _originalFov = _playerCamera.fieldOfView;

        if (hallucinationAudioSource == null)
        {
            hallucinationAudioSource = gameObject.AddComponent<AudioSource>();
            hallucinationAudioSource.spatialBlend = 1f;
        }

        _audioTimer = Random.Range(minAudioInterval, maxAudioInterval);
        _subtitleTimer = subtitleInterval;
    }

    void Update()
    {
        if (survivalTimer == null || _playerCamera == null) return;

        bool shouldHallucinate = !survivalTimer.inSafeZone
            && survivalTimer.timeRemaining <= hallucinationThreshold
            && survivalTimer.timeRemaining > 0;

        if (shouldHallucinate)
        {
            if (!_isHallucinating)
            {
                _isHallucinating = true;
                _originalFov = _playerCamera.fieldOfView;
            }
            HandleHallucinations();
        }
        else if (_isHallucinating)
        {
            _isHallucinating = false;
            _playerCamera.fieldOfView = _originalFov;
        }
    }

    void HandleHallucinations()
    {
        float urgency = 1f + (hallucinationThreshold - survivalTimer.timeRemaining) / hallucinationThreshold;

        // FOV 울렁거림
        float pulse = Mathf.Sin(Time.time * fovPulseSpeed * urgency) * (fovPulseAmount * urgency);
        _playerCamera.fieldOfView = _originalFov + pulse;

        // 등 뒤 가짜 발소리
        _audioTimer -= Time.deltaTime;
        if (_audioTimer <= 0f)
        {
            if (fakeFootsteps != null && fakeFootsteps.Length > 0 && hallucinationAudioSource != null)
            {
                AudioClip clip = fakeFootsteps[Random.Range(0, fakeFootsteps.Length)];
                hallucinationAudioSource.transform.position = transform.position - transform.forward * 2f;
                hallucinationAudioSource.pitch = Random.Range(0.8f, 1.2f);
                hallucinationAudioSource.PlayOneShot(clip, Random.Range(0.5f, 1f));
            }
            _audioTimer = Random.Range(minAudioInterval, maxAudioInterval) / urgency;
        }

        // 환각 자막
        _subtitleTimer -= Time.deltaTime;
        if (_subtitleTimer <= 0f)
        {
            if (SubtitleManager.Instance != null)
            {
                string msg = _creepySubtitles[Random.Range(0, _creepySubtitles.Length)];
                SubtitleManager.Instance.ShowSubtitle(msg, 3f, false);
            }
            _subtitleTimer = subtitleInterval;
        }
    }
}

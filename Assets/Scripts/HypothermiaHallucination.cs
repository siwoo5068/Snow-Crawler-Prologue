using UnityEngine;
using System.Collections;

public class HypothermiaHallucination : MonoBehaviour
{
    [Header("References")]
    public SurvivalTimer survivalTimer;
    private Camera _playerCamera;
    
    [Header("Settings")]
    public float hallucinationThreshold = 15f; // 체온이 이 시간 이하로 떨어지면 발동
    
    [Header("Visual (FOV Pulse)")]
    public float fovPulseSpeed = 5f;
    public float fovPulseAmount = 3f;
    private float _originalFov;

    [Header("Audio (Fake Footsteps / Heartbeat)")]
    public AudioSource hallucinationAudioSource;
    public AudioClip[] fakeFootsteps;
    public float minAudioInterval = 2f;
    public float maxAudioInterval = 5f;
    private float _audioTimer;

    [Header("Subtitles")]
    public float subtitleInterval = 6f;
    private float _subtitleTimer;
    private string[] _creepySubtitles = new string[] {
        "누군가 내 뒤에 있는 것 같아...",
        "갑자기... 몸이 왜 이렇게 따뜻하지?",
        "저기 별장 안에 불빛이 보였는데...",
        "...나를 부르는 목소리가 들려..."
    };

    private bool _isHallucinating = false;

    void Start()
    {
        if (survivalTimer == null)
            survivalTimer = GetComponent<SurvivalTimer>();

        _playerCamera = GetComponentInChildren<Camera>(true);
        if (_playerCamera == null) _playerCamera = Camera.main;

        if (_playerCamera != null)
            _originalFov = _playerCamera.fieldOfView;

        // 자체 오디오 소스가 없으면 자동 생성
        if (hallucinationAudioSource == null)
        {
            hallucinationAudioSource = gameObject.AddComponent<AudioSource>();
            hallucinationAudioSource.spatialBlend = 1f; // 3D sound
        }

        _audioTimer = Random.Range(minAudioInterval, maxAudioInterval);
        _subtitleTimer = subtitleInterval;
    }

    void Update()
    {
        if (survivalTimer == null || _playerCamera == null) return;

        // 체온이 임계치 이하이고 야외일 때 환각 발동
        if (!survivalTimer.inSafeZone && survivalTimer.timeRemaining <= hallucinationThreshold && survivalTimer.timeRemaining > 0)
        {
            if (!_isHallucinating)
            {
                _isHallucinating = true;
                _originalFov = _playerCamera.fieldOfView; // 시작할 때 기준 FOV 저장
            }

            HandleHallucinations();
        }
        else
        {
            if (_isHallucinating)
            {
                // 환각 종료 (안전 구역 진입 또는 시간 회복)
                _isHallucinating = false;
                _playerCamera.fieldOfView = _originalFov; // FOV 원상복구
            }
        }
    }

    void HandleHallucinations()
    {
        // 1. 심장 박동에 맞춘 FOV 울렁거림 (심리적 압박감)
        // 시간이 줄어들수록 박동이 더 빠르고 강해지도록 설정할 수도 있음
        float urgency = 1f + ((hallucinationThreshold - survivalTimer.timeRemaining) / hallucinationThreshold);
        float pulse = Mathf.Sin(Time.time * fovPulseSpeed * urgency) * (fovPulseAmount * urgency);
        _playerCamera.fieldOfView = _originalFov + pulse;

        // 2. 등 뒤에서 들리는 가짜 발소리 / 이명
        _audioTimer -= Time.deltaTime;
        if (_audioTimer <= 0f)
        {
            if (fakeFootsteps != null && fakeFootsteps.Length > 0 && hallucinationAudioSource != null)
            {
                AudioClip clip = fakeFootsteps[Random.Range(0, fakeFootsteps.Length)];
                
                // 플레이어 약간 뒤쪽에서 소리가 나도록 트릭 (오디오 소스 위치를 등 뒤로 이동)
                hallucinationAudioSource.transform.position = transform.position - transform.forward * 2f;
                
                hallucinationAudioSource.pitch = Random.Range(0.8f, 1.2f);
                hallucinationAudioSource.PlayOneShot(clip, Random.Range(0.5f, 1f));
            }
            
            _audioTimer = Random.Range(minAudioInterval, maxAudioInterval) / urgency;
        }

        // 3. 기괴한 환각 자막
        _subtitleTimer -= Time.deltaTime;
        if (_subtitleTimer <= 0f)
        {
            if (SubtitleManager.Instance != null)
            {
                string msg = _creepySubtitles[Random.Range(0, _creepySubtitles.Length)];
                // 우선순위를 낮게 줘서 중요한 게임오버 자막 등을 덮지 않도록 할 수 있음 (우리는 덮어쓰기 false로 보냄)
                SubtitleManager.Instance.ShowSubtitle(msg, 3f, false); 
            }
            _subtitleTimer = subtitleInterval;
        }
    }
}

using UnityEngine;

// =============================================================================
//  RadioStatic — 라디오 치지직 잡음 생성기 (오디오 파일 없이 프로시저럴)
//
//  EndingManager가 TriggerStatic()을 호출하면:
//   1. 화이트 노이즈 + 간헐적 "삐-" 톤 재생 시작
//   2. 지정된 시간 후 자동 페이드아웃
//
//  Radio 오브젝트에 부착하면 플레이어가 가까이 갈수록 소리가 커짐 (3D)
// =============================================================================
[RequireComponent(typeof(AudioSource))]
public class RadioStatic : MonoBehaviour
{
    [Header("Static Settings")]
    [Tooltip("잡음 볼륨")]
    public float staticVolume = 0.4f;
    [Tooltip("간헐적 톤(삐-) 빈도 (0~1)")]
    public float toneChance = 0.002f;
    [Tooltip("톤 주파수 (Hz)")]
    public float toneFrequency = 800f;

    private AudioSource _audioSource;
    private bool _isPlaying = false;
    private float _fadeTimer = -1f;
    private float _fadeDuration;
    private float _currentVolume;
    private float _tonePhase;

    // 재생 중인지 외부에서 확인용
    public bool IsPlaying => _isPlaying;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.spatialBlend = 1f;  // 3D 사운드
        _audioSource.rolloffMode = AudioRolloffMode.Linear;
        _audioSource.minDistance = 1f;
        _audioSource.maxDistance = 15f;
        _audioSource.loop = true;
        _audioSource.playOnAwake = false;
        _audioSource.volume = 0f;
    }

    /// <summary>
    /// 라디오 잡음 재생 시작.
    /// duration초 후 자동 페이드아웃. duration=0이면 수동 Stop 전까지 재생.
    /// </summary>
    public void TriggerStatic(float duration = 0f, float fadeOut = 2f)
    {
        _isPlaying = true;
        _currentVolume = staticVolume;
        _audioSource.volume = staticVolume;

        if (!_audioSource.isPlaying)
            _audioSource.Play();

        if (duration > 0f)
        {
            _fadeTimer = duration;
            _fadeDuration = fadeOut;
        }

        Debug.Log($"[RadioStatic] 잡음 재생 시작 (duration={duration}s)");
    }

    /// <summary>
    /// 잡음 즉시 중지
    /// </summary>
    public void StopStatic()
    {
        _isPlaying = false;
        _audioSource.volume = 0f;
        _audioSource.Stop();
    }

    void Update()
    {
        if (!_isPlaying) return;

        // 페이드아웃 타이머
        if (_fadeTimer > 0f)
        {
            _fadeTimer -= Time.deltaTime;

            if (_fadeTimer <= _fadeDuration)
            {
                float t = Mathf.Clamp01(_fadeTimer / _fadeDuration);
                _audioSource.volume = _currentVolume * t;
            }

            if (_fadeTimer <= 0f)
            {
                StopStatic();
            }
        }
    }

    // ── 프로시저럴 화이트 노이즈 생성 ────────────────────────────────────
    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!_isPlaying) return;

        for (int i = 0; i < data.Length; i += channels)
        {
            // 기본: 화이트 노이즈
            float noise = (Random.Range(0f, 1f) * 2f - 1f) * 0.3f;

            // 간헐적 톤 (삐- 소리)
            float tone = 0f;
            if (Random.value < toneChance)
            {
                _tonePhase += toneFrequency * 2f * Mathf.PI / AudioSettings.outputSampleRate;
                if (_tonePhase > Mathf.PI * 2f) _tonePhase -= Mathf.PI * 2f;
                tone = Mathf.Sin(_tonePhase) * 0.15f;
            }

            float sample = noise + tone;

            // 모든 채널에 적용
            for (int ch = 0; ch < channels; ch++)
            {
                data[i + ch] = sample;
            }
        }
    }
}

using UnityEngine;

// =============================================================================
//  RadioStatic — 별장 안 라디오에서 치지직 잡음 재생
//  EndingManager가 2막 전환 시 TriggerStatic()을 호출하면
//  절차적 화이트 노이즈를 생성하여 라디오에서 소리가 남
//  나중에 실제 오디오 클립이 생기면 radioStaticClip에 연결 가능
// =============================================================================
[RequireComponent(typeof(AudioSource))]
public class RadioStatic : MonoBehaviour
{
    [Tooltip("라디오 잡음 오디오 클립. 비어있으면 절차적 노이즈 자동 생성.")]
    public AudioClip radioStaticClip;

    [Tooltip("최대 볼륨")]
    public float maxVolume = 0.5f;

    private AudioSource _source;
    private bool _isPlaying = false;
    private float _fadeOutStart;
    private float _fadeOutDuration;
    private float _stopTime;

    void Awake()
    {
        _source = GetComponent<AudioSource>();
        if (_source == null)
            _source = gameObject.AddComponent<AudioSource>();

        _source.spatialBlend = 1f;   // 3D 사운드 (라디오에서 나는 느낌)
        _source.minDistance = 1f;
        _source.maxDistance = 15f;
        _source.loop = true;
        _source.playOnAwake = false;
        _source.volume = 0f;
    }

    /// <summary>
    /// 라디오 잡음 시작. EndingManager에서 호출.
    /// </summary>
    /// <param name="duration">총 재생 시간 (초)</param>
    /// <param name="fadeOutTime">마지막 n초 동안 페이드아웃</param>
    public void TriggerStatic(float duration = 20f, float fadeOutTime = 3f)
    {
        if (_isPlaying) return;
        _isPlaying = true;

        _stopTime = Time.time + duration;
        _fadeOutStart = _stopTime - fadeOutTime;
        _fadeOutDuration = fadeOutTime;

        // 오디오 클립 설정
        if (radioStaticClip != null)
        {
            _source.clip = radioStaticClip;
        }
        else
        {
            // 절차적 화이트 노이즈 생성
            _source.clip = GenerateWhiteNoise(2f, 44100);
        }

        _source.volume = maxVolume;
        _source.Play();

        Debug.Log("[RadioStatic] 라디오 잡음 시작!");
    }

    void Update()
    {
        if (!_isPlaying) return;

        float now = Time.time;

        // 페이드아웃 구간
        if (now >= _fadeOutStart && now < _stopTime)
        {
            float remaining = _stopTime - now;
            float t = remaining / _fadeOutDuration;
            _source.volume = maxVolume * t;
        }

        // 종료
        if (now >= _stopTime)
        {
            _source.Stop();
            _source.volume = 0f;
            _isPlaying = false;
        }
    }

    /// <summary>
    /// 절차적 화이트 노이즈 클립 생성 (라디오 치지직 느낌)
    /// </summary>
    AudioClip GenerateWhiteNoise(float lengthSec, int sampleRate)
    {
        int sampleCount = (int)(lengthSec * sampleRate);
        float[] samples = new float[sampleCount];

        // 기본 화이트 노이즈 + 간헐적 크래클
        for (int i = 0; i < sampleCount; i++)
        {
            float noise = Random.Range(-1f, 1f) * 0.4f;

            // 간헐적 크래클 (치지직 느낌)
            if (Random.value < 0.003f)
                noise += Random.Range(-1f, 1f) * 0.8f;

            // 저주파 필터링 (부드러운 잡음)
            samples[i] = noise;
        }

        // 간단한 로우패스 (이전 샘플과 혼합)
        for (int i = 1; i < sampleCount; i++)
        {
            samples[i] = samples[i] * 0.3f + samples[i - 1] * 0.7f;
        }

        var clip = AudioClip.Create("RadioStatic", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}

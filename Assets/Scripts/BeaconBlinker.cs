using UnityEngine;

// =============================================================================
//  BeaconBlinker — 중계기 꼭대기 빨간 점멸등
//  멀리서 플레이어가 "저 빨간 불빛이 뭐지?" 하고 알아볼 수 있도록 깜빡임
// =============================================================================
public class BeaconBlinker : MonoBehaviour
{
    [Tooltip("깜빡임 속도 (초당 주기)")]
    public float blinkSpeed = 1.5f;

    [Tooltip("최소 밝기 (0에 가까울수록 완전히 꺼짐)")]
    public float minIntensity = 0.5f;

    private Light _light;
    private float _baseIntensity;

    void Start()
    {
        _light = GetComponent<Light>();
        if (_light != null)
            _baseIntensity = _light.intensity;
    }

    void Update()
    {
        if (_light == null) return;

        // 사인 파형으로 부드러운 점멸
        float pulse = (Mathf.Sin(Time.time * blinkSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
        _light.intensity = Mathf.Lerp(minIntensity, _baseIntensity, pulse);
    }
}

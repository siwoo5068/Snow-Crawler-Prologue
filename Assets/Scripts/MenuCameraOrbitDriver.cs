using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// MenuVirtualCamera의 OrbitalFollow 수평축을 자동으로 천천히 회전시킵니다.
/// MainMenuManager가 메뉴 상태일 때만 활성화됩니다.
/// </summary>
[RequireComponent(typeof(CinemachineCamera))]
public class MenuCameraOrbitDriver : MonoBehaviour
{
    [Header("Rotation")]
    [Tooltip("초당 회전 속도 (도). 낮을수록 느리게 돔.")]
    public float autoRotateDegPerSec = 6f;  // 1분에 한 바퀴 = 6 deg/s

    private CinemachineOrbitalFollow _orbital;

    void Awake()
    {
        _orbital = GetComponent<CinemachineOrbitalFollow>();
    }

    void Update()
    {
        if (_orbital == null) return;

        // HorizontalAxis.Value를 매 프레임 조금씩 증가 → 자동 회전
        var axis = _orbital.HorizontalAxis;
        axis.Value += autoRotateDegPerSec * Time.deltaTime;

        // Wrap: 360도를 넘으면 0으로 돌아옴 (Wrap = true 필요)
        if (axis.Value > 180f)  axis.Value -= 360f;
        if (axis.Value < -180f) axis.Value += 360f;

        _orbital.HorizontalAxis = axis;
    }
}

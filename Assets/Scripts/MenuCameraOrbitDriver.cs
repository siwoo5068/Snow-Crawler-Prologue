using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(CinemachineCamera))]
public class MenuCameraOrbitDriver : MonoBehaviour
{
    [Header("Rotation")]
    [Tooltip("Auto-rotation speed in degrees per second. Lower = slower.")]
    public float autoRotateDegPerSec = 6f;

    private CinemachineOrbitalFollow _orbital;

    void Awake()
    {
        _orbital = GetComponent<CinemachineOrbitalFollow>();
    }

    void Update()
    {
        if (_orbital == null) return;

        var axis = _orbital.HorizontalAxis;
        axis.Value += autoRotateDegPerSec * Time.deltaTime;

        if (axis.Value > 180f)  axis.Value -= 360f;
        if (axis.Value < -180f) axis.Value += 360f;

        _orbital.HorizontalAxis = axis;
    }
}

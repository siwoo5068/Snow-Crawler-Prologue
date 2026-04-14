using UnityEngine;

/// <summary>
/// Dolly-style camera path mover.
/// MainMenuCamera에 붙여서 여러 웨이포인트를 천천히 순회하게 합니다.
/// </summary>
public class CameraPath : MonoBehaviour
{
    [System.Serializable]
    public struct Waypoint
    {
        public Vector3 position;
        public Vector3 eulerRotation;
    }

    [Header("Path Settings")]
    public Waypoint[] waypoints;          // Inspector에서 경로 지정
    public float speed = 1.5f;            // 이동 속도 (units/sec)
    public float rotationSpeed = 1f;      // 회전 보간 속도
    public bool loop = true;              // 루프 여부

    private int _targetIndex = 1;
    private bool _active = true;

    void OnEnable()
    {
        if (waypoints == null || waypoints.Length < 2)
        {
            _active = false;
            return;
        }

        // 시작 위치로 텔레포트
        transform.position    = waypoints[0].position;
        transform.eulerAngles = waypoints[0].eulerRotation;
        _targetIndex = 1;
        _active = true;
    }

    void Update()
    {
        if (!_active || waypoints == null || waypoints.Length < 2) return;

        Waypoint target = waypoints[_targetIndex];

        // 위치 이동
        transform.position = Vector3.MoveTowards(
            transform.position, target.position, speed * Time.deltaTime);

        // 회전 보간
        Quaternion targetRot = Quaternion.Euler(target.eulerRotation);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

        // 목표 도달 판정
        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            _targetIndex++;
            if (_targetIndex >= waypoints.Length)
            {
                if (loop)
                    _targetIndex = 0;
                else
                    _active = false;
            }
        }
    }

    /// <summary>
    /// MainMenuManager에서 메뉴 종료 시 호출해 경로 정지.
    /// </summary>
    public void Stop() => _active = false;
}

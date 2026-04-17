using UnityEngine;

public class CameraPath : MonoBehaviour
{
    [System.Serializable]
    public struct Waypoint
    {
        public Vector3 position;
        public Vector3 eulerRotation;
    }

    [Header("Path Settings")]
    public Waypoint[] waypoints;
    public float speed = 1.5f;
    public float rotationSpeed = 1f;
    public bool loop = true;

    private int _targetIndex = 1;
    private bool _active = true;

    void OnEnable()
    {
        if (waypoints == null || waypoints.Length < 2)
        {
            _active = false;
            return;
        }

        transform.position    = waypoints[0].position;
        transform.eulerAngles = waypoints[0].eulerRotation;
        _targetIndex = 1;
        _active = true;
    }

    void Update()
    {
        if (!_active || waypoints == null || waypoints.Length < 2) return;

        Waypoint target = waypoints[_targetIndex];

        transform.position = Vector3.MoveTowards(
            transform.position, target.position, speed * Time.deltaTime);

        Quaternion targetRot = Quaternion.Euler(target.eulerRotation);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

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

    public void Stop() => _active = false;
}

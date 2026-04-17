using UnityEngine;
using UnityEngine.UI;

public class CabinCompass : MonoBehaviour
{
    [Header("References")]
    public SurvivalTimer survivalTimer;
    public Transform cabinTarget;

    [Header("Compass Dot")]
    public RectTransform compassDot;
    public Image dotImage;

    [Header("Settings")]
    public float edgeMargin = 60f;
    public float maxDistance = 50f;
    public float minDistance = 5f;
    public Color dotColor = new Color(0.6f, 0.85f, 1f, 1f);
    public float pulseSpeed = 1.5f;
    public float pulseAmount = 0.25f;

    private Camera _cam;
    private float _pulseTimer;
    private float _currentPulseSpeed;

    void Start()
    {
        _currentPulseSpeed = pulseSpeed;
        ResolveReferences();
    }

    void OnEnable()
    {
        ResolveCamera();
    }

    void ResolveReferences()
    {
        ResolveCamera();

        if (survivalTimer == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null) survivalTimer = player.GetComponent<SurvivalTimer>();
        }

        if (cabinTarget == null)
        {
            var sz = GameObject.FindGameObjectWithTag("SafeZone");
            if (sz != null) cabinTarget = sz.transform;
        }

        if (dotImage != null)
            dotImage.color = dotColor;
    }

    void ResolveCamera()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var cam = player.GetComponentInChildren<Camera>(true);
            if (cam != null)
            {
                _cam = cam;
                return;
            }
        }
        _cam = Camera.main;
    }

    void Update()
    {
        if (compassDot == null || cabinTarget == null || survivalTimer == null) return;
        if (_cam == null || !_cam.enabled || !_cam.gameObject.activeInHierarchy)
        {
            if (compassDot.gameObject.activeSelf) compassDot.gameObject.SetActive(false);
            return;
        }

        if (survivalTimer.inSafeZone)
        {
            compassDot.gameObject.SetActive(false);
            return;
        }

        float dist = Vector3.Distance(_cam.transform.position, cabinTarget.position);

        if (dist < minDistance)
        {
            compassDot.gameObject.SetActive(false);
            return;
        }

        compassDot.gameObject.SetActive(true);

        Vector3 screenPos = _cam.WorldToScreenPoint(cabinTarget.position);
        bool isBehind = screenPos.z < 0;
        if (isBehind)
        {
            screenPos.x = Screen.width - screenPos.x;
            screenPos.y = Screen.height - screenPos.y;
        }

        Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 dir = new Vector2(screenPos.x - center.x, screenPos.y - center.y).normalized;

        float hw = Screen.width * 0.5f - edgeMargin;
        float hh = Screen.height * 0.5f - edgeMargin;

        float scaleX = Mathf.Abs(dir.x) > 0.001f ? hw / Mathf.Abs(dir.x) : float.MaxValue;
        float scaleY = Mathf.Abs(dir.y) > 0.001f ? hh / Mathf.Abs(dir.y) : float.MaxValue;
        float scale = Mathf.Min(scaleX, scaleY);

        Vector2 edgePos = center + dir * scale;
        compassDot.position = new Vector3(edgePos.x, edgePos.y, 0f);

        float alpha = Mathf.Clamp01((dist - minDistance) / (maxDistance - minDistance));
        _currentPulseSpeed = Mathf.Lerp(1.5f, 4f, 1f - Mathf.Clamp01(dist / maxDistance));
        _pulseTimer += Time.deltaTime * _currentPulseSpeed;
        _pulseTimer %= Mathf.PI * 2f;

        float pulse = 1f - pulseAmount + Mathf.Sin(_pulseTimer) * pulseAmount;
        Color c = dotColor;
        c.a = alpha * pulse;
        dotImage.color = c;
    }
}

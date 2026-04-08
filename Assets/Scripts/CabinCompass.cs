using UnityEngine;
using UnityEngine.UI;

public class CabinCompass : MonoBehaviour
{
    [Header("References")]
    public SurvivalTimer survivalTimer;
    public Transform cabinTarget;   // SafeZone 또는 오두막 오브젝트

    [Header("Compass Dot")]
    public RectTransform compassDot;
    public Image dotImage;

    [Header("Settings")]
    public float edgeMargin = 60f;       // 화면 가장자리 여백
    public float maxDistance = 50f;      // 이 거리 이상이면 알파 최대
    public float minDistance = 5f;       // 이 거리 이하면 숨김
    public Color dotColor = new Color(0.6f, 0.85f, 1f, 1f);   // 차가운 파란빛
    public float pulseSpeed = 1.5f;
    public float pulseAmount = 0.25f;

    private Camera _cam;
    private float _pulseTimer;

    void Start()
    {
        _cam = Camera.main;

        if (survivalTimer == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player) survivalTimer = player.GetComponent<SurvivalTimer>();
        }

        // SafeZone 자동 탐색
        if (cabinTarget == null)
        {
            var sz = GameObject.FindGameObjectWithTag("SafeZone");
            if (sz) cabinTarget = sz.transform;
        }

        if (dotImage != null)
            dotImage.color = dotColor;
    }

    void Update()
    {
        if (compassDot == null || cabinTarget == null || survivalTimer == null) return;

        // 안전지대 안이면 숨기기
        if (survivalTimer.inSafeZone)
        {
            compassDot.gameObject.SetActive(false);
            return;
        }

        float dist = Vector3.Distance(
            _cam.transform.position,
            cabinTarget.position);

        // 너무 가까우면 숨기기
        if (dist < minDistance)
        {
            compassDot.gameObject.SetActive(false);
            return;
        }

        compassDot.gameObject.SetActive(true);

        // 화면 내 방향 → 가장자리로 클램프
        Vector3 screenPos = _cam.WorldToScreenPoint(cabinTarget.position);
        bool isBehind = screenPos.z < 0;
        if (isBehind)
        {
            screenPos.x = Screen.width - screenPos.x;
            screenPos.y = Screen.height - screenPos.y;
        }

        // 화면 중앙 기준 방향 벡터
        Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 dir = new Vector2(screenPos.x - center.x, screenPos.y - center.y).normalized;

        // 가장자리 위치 계산
        float hw = Screen.width * 0.5f - edgeMargin;
        float hh = Screen.height * 0.5f - edgeMargin;

        float scaleX = Mathf.Abs(dir.x) > 0.001f ? hw / Mathf.Abs(dir.x) : float.MaxValue;
        float scaleY = Mathf.Abs(dir.y) > 0.001f ? hh / Mathf.Abs(dir.y) : float.MaxValue;
        float scale = Mathf.Min(scaleX, scaleY);

        Vector2 edgePos = center + dir * scale;
        compassDot.position = new Vector3(edgePos.x, edgePos.y, 0f);

        // 거리에 따른 알파 + 펄스
        float alpha = Mathf.Clamp01((dist - minDistance) / (maxDistance - minDistance));
        _pulseTimer += Time.deltaTime * pulseSpeed;
        float pulse = 1f - pulseAmount + Mathf.Sin(_pulseTimer) * pulseAmount;

        Color c = dotColor;
        c.a = alpha * pulse;
        dotImage.color = c;

        // 거리 가까울수록 더 빠르게 펄스 (긴장감)
        pulseSpeed = Mathf.Lerp(1.5f, 4f, 1f - Mathf.Clamp01(dist / maxDistance));
    }
}

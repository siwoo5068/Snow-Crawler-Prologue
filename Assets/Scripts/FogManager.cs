using UnityEngine;

public class FogManager : MonoBehaviour
{
    [Header("References")]
    public SurvivalTimer survivalTimer;
    private SortieManager _sortieManager;

    [Header("Fog Settings (Safe Zone)")]
    public float safeZoneFogDensity = 0.002f;
    public Color safeZoneFogColor = new Color(0.8f, 0.85f, 0.9f);

    [Header("Fog Escalation (Outside)")]
    [Tooltip("출격 횟수 1일 때의 밝고 옅은 안개")]
    public float minOutsideDensity = 0.03f;
    public Color minOutsideColor = new Color(0.7f, 0.75f, 0.8f);

    [Tooltip("안개가 최대로 짙어지는 출격 횟수")]
    public int maxFogSortieCount = 5;

    [Tooltip("가장 짙고 어두울 때의 안개")]
    public float maxOutsideDensity = 0.12f;
    public Color maxOutsideColor = new Color(0.05f, 0.08f, 0.12f); // 아주 어두운 검푸른색

    [Header("Transition")]
    public float transitionSpeed = 1.5f;

    private float _targetDensity;
    private Color _targetColor;

    void Start()
    {
        // 참조 자동 검색
        if (survivalTimer == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
                survivalTimer = player.GetComponent<SurvivalTimer>();
        }

        _sortieManager = Object.FindFirstObjectByType<SortieManager>();

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = safeZoneFogColor;
        RenderSettings.fogDensity = safeZoneFogDensity;
    }

    void Update()
    {
        if (survivalTimer == null) return;

        if (survivalTimer.inSafeZone)
        {
            // 별장 내부: 옅고 밝은 안개 유지
            _targetDensity = safeZoneFogDensity;
            _targetColor = safeZoneFogColor;
        }
        else
        {
            // 야외: 출격 횟수에 비례하여 안개가 점점 어둡고 짙어짐
            int sortieCount = _sortieManager != null ? _sortieManager.SortieCount : 1;
            
            // t = 0 (첫 출격) ~ 1 (최대 안개 도달 시)
            float t = Mathf.Clamp01((float)(sortieCount - 1) / Mathf.Max(1, maxFogSortieCount - 1));

            _targetDensity = Mathf.Lerp(minOutsideDensity, maxOutsideDensity, t);
            _targetColor = Color.Lerp(minOutsideColor, maxOutsideColor, t);
        }

        // 부드러운 전환 적용
        RenderSettings.fogDensity = Mathf.MoveTowards(
            RenderSettings.fogDensity, _targetDensity, transitionSpeed * Time.deltaTime);

        // 색상 전환은 속도를 조금 조절해서 자연스럽게 블렌딩
        RenderSettings.fogColor = Color.Lerp(
            RenderSettings.fogColor, _targetColor, transitionSpeed * Time.deltaTime);
    }
}

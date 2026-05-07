using UnityEngine;

public class FogManager : MonoBehaviour
{
    [Header("References")]
    public SurvivalTimer survivalTimer;
    private SortieManager _sortieManager;

    [Header("Fog Settings (Safe Zone)")]
    public float safeZoneFogDensity = 0.002f;
    public Color safeZoneFogColor = new Color(0.8f, 0.85f, 0.9f);

    [Header("Fog Escalation (Outside Base)")]
    public float minOutsideDensity = 0.03f;
    public Color minOutsideColor = new Color(0.7f, 0.75f, 0.8f);
    public int maxFogSortieCount = 5;
    public float maxOutsideDensity = 0.12f;
    public Color maxOutsideColor = new Color(0.05f, 0.08f, 0.12f);

    [Header("Blizzard Waves (Periodic Storm)")]
    public float waveIntervalMin = 45f; // 돌풍 사이 최소 대기 시간
    public float waveIntervalMax = 75f; // 돌풍 사이 최대 대기 시간
    public float waveDuration = 12f;    // 돌풍 지속 시간
    public float waveFogDensity = 0.4f; // 돌풍 시 극단적인 시야 차단 농도
    public Color waveFogColor = new Color(0.9f, 0.92f, 0.95f); // 칠흑 속에서 번쩍이는 화이트아웃

    [Header("Transition")]
    public float transitionSpeed = 1.5f;

    private float _targetDensity;
    private Color _targetColor;
    
    private bool _isWaveActive = false;
    private float _waveTimer = 0f;

    private string[] _warningSubtitles = new string[] {
        "갑자기 바람 소리가 멎었다... 불길한 침묵이다.",
        "피부를 찢을 듯한 냉기가 몰려온다. 돌풍이야...!",
        "하늘이 짓눌리는 듯한 굉음이 들려온다... 버텨야 해!"
    };

    void Start()
    {
        if (survivalTimer == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null) survivalTimer = player.GetComponent<SurvivalTimer>();
        }

        _sortieManager = Object.FindFirstObjectByType<SortieManager>();

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = safeZoneFogColor;
        RenderSettings.fogDensity = safeZoneFogDensity;

        // 첫 웨이브 타이머 세팅
        _waveTimer = Random.Range(waveIntervalMin, waveIntervalMax);
    }

    void Update()
    {
        if (survivalTimer == null) return;

        if (survivalTimer.inSafeZone)
        {
            // 별장 내부: 옅고 밝은 안개 유지, 웨이브 강제 종료 및 타이머 초기화
            _targetDensity = safeZoneFogDensity;
            _targetColor = safeZoneFogColor;
            
            if (_isWaveActive) _isWaveActive = false;
            _waveTimer = Random.Range(waveIntervalMin, waveIntervalMax); 
        }
        else
        {
            // 1. 기본 야외 안개 (출격 횟수 비례)
            int sortieCount = _sortieManager != null ? _sortieManager.SortieCount : 1;
            float t = Mathf.Clamp01((float)(sortieCount - 1) / Mathf.Max(1, maxFogSortieCount - 1));

            float baseDensity = Mathf.Lerp(minOutsideDensity, maxOutsideDensity, t);
            Color baseColor = Color.Lerp(minOutsideColor, maxOutsideColor, t);

            // 2. 웨이브 타이머 틱
            _waveTimer -= Time.deltaTime;

            if (!_isWaveActive && _waveTimer <= 0f)
            {
                // 웨이브 시작
                _isWaveActive = true;
                _waveTimer = waveDuration;
                
                // 경고 자막 랜덤 출력
                if (SubtitleManager.Instance != null)
                {
                    string msg = _warningSubtitles[Random.Range(0, _warningSubtitles.Length)];
                    SubtitleManager.Instance.ShowSubtitle(msg, 4f, true);
                }
            }
            else if (_isWaveActive && _waveTimer <= 0f)
            {
                // 웨이브 종료
                _isWaveActive = false;
                _waveTimer = Random.Range(waveIntervalMin, waveIntervalMax);
                
                if (SubtitleManager.Instance != null)
                {
                    SubtitleManager.Instance.ShowSubtitle("폭풍이 지나갔다... 살았어...", 3f);
                }
            }

            // 3. 최종 목표값 설정 (웨이브 중이면 덮어쓰기)
            if (_isWaveActive)
            {
                _targetDensity = waveFogDensity;
                _targetColor = waveFogColor;
            }
            else
            {
                _targetDensity = baseDensity;
                _targetColor = baseColor;
            }
        }

        // 전환 (웨이브 중에는 눈보라가 급격하게 몰아치도록 transition 속도를 조금 빠르게 할 수도 있지만 기본값 유지)
        float currentSpeed = _isWaveActive ? transitionSpeed * 2f : transitionSpeed;

        RenderSettings.fogDensity = Mathf.MoveTowards(
            RenderSettings.fogDensity, _targetDensity, currentSpeed * Time.deltaTime);

        RenderSettings.fogColor = Color.Lerp(
            RenderSettings.fogColor, _targetColor, currentSpeed * Time.deltaTime);
    }
}

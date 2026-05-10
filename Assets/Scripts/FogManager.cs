using UnityEngine;

public class FogManager : MonoBehaviour
{
    [Header("References")]
    public SurvivalTimer survivalTimer;
    private SortieManager _sortieManager;

    [Header("Safe Zone Fog")]
    public float safeZoneFogDensity = 0.008f;
    public Color safeZoneFogColor = new Color(0.80f, 0.84f, 0.90f);

    [Header("Outside Fog Escalation")]
    public float minOutsideDensity = 0.04f;
    public Color minOutsideColor = new Color(0.75f, 0.78f, 0.85f);
    public int maxFogSortieCount = 5;
    public float maxOutsideDensity = 0.15f;
    public Color maxOutsideColor = new Color(0.20f, 0.22f, 0.28f);

    [Header("Blizzard Waves")]
    public float waveIntervalMin = 45f;
    public float waveIntervalMax = 75f;
    public float waveDuration = 12f;
    public float waveFogDensity = 0.4f;
    public Color waveFogColor = new Color(0.9f, 0.92f, 0.95f);

    [Header("Transition")]
    public float transitionSpeed = 1.5f;

    private float _targetDensity;
    private Color _targetColor;
    private bool _isWaveActive;
    private float _waveTimer;
    private bool _wasInSafeZone = true;

    private readonly string[] _warningSubtitles = {
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

        // 안개 값 강제 설정 (씬 Inspector 값 덮어씀)
        safeZoneFogDensity = 0.008f;
        safeZoneFogColor = new Color(0.80f, 0.84f, 0.90f);
        minOutsideDensity = 0.04f;
        minOutsideColor = new Color(0.75f, 0.78f, 0.85f);
        maxOutsideDensity = 0.15f;
        maxOutsideColor = new Color(0.20f, 0.22f, 0.28f);

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = safeZoneFogColor;
        RenderSettings.fogDensity = safeZoneFogDensity;
        _waveTimer = Random.Range(waveIntervalMin, waveIntervalMax);
    }

    void Update()
    {
        if (survivalTimer == null) return;

        if (survivalTimer.inSafeZone)
        {
            _targetDensity = safeZoneFogDensity;
            _targetColor = safeZoneFogColor;

            // 귀환 직후 1회만 웨이브 리셋
            if (!_wasInSafeZone)
            {
                _isWaveActive = false;
                _waveTimer = Random.Range(waveIntervalMin, waveIntervalMax);
            }
            _wasInSafeZone = true;
        }
        else
        {
            _wasInSafeZone = false;

            int sortieCount = _sortieManager != null ? _sortieManager.SortieCount : 1;
            float t = Mathf.Clamp01((float)(sortieCount - 1) / Mathf.Max(1, maxFogSortieCount - 1));
            float baseDensity = Mathf.Lerp(minOutsideDensity, maxOutsideDensity, t);
            Color baseColor = Color.Lerp(minOutsideColor, maxOutsideColor, t);

            _waveTimer -= Time.deltaTime;

            if (!_isWaveActive && _waveTimer <= 0f)
            {
                _isWaveActive = true;
                _waveTimer = waveDuration;
                if (SubtitleManager.Instance != null)
                {
                    string msg = _warningSubtitles[Random.Range(0, _warningSubtitles.Length)];
                    SubtitleManager.Instance.ShowSubtitle(msg, 4f, true);
                }
            }
            else if (_isWaveActive && _waveTimer <= 0f)
            {
                _isWaveActive = false;
                _waveTimer = Random.Range(waveIntervalMin, waveIntervalMax);
                if (SubtitleManager.Instance != null)
                    SubtitleManager.Instance.ShowSubtitle("폭풍이 지나갔다... 살았어...", 3f);
            }

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

        float speed = _isWaveActive ? transitionSpeed * 2f : transitionSpeed;
        RenderSettings.fogDensity = Mathf.MoveTowards(RenderSettings.fogDensity, _targetDensity, speed * Time.deltaTime);
        RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, _targetColor, speed * Time.deltaTime);
    }
}

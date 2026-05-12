using UnityEngine;
using TMPro;

public class SortieManager : MonoBehaviour
{
    [Header("References")]
    public SurvivalTimer survivalTimer;
    public ItemSpawner itemSpawner;

    private EndingManager _endingManager;

    [Header("Cold Escalation")]
    public float initialColdMultiplier = 1f;
    public float coldEscalationPerReturn = 0.2f;
    public float maxColdMultiplier = 3f;
    public bool skipEscalationOnFirstSortie = true;

    [Header("HUD (Optional)")]
    public TextMeshProUGUI sortieHUDText;

    public int SortieCount
    {
        get => _sortieCount;
        set => _sortieCount = value;
    }

    private int   _sortieCount;
    private int   _returnCount;
    private float _nextColdMultiplier;
    private bool  _isOutside;

    void Start()
    {
        if (survivalTimer == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null) survivalTimer = player.GetComponent<SurvivalTimer>();
        }
        if (itemSpawner == null)
            itemSpawner = Object.FindFirstObjectByType<ItemSpawner>();

        _endingManager = Object.FindFirstObjectByType<EndingManager>();

        // 한파 에스컬레이션 값 강제 설정 (씬 Inspector 값 덮어씀)
        initialColdMultiplier = 1f;
        coldEscalationPerReturn = 0.15f;
        maxColdMultiplier = 2.5f;

        _nextColdMultiplier = initialColdMultiplier;
        ApplyColdMultiplier(initialColdMultiplier);
        UpdateHUD();
    }

    void Update()
    {
        if (survivalTimer == null) return;

        bool currentlyOutside = !survivalTimer.inSafeZone;

        if (currentlyOutside && !_isOutside)
        {
            _isOutside = true;
            OnSortieStart();
        }
        else if (!currentlyOutside && _isOutside)
        {
            _isOutside = false;
            OnSortieReturn();
        }
    }

    void OnSortieStart()
    {
        _sortieCount++;

        // 2막: 마지막 출격 감지 → 극한 환경 적용
        if (_endingManager != null && _endingManager.IsAct2 && !_endingManager.IsFinalSortie)
        {
            _endingManager.ActivateFinalSortie();
            if (SubtitleManager.Instance != null)
                SubtitleManager.Instance.ShowSubtitle("이번이 마지막이다... 중계기를 찾아야 해.", 5f, true);
        }

        float mult = (skipEscalationOnFirstSortie && _sortieCount == 1)
            ? initialColdMultiplier
            : _nextColdMultiplier;

        // 마지막 출격이면 EndingManager가 coldMultiplier를 직접 제어하므로 덮어쓰지 않음
        if (_endingManager == null || !_endingManager.IsFinalSortie)
            ApplyColdMultiplier(mult);

        UpdateHUD();

        if (SubtitleManager.Instance != null)
        {
            if (_sortieCount == 1)
                SubtitleManager.Instance.ShowSubtitle("눈보라가 거세다... 필요한 가구만 빠르게 챙겨야 해.", 4f);
            else if (_sortieCount == 3)
                SubtitleManager.Instance.ShowSubtitle("점점 더 추워지는 기분이야... 너무 오래 밖을 맴돌면 위험해.", 4f);
        }
    }

    void OnSortieReturn()
    {
        _returnCount++;

        if (itemSpawner != null) itemSpawner.RespawnAll();
        if (SaveManager.Instance != null) SaveManager.Instance.SaveGame();

        var fc = Object.FindFirstObjectByType<FlashlightController>();
        if (fc != null) fc.RechargeBattery();

        _nextColdMultiplier = Mathf.Min(
            initialColdMultiplier + coldEscalationPerReturn * _returnCount,
            maxColdMultiplier);

        ApplyColdMultiplier(initialColdMultiplier);
        UpdateHUD();
    }

    void ApplyColdMultiplier(float value)
    {
        if (survivalTimer != null)
            survivalTimer.coldMultiplier = value;
    }

    void UpdateHUD()
    {
        if (sortieHUDText == null) return;

        if (_sortieCount == 0)
            sortieHUDText.text = "별장 밖으로 나가 가구를 찾으세요";
        else if (!_isOutside)
            sortieHUDText.text = _returnCount > 0
                ? $"귀환 완료  |  다음 한파: {_nextColdMultiplier:F1}x" : "";
        else
        {
            float cur = survivalTimer != null ? survivalTimer.coldMultiplier : 1f;
            sortieHUDText.text = $"출격 #{_sortieCount}  ❄ 한파: {cur:F1}x";
        }
    }
}

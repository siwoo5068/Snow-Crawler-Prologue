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

        // coldMultiplier는 항상 1.0x (2막 마지막 출격은 EndingManager가 제어)
        if (_endingManager == null || !_endingManager.IsFinalSortie)
            ApplyColdMultiplier(1f);

        UpdateHUD();

        if (SubtitleManager.Instance != null)
        {
            if (_sortieCount == 1)
                SubtitleManager.Instance.ShowSubtitle("눈보라가 거세다... 필요한 가구만 빠르게 챙겨야 해.", 4f);
        }
    }

    void OnSortieReturn()
    {
        _returnCount++;

        if (itemSpawner != null) itemSpawner.RespawnAll();
        if (SaveManager.Instance != null) SaveManager.Instance.SaveGame();

        var fc = Object.FindFirstObjectByType<FlashlightController>();
        if (fc != null) fc.RechargeBattery();

        ApplyColdMultiplier(1f);
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
            sortieHUDText.text = _returnCount > 0 ? "귀환 완료" : "";
        else
            sortieHUDText.text = $"출격 #{_sortieCount}";
    }
}

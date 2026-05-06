using UnityEngine;
using TMPro;

// =============================================================================
//  SortieManager — 트리거 기반 자동 출격 루프 관리
//
//  ★ 흐름 (키 입력 없음, 완전 자동) ★
//
//  [게임 시작]
//    └─ ItemSpawner.Start() → 아이템 첫 스폰
//
//  [SafeZone Exit — 플레이어가 별장 밖으로 나감]
//    └─ OnSortieStart() 호출
//       ├─ coldMultiplier 적용 (야외에 나가는 순간부터 더 추워짐)
//       └─ 출격 횟수 +1
//
//  [SafeZone Enter — 플레이어가 별장으로 귀환]
//    └─ OnSortieReturn() 호출
//       ├─ 아이템 리스폰 (다음 출격을 위해)
//       └─ 다음 출격 coldMultiplier 예고 계산
//
//  ★ F키 충돌 제거 ★
//   F키는 FlashlightController 전용으로 유지됩니다.
//   출격/귀환은 SafeZone Collider Trigger로 자동 감지합니다.
// =============================================================================
public class SortieManager : MonoBehaviour
{
    // ── 참조 ──────────────────────────────────────────────────────────────
    [Header("References")]
    [Tooltip("Player의 SurvivalTimer — 비워두면 자동 탐색")]
    public SurvivalTimer survivalTimer;

    [Tooltip("ItemSpawner — 비워두면 자동 탐색")]
    public ItemSpawner itemSpawner;

    // ── Cold Escalation ───────────────────────────────────────────────────
    [Header("Cold Escalation")]
    [Tooltip("첫 출격 체온 감소 배율 (1.0 = 기본)")]
    public float initialColdMultiplier = 1f;

    [Tooltip("귀환 1회마다 다음 출격 배율 증가량")]
    public float coldEscalationPerReturn = 0.2f;

    [Tooltip("체온 감소 배율 상한")]
    public float maxColdMultiplier = 3f;

    [Tooltip("첫 번째 출격에는 Cold Escalation 적용 안 함 (첫 번째는 기본 배율 유지)")]
    public bool skipEscalationOnFirstSortie = true;

    // ── UI (선택) ─────────────────────────────────────────────────────────
    [Header("HUD (Optional)")]
    [Tooltip("출격 횟수 + 한파 배율 표시 텍스트 — 비워두면 무시")]
    public TextMeshProUGUI sortieHUDText;

    // ── 내부 상태 ─────────────────────────────────────────────────────────
    private int   _sortieCount  = 0;   // 총 출격 횟수 (SafeZone Exit 기준)
    private int   _returnCount  = 0;   // 총 귀환 횟수 (SafeZone Enter 기준)
    private float _nextColdMultiplier; // 다음 출격에 적용될 배율 (귀환 시 계산)
    private bool  _isOutside    = false;

    // ─────────────────────────────────────────────────────────────────────
    void Start()
    {
        // 자동 참조 탐색
        if (survivalTimer == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null) survivalTimer = player.GetComponent<SurvivalTimer>();
        }
        if (itemSpawner == null)
        {
#if UNITY_2023_1_OR_NEWER
            itemSpawner = Object.FindAnyObjectByType<ItemSpawner>();
#else
            itemSpawner = Object.FindObjectOfType<ItemSpawner>();
#endif
        }

        _nextColdMultiplier = initialColdMultiplier;

        // 시작 시 기본 배율 적용 (야외 상태도 대비)
        ApplyColdMultiplier(initialColdMultiplier);
        UpdateHUD();

        Debug.Log("[SortieManager] 초기화 완료 — SafeZone 트리거 기반 자동 출격 모드");
    }

    // ─────────────────────────────────────────────────────────────────────
    //  SafeZone Trigger 이벤트 수신
    //  SurvivalTimer와 동일한 SafeZone 트리거를 공유합니다.
    //  → Player의 Collider가 SafeZone Collider와 충돌할 때 호출됩니다.
    //
    //  ※ 이 컴포넌트는 Player에 붙어있지 않으므로 직접 트리거를 받지 않습니다.
    //     SurvivalTimer의 OnTrigger를 감지하는 방식으로 폴링합니다.
    // ─────────────────────────────────────────────────────────────────────
    void Update()
    {
        if (survivalTimer == null) return;

        bool currentlyOutside = !survivalTimer.inSafeZone;

        // ── SafeZone Exit 감지 (별장 → 야외) ─────────────────────────────
        if (currentlyOutside && !_isOutside)
        {
            _isOutside = true;
            OnSortieStart();
        }

        // ── SafeZone Enter 감지 (야외 → 별장 귀환) ───────────────────────
        if (!currentlyOutside && _isOutside)
        {
            _isOutside = false;
            OnSortieReturn();
        }
    }

    // ── 출격 시작 (SafeZone Exit) ─────────────────────────────────────────
    void OnSortieStart()
    {
        _sortieCount++;

        // 첫 출격은 기본 배율 유지 (옵션)
        float mult = (skipEscalationOnFirstSortie && _sortieCount == 1)
            ? initialColdMultiplier
            : _nextColdMultiplier;

        ApplyColdMultiplier(mult);
        UpdateHUD();

        Debug.Log($"[SortieManager] 🚀 출격 #{_sortieCount} 시작 — coldMultiplier={mult:F2}x");
    }

    // ── 귀환 (SafeZone Enter) ─────────────────────────────────────────────
    void OnSortieReturn()
    {
        _returnCount++;

        // 아이템 리스폰 (귀환한 이후 다음 출격을 위해)
        if (itemSpawner != null)
        {
            itemSpawner.RespawnAll();
            Debug.Log($"[SortieManager] 🔄 귀환 #{_returnCount} — 아이템 리스폰 완료");
        }

        // 다음 출격용 coldMultiplier 계산 (귀환 횟수 기준으로 누적)
        _nextColdMultiplier = Mathf.Min(
            initialColdMultiplier + coldEscalationPerReturn * _returnCount,
            maxColdMultiplier);

        // 별장 안에서는 기본 배율로 복귀 (회복 중에 체온이 더 빠르게 안 빠지게)
        ApplyColdMultiplier(initialColdMultiplier);
        UpdateHUD();

        Debug.Log($"[SortieManager] 🏠 귀환 완료 — 다음 출격 배율={_nextColdMultiplier:F2}x");
    }

    // ── coldMultiplier → SurvivalTimer 반영 ──────────────────────────────
    void ApplyColdMultiplier(float value)
    {
        if (survivalTimer != null)
            survivalTimer.coldMultiplier = value;
    }

    // ── HUD 업데이트 ──────────────────────────────────────────────────────
    void UpdateHUD()
    {
        if (sortieHUDText == null) return;

        if (_sortieCount == 0)
        {
            sortieHUDText.text = "별장 밖으로 나가 가구를 찾으세요";
        }
        else if (!_isOutside)
        {
            // 별장 안 — 귀환 상태
            sortieHUDText.text = _returnCount > 0
                ? $"귀환 완료  |  다음 한파: {_nextColdMultiplier:F1}x"
                : "";
        }
        else
        {
            // 야외 — 출격 중
            float cur = survivalTimer != null ? survivalTimer.coldMultiplier : 1f;
            sortieHUDText.text = $"출격 #{_sortieCount}  ❄ 한파: {cur:F1}x";
        }
    }
}

using System.Collections;
using UnityEngine;

// =============================================================================
//  EndingManager — 2막 "새벽이 온다" 시스템
//
//  ■ 1막 → 2막 전환 조건: 코트 Lv.2 이상 + 안락도 60% 이상
//  ■ 트리거 시: 라디오 교신 자막 연출 → 통신 중계기 맵 끝에 스폰
//  ■ 마지막 출격: coldMultiplier 최대, 블리자드 극한
//  ■ 중계기 도달 + E키 상호작용 → 엔딩 연출 시작
// =============================================================================
public class EndingManager : MonoBehaviour
{
    // ── 참조 (자동 탐색) ─────────────────────────────────────────────────
    [Header("References (Auto-Find)")]
    public SurvivalTimer survivalTimer;
    public CabinComfort cabinComfort;
    public SortieManager sortieManager;
    public FogManager fogManager;
    public AtmosphereManager atmosphereManager;
    public SubtitleManager subtitleManager;

    // ── 2막 트리거 조건 ──────────────────────────────────────────────────
    [Header("Act 2 Trigger")]
    [Tooltip("필요한 코트 업그레이드 레벨")]
    public int requiredCoatLevel = 2;
    [Tooltip("필요한 안락도 비율 (0~1)")]
    public float requiredComfortRatio = 0.6f;

    // ── 중계기 설정 ──────────────────────────────────────────────────────
    [Header("Beacon Settings")]
    [Tooltip("중계기 프리팹. 비어있으면 런타임에 자동 생성.")]
    public GameObject beaconPrefab;
    [Tooltip("중계기 꼭대기 점멸등 색상")]
    public Color beaconBlinkColor = Color.red;
    [Tooltip("중계기 상호작용 시 은은한 주황빛")]
    public Color beaconWarmGlow = new Color(1f, 0.6f, 0.2f, 1f);

    // ── 마지막 출격 설정 ─────────────────────────────────────────────────
    [Header("Final Sortie")]
    public float finalColdMultiplier = 3f;
    public float finalBlizzardInterval = 15f;

    // ── 내부 상태 ────────────────────────────────────────────────────────
    private bool _act2Triggered = false;
    private bool _beaconSpawned = false;
    private bool _endingStarted = false;
    private bool _finalSortieActive = false;
    private GameObject _beaconInstance;

    public bool IsAct2 => _act2Triggered;
    public bool IsFinalSortie => _finalSortieActive;

    // ─────────────────────────────────────────────────────────────────────
    void Start()
    {
        // 자동 참조 탐색
        if (survivalTimer == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null) survivalTimer = player.GetComponent<SurvivalTimer>();
        }
        if (cabinComfort == null)
            cabinComfort = Object.FindFirstObjectByType<CabinComfort>();
        if (sortieManager == null)
            sortieManager = Object.FindFirstObjectByType<SortieManager>();
        if (fogManager == null)
            fogManager = Object.FindFirstObjectByType<FogManager>();
        if (atmosphereManager == null)
            atmosphereManager = Object.FindFirstObjectByType<AtmosphereManager>();

        // 안락도 변화 이벤트 구독
        if (cabinComfort != null)
            cabinComfort.OnComfortChanged += OnComfortChanged;
    }

    void OnDestroy()
    {
        if (cabinComfort != null)
            cabinComfort.OnComfortChanged -= OnComfortChanged;
    }

    // ── 안락도 변화 콜백 ─────────────────────────────────────────────────
    void OnComfortChanged(float comfortRatio)
    {
        if (_act2Triggered) return;
        CheckAct2Conditions();
    }

    // ── 코트 업그레이드 시에도 체크 (SurvivalTimer에서 호출) ──────────────
    public void OnCoatUpgraded(int newLevel)
    {
        if (_act2Triggered) return;
        CheckAct2Conditions();
    }

    // ── 2막 조건 확인 ────────────────────────────────────────────────────
    void CheckAct2Conditions()
    {
        if (survivalTimer == null || cabinComfort == null) return;
        if (!survivalTimer.inSafeZone) return; // 별장 안에서만 트리거

        bool coatOk = survivalTimer.upgradeLevel >= requiredCoatLevel;
        bool comfortOk = cabinComfort.ComfortRatio >= requiredComfortRatio;

        if (coatOk && comfortOk)
        {
            _act2Triggered = true;
            StartCoroutine(Act2TransitionSequence());
        }
    }

    // =====================================================================
    //  2막 전환 연출 코루틴
    // =====================================================================
    IEnumerator Act2TransitionSequence()
    {
        Debug.Log("[EndingManager] ★ 2막 전환 트리거!");

        var sub = SubtitleManager.Instance;

        // 1단계: 라디오 잡음
        yield return new WaitForSeconds(2f);
        if (sub != null)
            sub.ShowSubtitle("...라디오에서 잡음이 들려온다...", 3f, true);

        yield return new WaitForSeconds(4f);

        // 2단계: 구조대 교신
        if (sub != null)
            sub.ShowSubtitle("\"여기는 구조대... 좌표를 보내라. 반복한다, 좌표를...\"", 5f, true);

        yield return new WaitForSeconds(6f);

        // 3단계: 중계기 힌트
        if (sub != null)
            sub.ShowSubtitle("...언덕 위 통신 중계기에 가면 좌표를 보낼 수 있을지도 모른다.", 5f);

        yield return new WaitForSeconds(3f);

        // 4단계: 중계기 스폰
        SpawnBeacon();

        if (sub != null)
            sub.ShowSubtitle("멀리 깜빡이는 빨간 불빛이 보인다...", 4f);

        Debug.Log("[EndingManager] ★ 중계기 스폰 완료. 마지막 출격 대기 중.");
    }

    // =====================================================================
    //  중계기 스폰
    // =====================================================================
    void SpawnBeacon()
    {
        if (_beaconSpawned) return;
        _beaconSpawned = true;

        // 맵에서 가장 먼 지점 계산 (별장 = SafeZone 기준)
        Vector3 safePos = Vector3.zero;
        var safeZone = GameObject.FindWithTag("SafeZone");
        if (safeZone != null) safePos = safeZone.transform.position;

        // ItemSpawner의 spawnRadius를 참조하여 맵 끝 지점 계산
        float spawnDistance = 80f; // 기본값
        var itemSpawner = Object.FindFirstObjectByType<ItemSpawner>();
        if (itemSpawner != null)
        {
            // ItemSpawner의 maxSpawnDistance 필드를 참조
            var maxField = itemSpawner.GetType().GetField("maxSpawnDistance");
            if (maxField != null)
                spawnDistance = (float)maxField.GetValue(itemSpawner) * 0.9f;
        }

        // 별장에서 가장 먼 방향 (랜덤 각도)
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 beaconPos = safePos + new Vector3(
            Mathf.Cos(angle) * spawnDistance,
            0f,
            Mathf.Sin(angle) * spawnDistance
        );

        // 지형 높이에 맞추기
        if (Physics.Raycast(beaconPos + Vector3.up * 100f, Vector3.down, out RaycastHit hit, 200f))
            beaconPos.y = hit.point.y;

        // 중계기 생성
        if (beaconPrefab != null)
        {
            _beaconInstance = Instantiate(beaconPrefab, beaconPos, Quaternion.identity);
        }
        else
        {
            _beaconInstance = CreateBeaconPrimitive(beaconPos);
        }

        _beaconInstance.name = "RadioBeacon";
        _beaconInstance.tag = "Untagged";

        Debug.Log($"[EndingManager] 중계기 스폰 위치: {beaconPos}");
    }

    // ── 프리미티브 중계기 자동 생성 (프리팹 없을 때) ─────────────────────
    GameObject CreateBeaconPrimitive(Vector3 position)
    {
        var root = new GameObject("RadioBeacon");
        root.transform.position = position;

        // 기둥 (가느다란 실린더, 약간 기울어짐)
        var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.name = "Pole";
        pole.transform.SetParent(root.transform, false);
        pole.transform.localScale = new Vector3(0.15f, 2.5f, 0.15f);
        pole.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        pole.transform.localRotation = Quaternion.Euler(0f, 0f, 5f); // 약간 기울어짐

        var poleRenderer = pole.GetComponent<Renderer>();
        if (poleRenderer != null)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = new Color(0.35f, 0.35f, 0.4f); // 어두운 금속색
            poleRenderer.material = mat;
        }

        // 꼭대기 빨간 점멸등
        var lightObj = new GameObject("BeaconLight");
        lightObj.transform.SetParent(root.transform, false);
        lightObj.transform.localPosition = new Vector3(0f, 5.2f, 0f);

        var beaconLight = lightObj.AddComponent<Light>();
        beaconLight.type = LightType.Point;
        beaconLight.color = beaconBlinkColor;
        beaconLight.intensity = 8f;
        beaconLight.range = 50f;

        // 점멸 효과 스크립트
        var blinker = lightObj.AddComponent<BeaconBlinker>();
        blinker.blinkSpeed = 1.5f;

        // 꼭대기 빨간 구체 (시각적 표시)
        var bulb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bulb.name = "Bulb";
        bulb.transform.SetParent(root.transform, false);
        bulb.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        bulb.transform.localPosition = new Vector3(0f, 5.2f, 0f);

        // Collider 제거 (상호작용은 제어판에서)
        var bulbCol = bulb.GetComponent<Collider>();
        if (bulbCol != null) Object.Destroy(bulbCol);

        var bulbRenderer = bulb.GetComponent<Renderer>();
        if (bulbRenderer != null)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = beaconBlinkColor;
            mat.SetColor("_EmissionColor", beaconBlinkColor * 3f);
            mat.EnableKeyword("_EMISSION");
            bulbRenderer.material = mat;
        }

        // 제어판 박스 (E키 상호작용 대상)
        var panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        panel.name = "ControlPanel";
        panel.tag = "Interactable";
        panel.transform.SetParent(root.transform, false);
        panel.transform.localScale = new Vector3(0.5f, 0.8f, 0.3f);
        panel.transform.localPosition = new Vector3(0.4f, 0.4f, 0f);

        var panelRenderer = panel.GetComponent<Renderer>();
        if (panelRenderer != null)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = new Color(0.25f, 0.3f, 0.25f); // 군용 녹색
            panelRenderer.material = mat;
        }

        // 제어판에 BeaconInteraction 컴포넌트 추가
        var interaction = panel.AddComponent<BeaconInteraction>();
        interaction.endingManager = this;
        interaction.warmGlowColor = beaconWarmGlow;

        // 루트에 큰 트리거 콜라이더 (접근 감지용)
        var trigger = root.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 5f;

        return root;
    }

    // =====================================================================
    //  마지막 출격 세팅 (SortieManager에서 호출)
    // =====================================================================
    public void ActivateFinalSortie()
    {
        _finalSortieActive = true;

        // 극한 환경 강제 적용
        if (survivalTimer != null)
            survivalTimer.coldMultiplier = finalColdMultiplier;

        // 블리자드 간격 극단적으로 짧게
        if (fogManager != null)
        {
            fogManager.waveIntervalMin = finalBlizzardInterval;
            fogManager.waveIntervalMax = finalBlizzardInterval + 5f;
        }

        Debug.Log("[EndingManager] ★ 마지막 출격 활성화! 극한 환경 적용.");
    }

    // =====================================================================
    //  엔딩 연출 시작 (BeaconInteraction에서 E키 상호작용 후 호출)
    // =====================================================================
    public void StartEndingSequence()
    {
        if (_endingStarted) return;
        _endingStarted = true;
        StartCoroutine(EndingSequence());
    }

    IEnumerator EndingSequence()
    {
        Debug.Log("[EndingManager] ★ 엔딩 연출 시작!");

        var sub = SubtitleManager.Instance;
        var player = GameObject.FindWithTag("Player");

        // 1. 조작 마비
        if (player != null)
        {
            var pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.enabled = false;
        }

        // 2. 신호 발사 자막
        if (sub != null)
            sub.ShowSubtitle("...신호가 발사됐다...제발 빨리...", 4f, true);

        yield return new WaitForSeconds(5f);

        // 3. 블리자드 서서히 멎음
        if (fogManager != null)
        {
            float elapsed = 0f;
            float fadeDuration = 8f;
            float startDensity = RenderSettings.fogDensity;
            Color startColor = RenderSettings.fogColor;
            Color clearColor = new Color(0.6f, 0.65f, 0.75f);

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                RenderSettings.fogDensity = Mathf.Lerp(startDensity, 0.001f, t);
                RenderSettings.fogColor = Color.Lerp(startColor, clearColor, t);
                yield return null;
            }
        }

        // 4. 새벽 색감 전환 (파란색 → 주황색)
        if (sub != null)
            sub.ShowSubtitle("...멀리서 헬기 소리가 들려온다...", 5f, true);

        if (atmosphereManager != null && atmosphereManager.environmentLight != null)
        {
            float elapsed = 0f;
            float dawnDuration = 6f;
            Light envLight = atmosphereManager.environmentLight;
            Color startColor = envLight.color;
            float startIntensity = envLight.intensity;
            Color dawnColor = new Color(1f, 0.65f, 0.3f); // 따뜻한 주황색 새벽

            while (elapsed < dawnDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dawnDuration;
                envLight.color = Color.Lerp(startColor, dawnColor, t);
                envLight.intensity = Mathf.Lerp(startIntensity, 1.5f, t);
                yield return null;
            }
        }

        yield return new WaitForSeconds(3f);

        // 5. 카메라 기울임 (쓰러지는 연출)
        Camera cam = Camera.main;
        if (cam != null)
        {
            float elapsed = 0f;
            float fallDuration = 4f;
            Quaternion startRot = cam.transform.rotation;
            // 천천히 아래+옆으로 기울어짐
            Quaternion endRot = startRot * Quaternion.Euler(60f, 15f, 30f);

            while (elapsed < fallDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fallDuration;
                // 이징: 처음엔 천천히 → 나중에 빨라짐
                float eased = t * t;
                cam.transform.rotation = Quaternion.Slerp(startRot, endRot, eased);
                yield return null;
            }
        }

        // 6. 하얀 암전 (빛 속으로 의식을 잃는 느낌)
        yield return StartCoroutine(FadeToWhite(3f));

        yield return new WaitForSeconds(2f);

        // 7. PROLOGUE END 표시
        if (sub != null)
            sub.ShowSubtitle("PROLOGUE END", 999f, true);

        Debug.Log("[EndingManager] ★ 엔딩 완료.");
    }

    IEnumerator FadeToWhite(float duration)
    {
        // GameDirector의 fadeOverlay를 재활용하거나, 없으면 새로 생성
        var gd = Object.FindFirstObjectByType<GameDirector>();
        UnityEngine.UI.Image overlay = null;

        if (gd != null && gd.fadeOverlay != null)
        {
            overlay = gd.fadeOverlay;
        }

        if (overlay != null)
        {
            overlay.gameObject.SetActive(true);
            overlay.raycastTarget = false;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                overlay.color = new Color(1f, 1f, 1f, t); // 하얀색 페이드
                yield return null;
            }
            overlay.color = Color.white;
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SurvivalTimer : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI inventoryText;
    public TextMeshProUGUI gameOverTimeText;

    [Header("Survival Settings")]
    public float maxTime = 30f;
    private float currentTime;
    public float timeRemaining => currentTime;
    public float TimeRatio { get { return maxTime > 0f ? currentTime / maxTime : 0f; } }
    private bool isDead = false;
    public bool inSafeZone = true;

    [Header("Cold Escalation")]
    [Tooltip("체온 감소 속도 배율 — SortieManager가 출격마다 값을 올립니다")]
    public float coldMultiplier = 1f;

    [Header("Safe Zone Recovery")]
    public float recoverySpeed = 8f;

    [Header("Materials & Upgrade")]
    public int materialCount = 0;
    public int upgradeLevel = 0;
    public int maxUpgradeLevel = 3;
    public int materialsPerUpgrade = 3;
    public float timePerUpgrade = 15f;

    [Header("Last Chance (2막 전용)")]
    [Tooltip("라스트 찬스 지속 시간 (초)")]
    public float lastChanceDuration = 15f;
    [Tooltip("터널 비전 오버레이 (비네트). 비어있으면 자동 생성.")]
    public Image vignetteOverlay;

    // 라스트 찬스 상태
    private bool _lastChanceActive = false;
    private float _lastChanceTimer;
    public bool IsLastChance => _lastChanceActive;

    private float _sessionStartTime;
    private bool _warned50;
    private bool _warned25;

    void Start()
    {
        _sessionStartTime = Time.time;
        currentTime = maxTime;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        UpdateInventoryUI();
    }

    void Update()
    {
        if (isDead) return;

        if (!inSafeZone)
        {
            currentTime -= Time.deltaTime * coldMultiplier;
            if (timerText != null)
            {
                timerText.text = "체온: " + currentTime.ToString("F1") + "초";
                timerText.color = currentTime < maxTime * 0.3f ? Color.red : Color.white;
            }

            // 체온 경고 자막
            float ratio = currentTime / maxTime;
            if (ratio <= 0.5f && !_warned50)
            {
                _warned50 = true;
                if (SubtitleManager.Instance != null)
                    SubtitleManager.Instance.ShowSubtitle("몸이 점점 굳어간다... 빨리 돌아가야 해.", 3f);
            }
            if (ratio <= 0.25f && !_warned25)
            {
                _warned25 = true;
                if (SubtitleManager.Instance != null)
                    SubtitleManager.Instance.ShowSubtitle("더 이상은 위험해...! 지금 당장 별장으로!", 3f, true);
            }

            if (currentTime <= 0)
            {
                currentTime = 0;

                // 2막 마지막 출격 중이면 라스트 찬스 모드
                var ending = Object.FindFirstObjectByType<EndingManager>();
                if (ending != null && ending.IsFinalSortie && !_lastChanceActive)
                {
                    EnterLastChance();
                }
                else if (!_lastChanceActive)
                {
                    GameOver();
                }
            }

            // 라스트 찬스 카운트다운
            if (_lastChanceActive)
            {
                _lastChanceTimer -= Time.deltaTime;
                UpdateVignette();

                if (_lastChanceTimer <= 0f)
                {
                    _lastChanceActive = false;
                    GameOver();
                }
            }
        }
        else
        {
            currentTime = Mathf.MoveTowards(currentTime, maxTime, recoverySpeed * Time.deltaTime);
            if (timerText != null)
            {
                if (currentTime >= maxTime)
                {
                    timerText.text = "별장";
                    timerText.color = Color.green;
                }
                else
                {
                    timerText.text = string.Format("체온 회복중... {0:F0}초", currentTime);
                    timerText.color = Color.yellow;
                }
            }
        }
    }

    public void AddTime(float bonusTime)
    {
        currentTime += bonusTime;
        if (currentTime > maxTime) currentTime = maxTime;
    }

    public void AddMaterial(int amount)
    {
        materialCount += amount;
        UpdateInventoryUI();
    }

    public void UpgradeCoat()
    {
        if (upgradeLevel >= maxUpgradeLevel)
        {
            Debug.Log("Max upgrade reached!");
            return;
        }

        if (materialCount >= materialsPerUpgrade)
        {
            materialCount -= materialsPerUpgrade;
            upgradeLevel++;
            maxTime += timePerUpgrade;
            currentTime = maxTime;
            UpdateInventoryUI();
            Debug.Log(string.Format("Upgraded Lv.{0}! Survival time: {1}s", upgradeLevel, maxTime));

            // EndingManager에 코트 레벨 변화 알림 (2막 트리거 체크)
            var ending = Object.FindFirstObjectByType<EndingManager>();
            if (ending != null) ending.OnCoatUpgraded(upgradeLevel);
        }
        else
        {
            Debug.Log(string.Format("Not enough materials! ({0}/{1})", materialCount, materialsPerUpgrade));
        }
    }

    void UpdateInventoryUI()
    {
        if (inventoryText != null)
        {
            string upgradeInfo = upgradeLevel < maxUpgradeLevel
                ? string.Format("코트 재료: {0} / {1}", materialCount, materialsPerUpgrade)
                : string.Format("코트 재료: {0} [MAX]", materialCount);
            inventoryText.text = upgradeInfo;
        }
    }

    void GameOver()
    {
        isDead = true;

        // 카메라 회전 + 이동 완전 차단
        var pc = GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;

        // 마우스 커서 표시 (UI 버튼 클릭 가능)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        float elapsed = Time.time - _sessionStartTime;
        int mins = Mathf.FloorToInt(elapsed / 60f);
        int secs = Mathf.FloorToInt(elapsed % 60f);

        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (gameOverTimeText != null)
            gameOverTimeText.text = string.Format("Survived: {0:00}:{1:00}", mins, secs);

        Time.timeScale = 0f;
    }

    // =====================================================================
    //  라스트 찬스 시스템
    // =====================================================================
    void EnterLastChance()
    {
        _lastChanceActive = true;
        _lastChanceTimer = lastChanceDuration;

        // 달리기 잠금
        var pc = GetComponent<PlayerController>();
        if (pc != null) pc.sprintLocked = true;

        // 비네트 오버레이 자동 생성 (없으면)
        EnsureVignetteOverlay();

        // 경고 자막
        if (SubtitleManager.Instance != null)
            SubtitleManager.Instance.ShowSubtitle("의식이 흐려진다... 몸이 말을 듣지 않아...", 4f, true);

        Debug.Log("[SurvivalTimer] ★ 라스트 찬스 모드 진입!");
    }

    void EnsureVignetteOverlay()
    {
        if (vignetteOverlay != null) return;

        // Canvas 찾기
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        var go = new GameObject("VignetteOverlay");
        go.transform.SetParent(canvas.transform, false);

        var img = go.AddComponent<Image>();
        img.raycastTarget = false;

        // 비네트 텍스처 생성 (가장자리는 검정, 중앙은 투명)
        int size = 512;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size * 0.5f;
        float maxRadius = center;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float t = Mathf.Clamp01(dist / maxRadius);
                // 비네트 커브: 중앙은 완전 투명, 가장자리로 갈수록 검정
                float alpha = Mathf.Pow(t, 1.5f);
                tex.SetPixel(x, y, new Color(0f, 0f, 0f, alpha));
            }
        }
        tex.Apply();

        img.sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        img.color = new Color(1f, 1f, 1f, 0f); // 처음엔 투명

        // 전체 화면 덮기
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        vignetteOverlay = img;
    }

    void UpdateVignette()
    {
        if (vignetteOverlay == null) return;

        // 시간이 지날수록 터널 비전 강해짐 (알파 0 → 1)
        float progress = 1f - (_lastChanceTimer / lastChanceDuration);
        float alpha = Mathf.Lerp(0.3f, 1f, progress); // 처음부터 약간 보이고, 점점 강해짐
        vignetteOverlay.color = new Color(1f, 1f, 1f, alpha);
    }

    public void ResetState()
    {
        isDead = false;
        inSafeZone = true;
        materialCount = 0;
        upgradeLevel = 0;
        maxTime = 30f;
        currentTime = maxTime;
        coldMultiplier = 1f;
        _warned50 = false;
        _warned25 = false;
        _lastChanceActive = false;
        _sessionStartTime = Time.time;
        UpdateInventoryUI();

        // 달리기 잠금 해제
        var pc = GetComponent<PlayerController>();
        if (pc != null) pc.sprintLocked = false;

        // 비네트 제거
        if (vignetteOverlay != null)
        {
            vignetteOverlay.color = new Color(1f, 1f, 1f, 0f);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SafeZone")) inSafeZone = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SafeZone")) inSafeZone = false;
    }
}
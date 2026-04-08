using UnityEngine;
using TMPro;

public class GameProgress : MonoBehaviour
{
    [Header("References")]
    public SurvivalTimer survivalTimer;
    public CabinComfort cabinComfort;

    [Header("Win Conditions")]
    public int requiredUpgradeLevel = 2;       // 코트 Lv.2 이상
    public float requiredComfortRatio = 0.6f;  // 가구 60% 이상 배치

    [Header("Win Screen")]
    public GameObject winPanel;
    public TextMeshProUGUI winTitleText;
    public TextMeshProUGUI winStatsText;

    [Header("Radio Hint UI")]
    public TextMeshProUGUI radioHintText;

    private bool _isWon = false;
    private float _startTime;

    void Start()
    {
        _startTime = Time.time;
        if (winPanel != null) winPanel.SetActive(false);
        if (radioHintText != null) radioHintText.text = "";
    }

    void Update()
    {
        if (_isWon) return;
        UpdateRadioHint();
    }

    void UpdateRadioHint()
    {
        if (radioHintText == null || survivalTimer == null || cabinComfort == null) return;

        bool coatOk = survivalTimer.upgradeLevel >= requiredUpgradeLevel;
        bool comfortOk = cabinComfort.ComfortRatio >= requiredComfortRatio;

        if (coatOk && comfortOk)
        {
            radioHintText.text = "<color=#FFD700>★ 무전기로 구조 요청 가능!</color>";
        }
        else
        {
            string coat = coatOk
                ? "<color=#90EE90>✔ 코트 업그레이드</color>"
                : string.Format("<color=#FF6B6B>✘ 코트 Lv.{0} 필요</color>", requiredUpgradeLevel);

            int placed = cabinComfort.PlacedCount;
            int needed = Mathf.CeilToInt(cabinComfort.maxFurnitureCount * requiredComfortRatio);
            string comfort = comfortOk
                ? "<color=#90EE90>✔ 오두막 정리</color>"
                : string.Format("<color=#FF6B6B>✘ 가구 {0}/{1} 배치</color>", placed, needed);

            radioHintText.text = coat + "  " + comfort;
        }
    }

    // Interaction.cs에서 Radio(ExitPoint) 상호작용 시 호출
    public bool TryWin()
    {
        if (survivalTimer == null || cabinComfort == null) return false;

        bool coatOk = survivalTimer.upgradeLevel >= requiredUpgradeLevel;
        bool comfortOk = cabinComfort.ComfortRatio >= requiredComfortRatio;

        if (coatOk && comfortOk)
        {
            TriggerWin();
            return true;
        }
        else
        {
            // 조건 미달 피드백 (RadioHint가 알아서 표시)
            Debug.Log("조건 미달: 코트=" + coatOk + " 가구=" + comfortOk);
            return false;
        }
    }

    void TriggerWin()
    {
        _isWon = true;
        Time.timeScale = 0f;

        if (winPanel != null) winPanel.SetActive(true);

        float elapsed = Time.time - _startTime;
        int mins = Mathf.FloorToInt(elapsed / 60f);
        int secs = Mathf.FloorToInt(elapsed % 60f);

        if (winTitleText != null)
            winTitleText.text = "밤을 버텼다...";

        if (winStatsText != null)
        {
            winStatsText.text = string.Format(
                "생존 시간: {0:00}:{1:00}\n코트 업그레이드: Lv.{2}\n배치한 가구: {3}개\n",
                mins, secs,
                survivalTimer != null ? survivalTimer.upgradeLevel : 0,
                cabinComfort != null ? cabinComfort.PlacedCount : 0
            );
        }
    }

    // R키 재시작
    void LateUpdate()
    {
        if (_isWon && Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }
}

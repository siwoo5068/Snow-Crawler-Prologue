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
            radioHintText.text = "<color=#FFD700>★ Radio rescue available!</color>";
        }
        else
        {
            string coat = coatOk
                ? "<color=#90EE90>✔ Coat upgraded</color>"
                : string.Format("<color=#FF6B6B>✘ Coat Lv.{0} required</color>", requiredUpgradeLevel);

            int placed = cabinComfort.PlacedCount;
            int needed = Mathf.CeilToInt(cabinComfort.maxFurnitureCount * requiredComfortRatio);
            string comfort = comfortOk
                ? "<color=#90EE90>✔ Cabin ready</color>"
                : string.Format("<color=#FF6B6B>✘ Furniture {0}/{1} placed</color>", placed, needed);

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
            Debug.Log("Win conditions not met: coat=" + coatOk + " comfort=" + comfortOk);
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
            winTitleText.text = "You survived the night...";

        if (winStatsText != null)
        {
            winStatsText.text = string.Format(
                "Survival Time: {0:00}:{1:00}\nCoat Level: Lv.{2}\nFurniture Placed: {3}\n",
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

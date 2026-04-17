using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameProgress : MonoBehaviour
{
    [Header("References")]
    public SurvivalTimer survivalTimer;
    public CabinComfort cabinComfort;

    [Header("Win Conditions")]
    public int requiredUpgradeLevel = 2;
    public float requiredComfortRatio = 0.6f;

    [Header("Win Screen")]
    public GameObject winPanel;
    public TextMeshProUGUI winTitleText;
    public TextMeshProUGUI winStatsText;

    private bool _isWon = false;
    private float _startTime;

    void Start()
    {
        _startTime = Time.time;
        if (winPanel != null) winPanel.SetActive(false);
    }


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
                "Survival Time: {0:00}:{1:00}\nCoat Level: Lv.{2}\nFurniture Placed: {3}",
                mins, secs,
                survivalTimer != null ? survivalTimer.upgradeLevel : 0,
                cabinComfort != null ? cabinComfort.PlacedCount : 0
            );
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
        SceneManager.LoadScene(0);
    }
}

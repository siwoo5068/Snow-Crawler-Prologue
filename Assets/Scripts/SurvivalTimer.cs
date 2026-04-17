using UnityEngine;
using UnityEngine.SceneManagement;
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
    public float TimeRatio { get { return maxTime > 0f ? currentTime / maxTime : 0f; } }
    private bool isDead = false;
    public bool inSafeZone = true;

    [Header("Safe Zone Recovery")]
    public float recoverySpeed = 8f;

    [Header("Materials & Upgrade")]
    public int materialCount = 0;
    public int upgradeLevel = 0;
    public int maxUpgradeLevel = 3;
    public int materialsPerUpgrade = 3;
    public float timePerUpgrade = 15f;

    private float _sessionStartTime;

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
            currentTime -= Time.deltaTime;
            if (timerText != null)
            {
                timerText.text = "Time: " + currentTime.ToString("F1") + "s";
                timerText.color = currentTime < maxTime * 0.3f ? Color.red : Color.white;
            }

            if (currentTime <= 0)
            {
                currentTime = 0;
                GameOver();
            }
        }
        else
        {
            currentTime = Mathf.MoveTowards(currentTime, maxTime, recoverySpeed * Time.deltaTime);
            if (timerText != null)
            {
                if (currentTime >= maxTime)
                {
                    timerText.text = "Safe";
                    timerText.color = Color.green;
                }
                else
                {
                    timerText.text = string.Format("Warming... {0:F0}s", currentTime);
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
                ? string.Format("Material: {0} / {1}", materialCount, materialsPerUpgrade)
                : string.Format("Material: {0} [MAX]", materialCount);
            inventoryText.text = upgradeInfo;
        }
    }

    void GameOver()
    {
        isDead = true;

        float elapsed = Time.time - _sessionStartTime;
        int mins = Mathf.FloorToInt(elapsed / 60f);
        int secs = Mathf.FloorToInt(elapsed % 60f);

        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (gameOverTimeText != null)
            gameOverTimeText.text = string.Format("Survived: {0:00}:{1:00}", mins, secs);

        Time.timeScale = 0f;
    }

    public void ResetState()
    {
        isDead = false;
        inSafeZone = true;
        materialCount = 0;
        upgradeLevel = 0;
        maxTime = 30f;
        currentTime = maxTime;
        _sessionStartTime = Time.time;
        UpdateInventoryUI();
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SafeZone")) inSafeZone = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SafeZone")) inSafeZone = false;
    }
}
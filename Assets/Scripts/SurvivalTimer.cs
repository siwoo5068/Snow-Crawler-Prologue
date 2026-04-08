using UnityEngine;
using TMPro;

public class SurvivalTimer : MonoBehaviour
{
    [Header("UI 연결")]
    public TextMeshProUGUI timerText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI inventoryText;

    [Header("생존 설정")]
    public float maxTime = 10f;
    private float currentTime;
    public float TimeRatio { get { return maxTime > 0f ? currentTime / maxTime : 0f; } }
    private bool isDead = false;
    public bool inSafeZone = true;

    [Header("안전지대 회복")]
    public float recoverySpeed = 5f;   // 초당 회복 속도 (5초 = 5초/s)

    [Header("재료 및 업그레이드")]
    public int materialCount = 0;
    public int upgradeLevel = 0;
    public int maxUpgradeLevel = 3;
    public int materialsPerUpgrade = 3;
    public float timePerUpgrade = 10f;

    void Start()
    {
        currentTime = maxTime;

        // 모든 패널 초기화
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        UpdateInventoryUI();
    }

    void Update()
    {
        // 죽었으면 아무것도 안 함
        if (isDead) return;

        if (!inSafeZone)
        {
            // 안전지대 밖: 시간 깎임
            currentTime -= Time.deltaTime;
            if (timerText != null)
            {
                timerText.text = "Time: " + currentTime.ToString("F2") + "s";
                timerText.color = Color.white;
            }

            if (currentTime <= 0)
            {
                currentTime = 0;
                GameOver();
            }
        }
        else
        {
            // 안전지대 안: 점진적 회복
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


    // 아이템 먹었을 때 시간 추가
    public void AddTime(float bonusTime)
    {
        currentTime += bonusTime;
        if (currentTime > maxTime) currentTime = maxTime;
    }

    // 재료(SupplyBox) 수집
    public void AddMaterial(int amount)
    {
        materialCount += amount;
        UpdateInventoryUI();
    }

    // 작업대에서 호출: 재료를 소모하여 생존 시간 업그레이드
    public void UpgradeCoat()
    {
        if (upgradeLevel >= maxUpgradeLevel)
        {
            Debug.Log("최대 업그레이드 달성!");
            return;
        }

        if (materialCount >= materialsPerUpgrade)
        {
            materialCount -= materialsPerUpgrade;
            upgradeLevel++;
            maxTime += timePerUpgrade;
            currentTime = maxTime;
            UpdateInventoryUI();
            Debug.Log(string.Format("업그레이드 Lv.{0}! 생존시간 {1}초", upgradeLevel, maxTime));
        }
        else
        {
            Debug.Log(string.Format("재료 부족! ({0}/{1})", materialCount, materialsPerUpgrade));
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
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    // R키 재시작 (GameOver 상태에서)
    void LateUpdate()
    {
        if (isDead && Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }

    // 트리거 감지 (이게 있어야 나갔다 들어올 때 작동해요!)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SafeZone")) inSafeZone = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SafeZone")) inSafeZone = false;
    }
}
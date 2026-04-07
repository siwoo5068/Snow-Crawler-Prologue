using UnityEngine;
using TMPro;

public class SurvivalTimer : MonoBehaviour
{
    [Header("UI 연결 (Canvas에서 드래그)")]
    public TextMeshProUGUI timerText;
    public GameObject gameOverPanel;
    public GameObject winPanel;      // 🌟 승리 화면 패널
    public TextMeshProUGUI inventoryText;

    [Header("생존 설정")]
    public float maxTime = 10f;
    private float currentTime;
    public float TimeRatio { get { return maxTime > 0f ? currentTime / maxTime : 0f; } }
    private bool isDead = false;
    private bool isWin = false;
    public bool inSafeZone = true;   // 🌟 겹쳐놓으셨으니 시작을 true로!

    [Header("재료 및 업그레이드")]
    public int materialCount = 0;
    public bool isUpgraded = false;

    void Start()
    {
        currentTime = maxTime;

        // 모든 패널 초기화
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);

        UpdateInventoryUI();
    }

    void Update()
    {
        // 죽었거나 이미 이겼으면 아무것도 안 함
        if (isDead || isWin) return;

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
            // 안전지대 안: 시간 풀충전
            currentTime = maxTime;
            if (timerText != null)
            {
                timerText.text = "Safe Zone";
                timerText.color = Color.green;
            }
        }
    }

    // 아이템 먹었을 때 시간 추가
    public void AddTime(float bonusTime)
    {
        currentTime += bonusTime;
        if (currentTime > maxTime) currentTime = maxTime;
    }

    // 재료 추가
    public void AddMaterial(int amount)
    {
        materialCount += amount;
        UpdateInventoryUI();
    }

    // 가방 UI 업데이트
    void UpdateInventoryUI()
    {
        if (inventoryText != null)
        {
            inventoryText.text = "Material : " + materialCount + " / 5";
        }
    }

    // 🌟 작업대에서 호출할 코트 제작 기능
    public void UpgradeCoat()
    {
        if (isUpgraded) return;

        if (materialCount >= 5)
        {
            materialCount -= 5;
            maxTime = 30f;
            currentTime = maxTime;
            isUpgraded = true;
            UpdateInventoryUI();
            Debug.Log("코트 제작 완료! 30초 생존 가능!");
        }
        else
        {
            Debug.Log("재료 부족! (현재: " + materialCount + "/5)");
        }
    }

    // 🌟 무전기에서 호출할 승리 기능
    public void WinGame()
    {
        if (isDead) return;
        isWin = true;
        if (winPanel != null) winPanel.SetActive(true);
        Time.timeScale = 0f; // 게임 멈춤
        Debug.Log("탈출 성공!");
    }

    void GameOver()
    {
        isDead = true;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
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
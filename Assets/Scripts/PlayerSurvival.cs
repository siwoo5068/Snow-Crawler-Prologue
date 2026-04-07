using UnityEngine;
using TMPro;

public class PlayerSurvival : MonoBehaviour
{
    [Header("Survival Settings")]
    public float maxTime = 10f; // 최대 버틸 수 있는 시간
    private float currentTime;
    private bool isOutside = false;

    [Header("UI Reference")]
    public TextMeshProUGUI timerText; // 화면에 띄울 글씨

    void Start()
    {
        currentTime = maxTime;
        UpdateUIText();
    }

    void Update()
    {
        // 밖에 나가면 시간이 줄어들기 시작해요!
        if (isOutside)
        {
            currentTime -= Time.deltaTime;

            // 시간이 0 이하로 떨어지면?
            if (currentTime <= 0)
            {
                currentTime = 0;
                Debug.Log("Time's up! You are frozen..."); // 영어로 변경
            }

            UpdateUIText();
        }
    }

    // 투명 상자(오두막)에서 '나갔을 때'
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SafeZone"))
        {
            isOutside = true;
            Debug.Log("Leaving safe zone! Countdown started!"); // 영어로 변경
        }
    }

    // 투명 상자(오두막) 안으로 '들어왔을 때'
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SafeZone"))
        {
            isOutside = false;
            currentTime = maxTime; // 시간 꽉 채워주기!
            UpdateUIText();
            Debug.Log("Returned to safe zone. Safe!"); // 영어로 변경
        }
    }

    // 화면에 남은 시간 글씨를 업데이트해 주는 기능
    void UpdateUIText()
    {
        if (timerText != null)
        {
            // 한글 폰트 경고가 뜨지 않도록 영어로 변경했어요!
            timerText.text = "Time: " + currentTime.ToString("F2") + "s";

            // 3초 이하로 남으면 글씨를 빨간색으로 경고!
            if (currentTime <= 3f)
            {
                timerText.color = Color.red;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
    }
}
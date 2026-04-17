using UnityEngine;
using TMPro;

public class PlayerSurvival : MonoBehaviour
{
    [Header("Survival Settings")]
    public float maxTime = 10f;
    private float currentTime;
    private bool isOutside = false;

    [Header("UI Reference")]
    public TextMeshProUGUI timerText;

    void Start()
    {
        currentTime = maxTime;
        UpdateUIText();
    }

    void Update()
    {
        if (isOutside)
        {
            currentTime -= Time.deltaTime;

            if (currentTime <= 0)
            {
                currentTime = 0;
                Debug.Log("Time's up! You are frozen...");
            }

            UpdateUIText();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SafeZone"))
        {
            isOutside = true;
            Debug.Log("Leaving safe zone! Countdown started!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SafeZone"))
        {
            isOutside = false;
            currentTime = maxTime;
            UpdateUIText();
            Debug.Log("Returned to safe zone. Safe!");
        }
    }

    void UpdateUIText()
    {
        if (timerText != null)
        {
            timerText.text = "Time: " + currentTime.ToString("F2") + "s";

            if (currentTime <= 3f)
                timerText.color = Color.red;
            else
                timerText.color = Color.white;
        }
    }
}
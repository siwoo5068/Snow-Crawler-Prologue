using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class SubtitleMessage
{
    public string text;
    public float duration;
    public bool urgent;
}

public class SubtitleManager : MonoBehaviour
{
    public static SubtitleManager Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("자막을 띄울 텍스트 컴포넌트")]
    public TextMeshProUGUI subtitleText;
    
    [Header("Settings")]
    public float fadeDuration = 0.8f; // 서서히 나타나고 사라지는 시간

    private Queue<SubtitleMessage> messageQueue = new Queue<SubtitleMessage>();
    private Coroutine currentCoroutine;
    private bool isPlaying = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (subtitleText != null)
        {
            Color c = subtitleText.color;
            c.a = 0f;
            subtitleText.color = c;
            subtitleText.text = "";
        }
    }

    /// <summary>
    /// 영화 같은 자막을 화면에 띄웁니다.
    /// </summary>
    /// <param name="message">출력할 대사</param>
    /// <param name="duration">화면에 떠 있는 시간</param>
    /// <param name="urgent">true일 경우 다른 대사를 무시하고 즉시 덮어씌움</param>
    public void ShowSubtitle(string message, float duration = 3f, bool urgent = false)
    {
        SubtitleMessage msg = new SubtitleMessage { text = message, duration = duration, urgent = urgent };

        if (urgent)
        {
            messageQueue.Clear();
            if (currentCoroutine != null) StopCoroutine(currentCoroutine);
            currentCoroutine = StartCoroutine(PlaySubtitleRoutine(msg));
        }
        else
        {
            messageQueue.Enqueue(msg);
            if (!isPlaying)
            {
                currentCoroutine = StartCoroutine(ProcessQueueRoutine());
            }
        }
    }

    private IEnumerator ProcessQueueRoutine()
    {
        isPlaying = true;

        while (messageQueue.Count > 0)
        {
            SubtitleMessage msg = messageQueue.Dequeue();
            yield return StartCoroutine(PlaySubtitleRoutine(msg));
            yield return new WaitForSeconds(0.3f); // 대사 사이 숨 고르기
        }

        isPlaying = false;
        currentCoroutine = null;
    }

    private IEnumerator PlaySubtitleRoutine(SubtitleMessage msg)
    {
        if (subtitleText == null) yield break;

        subtitleText.text = msg.text;

        // Fade In
        float elapsed = 0f;
        Color c = subtitleText.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            subtitleText.color = c;
            yield return null;
        }
        c.a = 1f;
        subtitleText.color = c;

        // 읽을 시간 주기
        yield return new WaitForSeconds(msg.duration);

        // Fade Out
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            subtitleText.color = c;
            yield return null;
        }
        c.a = 0f;
        subtitleText.color = c;
    }
}

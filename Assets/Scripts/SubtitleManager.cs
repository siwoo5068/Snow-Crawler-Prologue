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

    [Header("UI")]
    public TextMeshProUGUI subtitleText;

    [Header("Settings")]
    public float fadeDuration = 0.8f;

    private readonly Queue<SubtitleMessage> _messageQueue = new Queue<SubtitleMessage>();
    private Coroutine _currentCoroutine;
    private bool _isPlaying;

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

    public void ShowSubtitle(string message, float duration = 3f, bool urgent = false)
    {
        var msg = new SubtitleMessage { text = message, duration = duration, urgent = urgent };

        if (urgent)
        {
            _messageQueue.Clear();
            if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
            _currentCoroutine = StartCoroutine(PlaySubtitleRoutine(msg));
        }
        else
        {
            _messageQueue.Enqueue(msg);
            if (!_isPlaying)
                _currentCoroutine = StartCoroutine(ProcessQueueRoutine());
        }
    }

    private IEnumerator ProcessQueueRoutine()
    {
        _isPlaying = true;
        while (_messageQueue.Count > 0)
        {
            yield return StartCoroutine(PlaySubtitleRoutine(_messageQueue.Dequeue()));
            yield return new WaitForSeconds(0.3f);
        }
        _isPlaying = false;
        _currentCoroutine = null;
    }

    private IEnumerator PlaySubtitleRoutine(SubtitleMessage msg)
    {
        if (subtitleText == null) yield break;

        subtitleText.text = msg.text;
        Color c = subtitleText.color;

        // Fade In
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            subtitleText.color = c;
            yield return null;
        }
        c.a = 1f;
        subtitleText.color = c;

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

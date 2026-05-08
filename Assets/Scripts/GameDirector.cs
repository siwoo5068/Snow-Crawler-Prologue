using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameDirector : MonoBehaviour
{
    public static GameDirector Instance { get; private set; }

    [Header("Cinematic UI")]
    [Tooltip("화면 전체를 덮을 검은색 패널 (Image)")]
    public Image fadeOverlay;
    public float fadeDuration = 3f;

    [Header("Player References")]
    public MonoBehaviour playerController; // PlayerController 컴포넌트
    public MonoBehaviour footstepSound;    // FootstepSound 컴포넌트

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (fadeOverlay != null)
        {
            Color c = fadeOverlay.color;
            c.a = 0f;
            fadeOverlay.color = c;
            fadeOverlay.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 플레이어가 동사했을 때의 시네마틱 연출을 시작합니다.
    /// </summary>
    public void TriggerGameOverCinematic(System.Action onCinematicFinished)
    {
        StartCoroutine(GameOverRoutine(onCinematicFinished));
    }

    private IEnumerator GameOverRoutine(System.Action onCinematicFinished)
    {
        // 1. 조작 권한 즉시 박탈 (얼어붙음)
        if (playerController != null) playerController.enabled = false;
        if (footstepSound != null) footstepSound.enabled = false;

        // 2. 마지막 절망적인 독백 출력 (다른 자막을 끊고 강제 출력)
        if (SubtitleManager.Instance != null)
        {
            SubtitleManager.Instance.ShowSubtitle("추위가... 온몸을 덮쳐온다...", 3f, true);
        }

        // 3. 서서히 화면을 암흑으로 물들임 (눈이 감기는 연출)
        if (fadeOverlay != null)
        {
            float elapsed = 0f;
            Color c = fadeOverlay.color;
            c.r = 0f; c.g = 0f; c.b = 0f; // 완전한 검은색
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                fadeOverlay.color = c;
                yield return null;
            }
            c.a = 1f;
            fadeOverlay.color = c;
        }

        // 4. 완전한 암흑 속에서 오디오(눈보라 소리 등)만 들으며 여운 대기
        yield return new WaitForSeconds(1.5f);

        // 5. 연출이 모두 끝난 뒤 원래의 게임오버 패널 띄우기
        onCinematicFinished?.Invoke();
    }
}

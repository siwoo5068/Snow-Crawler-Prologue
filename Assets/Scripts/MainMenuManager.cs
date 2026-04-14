using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Apex Legends 스타일 메인 메뉴 관리자.
/// - Awake: 메뉴 상태 진입 (인게임 스크립트 비활성, 플레이어 카메라 OFF)
/// - StartGame(): 페이드 전환 후 인게임 상태 복원
/// - QuitGame(): 게임 종료
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  Inspector 연결
    // ─────────────────────────────────────────
    [Header("Cinemachine")]
    public Unity.Cinemachine.CinemachineCamera menuVirtualCamera; // MenuVirtualCamera

    [Header("Cameras")]
    public Camera mainMenuCamera;   // MainMenuCamera (Brain이 붙어있는 카메라)
    public Camera playerCamera;     // Player 자식 Main Camera

    [Header("Canvas")]
    public GameObject mainMenuCanvas;   // 메인 메뉴 전용 Canvas
    public GameObject gameplayCanvas;   // 기존 인게임 HUD Canvas

    [Header("Player Scripts to Disable During Menu")]
    public MonoBehaviour playerController;      // PlayerController
    public MonoBehaviour interactionPrompt;     // InteractionPrompt  ← 추가
    public MonoBehaviour survivalTimerComp;     // SurvivalTimer      ← 추가

    [Header("Fade")]
    public Image fadeImage;
    public float fadeDuration = 0.6f;

    [Header("Timing")]
    public float transitionDelay = 0.2f;

    // ─────────────────────────────────────────
    //  내부
    // ─────────────────────────────────────────
    private bool _transitioning = false;

    // ─────────────────────────────────────────
    //  초기화
    // ─────────────────────────────────────────
    void Awake()
    {
        Resolve();
        SetMenuState();
    }

    // ─────────────────────────────────────────
    //  자동 레퍼런스 탐색
    // ─────────────────────────────────────────
    void Resolve()
    {
        // MainMenuCamera
        if (mainMenuCamera == null)
        {
            var go = GameObject.Find("MainMenuCamera");
            if (go != null) mainMenuCamera = go.GetComponent<Camera>();
        }

        // Player 하위 탐색
        var player = GameObject.FindWithTag("Player");

        if (playerCamera == null && player != null)
            playerCamera = player.GetComponentInChildren<Camera>(true);

        if (playerController == null && player != null)
            playerController = player.GetComponent<PlayerController>();

        if (interactionPrompt == null)
        {
            // InteractionPrompt는 Player가 아닌 독립 오브젝트일 수 있음
            var ip = GameObject.FindObjectOfType<InteractionPrompt>();
            if (ip != null) interactionPrompt = ip;
        }

        if (survivalTimerComp == null && player != null)
            survivalTimerComp = player.GetComponent<SurvivalTimer>();

        // Canvas
        // Cinemachine Virtual Camera
        if (menuVirtualCamera == null)
        {
            var vcGO = GameObject.Find("MenuVirtualCamera");
            if (vcGO != null) menuVirtualCamera = vcGO.GetComponent<Unity.Cinemachine.CinemachineCamera>();
        }

        if (mainMenuCanvas == null)
            mainMenuCanvas = GameObject.Find("MainMenuCanvas");
        if (gameplayCanvas == null)
            gameplayCanvas = GameObject.Find("Canvas");

        // FadeImage
        if (fadeImage == null)
        {
            var go = GameObject.Find("FadeImage");
            if (go != null) fadeImage = go.GetComponent<Image>();
        }

        // 경고
        if (mainMenuCamera == null)
            Debug.LogWarning("[MainMenuManager] MainMenuCamera not found!");
        if (playerCamera == null)
            Debug.LogWarning("[MainMenuManager] Player Camera not found!");
    }

    // ─────────────────────────────────────────
    //  메뉴 상태 진입
    // ─────────────────────────────────────────
    void SetMenuState()
    {
        // 카메라 전환
        if (mainMenuCamera != null)    mainMenuCamera.enabled    = true;
        if (playerCamera   != null)    playerCamera.enabled      = false;
        if (menuVirtualCamera != null) menuVirtualCamera.enabled = true; // VirtualCamera ON → Brain이 이걸 렌더링

        // UI 전환
        if (gameplayCanvas != null) gameplayCanvas.SetActive(false);  // HUD 숨김
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);

        // 인게임 전용 스크립트들 비활성 (Update 루프 중단 → NullRef 방지)
        if (playerController  != null) playerController.enabled  = false;
        if (interactionPrompt != null) interactionPrompt.enabled = false;
        if (survivalTimerComp != null) survivalTimerComp.enabled = false;

        // 마우스 커서
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        // 페이드 이미지 초기화
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(false);
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }
    }

    // ─────────────────────────────────────────
    //  버튼 콜백
    // ─────────────────────────────────────────

    /// <summary>[Start Game] 버튼 onClick에 연결</summary>
    public void StartGame()
    {
        if (_transitioning) return;
        StartCoroutine(DoTransition());
    }

    /// <summary>[Quit] 버튼 onClick에 연결</summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ─────────────────────────────────────────
    //  전환 코루틴
    // ─────────────────────────────────────────
    IEnumerator DoTransition()
    {
        _transitioning = true;

        yield return new WaitForSeconds(transitionDelay);

        // ① 화면 페이드 아웃 (투명 → 검정)
        yield return StartCoroutine(DoFade(0f, 1f));

        // ② 메뉴 UI / 카메라 끄기
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);

        // VirtualCamera 비활성화 (회전 드라이버 포함 중단)
        if (menuVirtualCamera != null) menuVirtualCamera.enabled = false;

        if (mainMenuCamera != null)
        {
            mainMenuCamera.enabled = false; // Brain도 껌
        }

        // ③ 인게임 UI / 카메라 / 스크립트 켜기
        if (gameplayCanvas  != null) gameplayCanvas.SetActive(true);
        if (playerCamera    != null) playerCamera.enabled    = true;

        // 스크립트는 카메라가 켜진 다음 프레임에 활성화해야
        // InteractionPrompt의 _cam 체크가 정상 작동함
        if (playerController  != null) playerController.enabled  = true;
        if (interactionPrompt != null) interactionPrompt.enabled = true;
        if (survivalTimerComp != null) survivalTimerComp.enabled = true;

        // 마우스 잠금
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        // ④ 화면 페이드 인 (검정 → 투명)
        yield return StartCoroutine(DoFade(1f, 0f));

        if (fadeImage != null) fadeImage.gameObject.SetActive(false);

        _transitioning = false;
    }

    IEnumerator DoFade(float from, float to)
    {
        if (fadeImage == null) yield break;

        fadeImage.gameObject.SetActive(true);
        float elapsed = 0f;
        Color c = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, elapsed / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = to;
        fadeImage.color = c;
    }
}

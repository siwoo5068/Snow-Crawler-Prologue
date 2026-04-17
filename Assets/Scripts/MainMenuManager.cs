using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Cinemachine")]
    public Unity.Cinemachine.CinemachineCamera menuVirtualCamera;

    [Header("Cameras")]
    public Camera mainMenuCamera;
    public Camera playerCamera;

    [Header("Canvas")]
    public GameObject mainMenuCanvas;
    public GameObject gameplayCanvas;

    [Header("Player Scripts to Disable During Menu")]
    public MonoBehaviour playerController;
    public MonoBehaviour interactionPrompt;
    public MonoBehaviour survivalTimerComp;

    [Header("Fade")]
    public Image fadeImage;
    public float fadeDuration = 0.6f;

    [Header("Timing")]
    public float transitionDelay = 0.2f;

    private bool _transitioning = false;

    void Awake()
    {
        Resolve();
        SetMenuState();
    }

    void Resolve()
    {
        if (mainMenuCamera == null)
        {
            var go = GameObject.Find("MainMenuCamera");
            if (go != null) mainMenuCamera = go.GetComponent<Camera>();
        }

        var player = GameObject.FindWithTag("Player");

        if (playerCamera == null && player != null)
            playerCamera = player.GetComponentInChildren<Camera>(true);

        if (playerController == null && player != null)
            playerController = player.GetComponent<PlayerController>();

        if (interactionPrompt == null)
        {
#if UNITY_2023_1_OR_NEWER
            var ip = Object.FindAnyObjectByType<InteractionPrompt>();
#else
            var ip = Object.FindObjectOfType<InteractionPrompt>();
#endif
            if (ip != null) interactionPrompt = ip;
        }

        if (survivalTimerComp == null && player != null)
            survivalTimerComp = player.GetComponent<SurvivalTimer>();

        if (menuVirtualCamera == null)
        {
            var vcGO = GameObject.Find("MenuVirtualCamera");
            if (vcGO != null) menuVirtualCamera = vcGO.GetComponent<Unity.Cinemachine.CinemachineCamera>();
        }

        if (mainMenuCanvas == null)
            mainMenuCanvas = GameObject.Find("MainMenuCanvas");
        if (gameplayCanvas == null)
            gameplayCanvas = GameObject.Find("Canvas");

        if (fadeImage == null)
        {
            var go = GameObject.Find("FadeImage");
            if (go != null) fadeImage = go.GetComponent<Image>();
        }

        if (mainMenuCamera == null)
            Debug.LogWarning("[MainMenuManager] MainMenuCamera not found!");
        if (playerCamera == null)
            Debug.LogWarning("[MainMenuManager] Player Camera not found!");
    }

    void SetMenuState()
    {
        Time.timeScale = 1f;

        if (mainMenuCamera != null)    mainMenuCamera.enabled    = true;
        if (playerCamera   != null)    playerCamera.enabled      = false;
        if (menuVirtualCamera != null) menuVirtualCamera.enabled = true;

        if (gameplayCanvas != null) gameplayCanvas.SetActive(false);
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);

        if (playerController  != null) playerController.enabled  = false;
        if (interactionPrompt != null) interactionPrompt.enabled = false;
        if (survivalTimerComp != null) survivalTimerComp.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(false);
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }
    }

    public void StartGame()
    {
        if (_transitioning) return;
        StartCoroutine(DoTransition());
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    IEnumerator DoTransition()
    {
        _transitioning = true;

        yield return new WaitForSeconds(transitionDelay);

        yield return StartCoroutine(DoFade(0f, 1f));

        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);

        if (menuVirtualCamera != null) menuVirtualCamera.enabled = false;

        if (mainMenuCamera != null)
            mainMenuCamera.enabled = false;

        if (gameplayCanvas  != null) gameplayCanvas.SetActive(true);
        if (playerCamera    != null) playerCamera.enabled    = true;

        var survivalTimer = survivalTimerComp as SurvivalTimer;
        if (survivalTimer != null) survivalTimer.ResetState();

        var inv = survivalTimerComp != null
            ? survivalTimerComp.GetComponent<PlayerInventory>()
            : null;
        if (inv != null) inv.ResetState();

        if (playerController  != null) playerController.enabled  = true;
        if (interactionPrompt != null) interactionPrompt.enabled = true;
        if (survivalTimerComp != null) survivalTimerComp.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

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

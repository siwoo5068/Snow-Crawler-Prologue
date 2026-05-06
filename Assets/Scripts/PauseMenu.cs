using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// =============================================================================
//  PauseMenu — ESC키 일시정지
//
//  ESC → 게임 정지 + 커서 해제 + PausePanel 표시
//  Resume 버튼 or ESC 재입력 → 게임 재개
// =============================================================================
public class PauseMenu : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("일시정지 패널 (Inspector에서 연결)")]
    public GameObject pausePanel;

    [Tooltip("게임오버 패널 — 일시정지 중 비활성화 방지용 참조")]
    public GameObject gameOverPanel;

    [Header("Settings")]
    [Tooltip("일시정지/재개 키 (기본: ESC)")]
    public KeyCode pauseKey = KeyCode.Escape;

    // ── 내부 상태 ─────────────────────────────────────────────────────────
    private bool _isPaused = false;
    public  bool IsPaused => _isPaused;

    // ─────────────────────────────────────────────────────────────────────
    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    void Update()
    {
        // 게임오버/윈 상태에서는 ESC 무시
        if (gameOverPanel != null && gameOverPanel.activeSelf) return;

        if (Input.GetKeyDown(pauseKey))
        {
            if (_isPaused) Resume();
            else           Pause();
        }
    }

    // ── 공개 API (버튼에서 호출) ──────────────────────────────────────────
    public void Resume()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
        _isPaused = false;
    }

    public void Pause()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        _isPaused = true;
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

    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

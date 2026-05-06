using UnityEngine;
using UnityEngine.UI;
using TMPro;

// =============================================================================
//  JournalUI — Tab키 메모지/목표 노트
//
//  Tab 토글 → 화면 앞에 종이(메모지) 패널 표시
//  나중에 실제 종이 에셋 이미지로 교체 예정 (paperImage 슬롯에 할당)
//
//  ★ 이미지 교체 방법 ★
//   Inspector > Paper Image 슬롯에 종이 텍스처(Sprite)를 드래그하면 자동 교체됩니다.
// =============================================================================
public class JournalUI : MonoBehaviour
{
    [Header("Keys")]
    [Tooltip("메모지를 열고 닫는 키 (기본: Tab)")]
    public KeyCode journalKey = KeyCode.Tab;

    [Header("UI References")]
    [Tooltip("메모지 패널 루트 오브젝트")]
    public GameObject journalPanel;

    [Tooltip("종이 Image 컴포넌트 — 나중에 실제 에셋 스프라이트로 교체하세요")]
    public Image paperImage;

    [Tooltip("메모지 안에 들어갈 텍스트 (TMP)")]
    public TextMeshProUGUI journalText;

    [Tooltip("일시정지 메뉴 참조 — 일시정지 중에는 메모지 열기 차단")]
    public PauseMenu pauseMenu;

    [Header("Journal Content")]
    [Tooltip("메모지에 표시할 내용 (나중에 실제 에셋으로 교체 전까지 표시됩니다)")]
    [TextArea(6, 15)]
    public string journalContent =
        "[ 생존 일지 ]\n\n" +
        "① 별장 밖으로 나가 가구를 찾으세요.\n" +
        "   E키 — 가구 줍기\n\n" +
        "② 무거울수록 이동이 느려집니다.\n" +
        "   G키 — 아이템 내려놓기\n\n" +
        "③ 별장으로 돌아와 가구를 배치하세요.\n" +
        "   Q키 — 가구 배치 모드\n" +
        "   R키 — 배치 중 회전\n\n" +
        "④ 작업대에서 코트를 업그레이드하세요.\n" +
        "   E키 — 업그레이드 (재료 3개 필요)\n\n" +
        "⑤ 코트 Lv.2 이상 + 안락도 60% 달성 시\n" +
        "   무전기를 사용할 수 있습니다.\n\n" +
        "[ Tab ] 닫기";

    // ── 내부 상태 ─────────────────────────────────────────────────────────
    private bool _isOpen = false;

    // ─────────────────────────────────────────────────────────────────────
    void Start()
    {
        if (journalPanel != null) journalPanel.SetActive(false);
        if (journalText  != null) journalText.text = journalContent;
    }

    void Update()
    {
        // 일시정지 중에는 Tab 무시
        if (pauseMenu != null && pauseMenu.IsPaused) return;

        if (Input.GetKeyDown(journalKey))
        {
            if (_isOpen) CloseJournal();
            else         OpenJournal();
        }

        // ESC로도 닫기 가능
        if (_isOpen && Input.GetKeyDown(KeyCode.Escape))
            CloseJournal();
    }

    // ── 열기/닫기 ─────────────────────────────────────────────────────────
    public void OpenJournal()
    {
        if (journalPanel == null) return;
        journalPanel.SetActive(true);
        // 메모지를 볼 때 커서 해제 (선택적)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        _isOpen = true;
        Time.timeScale = 0f;   // 메모지 보는 동안 게임 정지
    }

    public void CloseJournal()
    {
        if (journalPanel == null) return;
        journalPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
        _isOpen = false;
        Time.timeScale = 1f;
    }
}

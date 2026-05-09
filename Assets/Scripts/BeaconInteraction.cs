using UnityEngine;

// =============================================================================
//  BeaconInteraction — 중계기 제어판 E키 상호작용
//  플레이어가 접근하면 "E키: 신호 발사" 프롬프트 표시
//  상호작용 시 EndingManager.StartEndingSequence() 호출
// =============================================================================
public class BeaconInteraction : MonoBehaviour
{
    [HideInInspector]
    public EndingManager endingManager;

    [HideInInspector]
    public Color warmGlowColor = new Color(1f, 0.6f, 0.2f, 1f);

    [Tooltip("상호작용 가능 거리")]
    public float interactDistance = 3f;

    private bool _playerInRange = false;
    private bool _activated = false;
    private bool _showingPrompt = false;
    private Light _warmLight;

    void Start()
    {
        // 은은한 주황빛 (기본은 꺼져있고, 접근하면 켜짐)
        var glowObj = new GameObject("WarmGlow");
        glowObj.transform.SetParent(transform.parent, false);
        glowObj.transform.localPosition = new Vector3(0f, 1f, 0f);

        _warmLight = glowObj.AddComponent<Light>();
        _warmLight.type = LightType.Point;
        _warmLight.color = warmGlowColor;
        _warmLight.intensity = 0f; // 처음엔 꺼짐
        _warmLight.range = 8f;
    }

    void Update()
    {
        if (_activated) return;

        // 플레이어 거리 체크
        var player = GameObject.FindWithTag("Player");
        if (player == null) return;

        float dist = Vector3.Distance(player.transform.position, transform.position);
        _playerInRange = dist <= interactDistance;

        // 접근 시 은은한 주황빛 점점 켜짐
        if (_warmLight != null)
        {
            float targetIntensity = _playerInRange ? 3f : 0f;
            _warmLight.intensity = Mathf.MoveTowards(_warmLight.intensity, targetIntensity, Time.deltaTime * 4f);
        }

        // 프롬프트 표시
        if (_playerInRange)
        {
            // 프롬프트 표시 (SubtitleManager 활용)
            if (!_showingPrompt && SubtitleManager.Instance != null)
            {
                SubtitleManager.Instance.ShowSubtitle("E키: 신호 발사", 1f);
                _showingPrompt = true;
            }

            // E키 상호작용
            if (Input.GetKeyDown(KeyCode.E))
            {
                Activate();
            }
        }
        else
        {
            _showingPrompt = false;
        }
    }

    void Activate()
    {
        _activated = true;

        // 주황빛 최대로
        if (_warmLight != null)
            _warmLight.intensity = 6f;

        // 엔딩 시작
        if (endingManager != null)
            endingManager.StartEndingSequence();

        Debug.Log("[BeaconInteraction] ★ 신호 발사! 엔딩 시작.");
    }
}

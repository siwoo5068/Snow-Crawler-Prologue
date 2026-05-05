using UnityEngine;

/// <summary>
/// ⚠️ DEPRECATED — 이 스크립트는 더 이상 사용하지 않습니다.
///
/// 생존(체온) 로직은 SurvivalTimer.cs 가 전담합니다.
/// 씬에 이 컴포넌트가 남아 있더라도 Awake에서 스스로 비활성화되므로
/// SurvivalTimer와 이중 감소 충돌이 발생하지 않습니다.
///
/// [안전하게 제거하는 방법]
///   Inspector에서 Player GameObject → PlayerSurvival 컴포넌트 → 우클릭 → Remove Component
/// </summary>
public class PlayerSurvival : MonoBehaviour
{
    void Awake()
    {
        // 자기 자신을 즉시 비활성화하여 모든 Update/Trigger 이벤트 차단
        enabled = false;
        Debug.LogWarning(
            "[PlayerSurvival] DEPRECATED: 이 컴포넌트는 비활성화되었습니다. " +
            "생존 로직은 SurvivalTimer.cs 를 사용하세요. " +
            "Inspector에서 이 컴포넌트를 제거해 주세요.",
            this
        );
    }
}
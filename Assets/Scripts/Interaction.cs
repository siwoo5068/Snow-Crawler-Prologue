using UnityEngine;

public class Interaction : MonoBehaviour
{
    public float interactRange = 3f;
    public SurvivalTimer timer;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        Vector3 radarCenter = transform.position + Vector3.up;
        Collider[] hitColliders = Physics.OverlapSphere(radarCenter, interactRange);

        Collider closestCollider = null;
        float minDistance = Mathf.Infinity;

        foreach (var hitCollider in hitColliders)
        {
            // 이제 ExitPoint(무전기) 태그도 레이더망에 걸리게 합니다.
            if (hitCollider.CompareTag("TimeItem") ||
                hitCollider.CompareTag("MaterialItem") ||
                hitCollider.CompareTag("CraftingTable") ||
                hitCollider.CompareTag("ExitPoint"))
            {
                float distance = Vector3.Distance(radarCenter, hitCollider.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCollider = hitCollider;
                }
            }
        }

        if (closestCollider != null)
        {
            if (closestCollider.CompareTag("TimeItem"))
            {
                if (timer != null) timer.AddTime(5f);
                Destroy(closestCollider.gameObject);
            }
            else if (closestCollider.CompareTag("MaterialItem"))
            {
                if (timer != null) timer.AddMaterial(1);
                Destroy(closestCollider.gameObject);
            }
            else if (closestCollider.CompareTag("CraftingTable"))
            {
                if (timer != null) timer.UpgradeCoat();
            }
            // 🌟 추가: 가장 가까운 게 무전기(ExitPoint)라면?
            else if (closestCollider.CompareTag("ExitPoint"))
            {
                // 코트 업그레이드를 한 상태여야만 탈출 가능! (기획 의도)
                if (timer.isUpgraded)
                {
                    timer.WinGame();
                }
                else
                {
                    Debug.Log("너무 추워서 무전기를 칠 수 없습니다! 코트부터 만드세요.");
                }
            }
        }
    }
}
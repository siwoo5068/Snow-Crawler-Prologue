using UnityEngine;
using TMPro;

public class InteractionPrompt : MonoBehaviour
{
    [Header("References")]
    public SurvivalTimer survivalTimer;
    public PlayerInventory inventory;

    [Header("UI")]
    public TextMeshProUGUI promptText;

    [Header("Settings")]
    public float checkRange = 3f;
    public LayerMask interactLayer;

    private Camera _cam;

    void Start()
    {
        _cam = Camera.main;

        if (survivalTimer == null)
            survivalTimer = GetComponent<SurvivalTimer>();
        if (inventory == null)
            inventory = GetComponent<PlayerInventory>();

        if (promptText != null)
            promptText.text = "";
    }

    void Update()
    {
        if (promptText == null) return;

        string message = GetPromptMessage();
        promptText.text = message;
    }

    string GetPromptMessage()
    {
        // 1. 월드 오브젝트 Raycast
        RaycastHit hit;
        Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out hit, checkRange))
        {
            string tag = hit.collider.tag;

            if (tag == "FurnitureItem")
            {
                FurnitureItem fi = hit.collider.GetComponent<FurnitureItem>();
                if (fi != null)
                {
                    float w = ItemDatabase.Weight.ContainsKey(fi.itemType)
                        ? ItemDatabase.Weight[fi.itemType] : 0f;
                    bool canCarry = inventory != null && inventory.CanCarry(w);

                    if (canCarry)
                        return string.Format("[E] {0} 줍기  ({1:F1} kg)", fi.itemType, w);
                    else
                        return string.Format("<color=#FF6B6B>[무게 초과]  {0}  ({1:F1} kg)</color>", fi.itemType, w);
                }
            }
            else if (tag == "MaterialItem")
            {
                return "[E] 보급품 수집";
            }
            else if (tag == "TimeItem")
            {
                return "[E] 온기 아이템 (+5초)";
            }
            else if (tag == "CraftingTable")
            {
                if (survivalTimer != null)
                {
                    int mat = survivalTimer.materialCount;
                    int need = survivalTimer.materialsPerUpgrade;
                    int lv = survivalTimer.upgradeLevel;
                    int maxLv = survivalTimer.maxUpgradeLevel;

                    if (lv >= maxLv)
                        return "<color=#FFD700>[작업대] 코트 최대 업그레이드 달성!</color>";
                    else if (mat >= need)
                        return string.Format("<color=#90EE90>[E] 코트 업그레이드  ({0}/{1} 보유)  +10초</color>", mat, need);
                    else
                        return string.Format("[작업대] 재료 부족  ({0}/{1})", mat, need);
                }
                return "[작업대] 재료를 모아오세요";
            }
            else if (tag == "ExitPoint")
            {
                // 승리 조건 체크 (나중에 GameProgress에서 관리)
                return "[E] 무전기  (탈출 요청)";
            }
        }

        // 2. Raycast 미스 + 안전지대 안 + 인벤토리에 가구 있음
        if (survivalTimer != null && survivalTimer.inSafeZone
            && inventory != null && inventory.GetItemCount() > 0)
        {
            return string.Format("[Q] 가구 배치  ({0}개 보유)", inventory.GetItemCount());
        }

        return "";
    }
}

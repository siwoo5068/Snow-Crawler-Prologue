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
        // Camera.main은 태그가 "MainCamera"인 활성화된 카메라를 반환.
        // 메인 메뉴 상태에서는 playerCamera가 꺼져 있어 null일 수 있으므로
        // 직접 Player 자식에서 찾아서 보관.
        var player = GameObject.FindWithTag("Player");
        if (player != null)
            _cam = player.GetComponentInChildren<Camera>(true); // inactive 포함 탐색

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

        // ── 핵심 null 가드 ──────────────────────────────────────
        // 1) 카메라가 없거나 비활성화 상태면 (메인 메뉴 상태) 즉시 종료.
        //    _cam이 꺼져 있어도 ViewportPointToRay가 NullRef를 터뜨리므로
        //    enabled 체크를 반드시 해야 함.
        if (_cam == null || !_cam.enabled || !_cam.gameObject.activeInHierarchy)
        {
            promptText.text = "";
            return;
        }
        // ─────────────────────────────────────────────────────────

        string message = GetPromptMessage();
        promptText.text = message;
    }

    string GetPromptMessage()
    {
        // _cam이 여기까지 왔으면 반드시 활성 상태
        Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

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
                    bool canCarry = inventory != null && inventory.CanCarry(fi.itemType);

                    if (canCarry)
                        return string.Format("[E] Pick up {0}  ({1:F1} kg)", fi.itemType, w);
                    else
                        return string.Format("<color=#FF6B6B>[Overweight]  {0}  ({1:F1} kg)</color>", fi.itemType, w);
                }
            }
            else if (tag == "MaterialItem")
            {
                return "[E] Collect Supply";
            }
            else if (tag == "TimeItem")
            {
                return "[E] Warmth Item (+5s)";
            }
            else if (tag == "CraftingTable")
            {
                if (survivalTimer != null)
                {
                    int mat   = survivalTimer.materialCount;
                    int need  = survivalTimer.materialsPerUpgrade;
                    int lv    = survivalTimer.upgradeLevel;
                    int maxLv = survivalTimer.maxUpgradeLevel;

                    if (lv >= maxLv)
                        return "<color=#FFD700>[Workbench] Coat fully upgraded!</color>";
                    else if (mat >= need)
                        return string.Format("<color=#90EE90>[E] Upgrade Coat  ({0}/{1} mats)  +10s</color>", mat, need);
                    else
                        return string.Format("[Workbench] Not enough materials  ({0}/{1})", mat, need);
                }
                return "[Workbench] Collect more materials";
            }
            else if (tag == "ExitPoint")
            {
                return "[E] Radio  (Request Rescue)";
            }
        }

        // Raycast 미스 + 안전지대 안 + 인벤토리에 가구 있음
        if (survivalTimer != null && survivalTimer.inSafeZone
            && inventory != null && inventory.GetItemCount() > 0)
        {
            return string.Format("[Q] Place Furniture  ({0} held)", inventory.GetItemCount());
        }

        return "";
    }
}

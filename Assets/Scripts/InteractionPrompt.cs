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
    private string _lastMessage = "";

    void Start()
    {
        ResolveCamera();

        if (survivalTimer == null)
            survivalTimer = GetComponent<SurvivalTimer>();
        if (inventory == null)
            inventory = GetComponent<PlayerInventory>();

        if (promptText != null)
            promptText.text = "";
    }

    void OnEnable()
    {
        ResolveCamera();
    }

    void ResolveCamera()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var cam = player.GetComponentInChildren<Camera>(true);
            if (cam != null)
            {
                _cam = cam;
                return;
            }
        }
        _cam = Camera.main;
    }

    void Update()
    {
        if (promptText == null) return;

        if (_cam == null || !_cam.enabled || !_cam.gameObject.activeInHierarchy)
        {
            promptText.text = "";
            return;
        }

        string message = GetPromptMessage();
        if (message != _lastMessage)
        {
            _lastMessage = message;
            promptText.text = message;
        }
    }

    string GetPromptMessage()
    {
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
                        return string.Format("<color=#90EE90>[E] Upgrade Coat  ({0}/{1} mats)  +{2:F0}s</color>", mat, need, survivalTimer.timePerUpgrade);
                    else
                        return string.Format("[Workbench] Not enough materials  ({0}/{1})", mat, need);
                }
                return "[Workbench] Collect more materials";
            }
        }

        if (survivalTimer != null && survivalTimer.inSafeZone
            && inventory != null && inventory.GetItemCount() > 0)
        {
            return string.Format("[Q] Place Furniture  ({0} held)", inventory.GetItemCount());
        }

        return "";
    }
}

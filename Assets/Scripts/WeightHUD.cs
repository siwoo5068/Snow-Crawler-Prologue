using UnityEngine;
using TMPro;

public class WeightHUD : MonoBehaviour
{
    public PlayerInventory inventory;
    public TextMeshProUGUI weightText;
    public TextMeshProUGUI dropHintText;

    [Header("Display Settings")]
    public float dangerWeight = 12f;
    public Color safeColor = Color.white;
    public Color warningColor = Color.yellow;
    public Color dangerColor = Color.red;

    void Start()
    {
        if (inventory == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
                inventory = player.GetComponent<PlayerInventory>();
        }

        if (dropHintText != null)
            dropHintText.text = "";
    }

    void Update()
    {
        if (weightText == null || inventory == null) return;

        float weight = inventory.TotalWeight;
        int count = inventory.GetItemCount();

        weightText.text = count > 0
            ? string.Format("무게: {0:F1} / {1:F0} kg  ({2}개)", weight, inventory.maxWeight, count)
            : "무게: 비어있음";

        if (weight >= inventory.maxWeight)
            weightText.text += "  [최대]";

        float ratio = Mathf.Clamp01(weight / dangerWeight);

        if (ratio < 0.5f)
            weightText.color = Color.Lerp(safeColor, warningColor, ratio * 2f);
        else
            weightText.color = Color.Lerp(warningColor, dangerColor, (ratio - 0.5f) * 2f);

        if (dropHintText != null)
        {
            if (weight >= inventory.maxWeight && count > 0)
                dropHintText.text = "<color=#FF6B6B>[G] 아이템을 내려놓으면 빨라집니다</color>";
            else
                dropHintText.text = "";
        }
    }
}

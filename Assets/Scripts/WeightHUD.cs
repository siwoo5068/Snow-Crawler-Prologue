using UnityEngine;
using TMPro;

public class WeightHUD : MonoBehaviour
{
    public PlayerInventory inventory;
    public TextMeshProUGUI weightText;

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
    }

    void Update()
    {
        if (weightText == null || inventory == null) return;

        float weight = inventory.TotalWeight;
        int count = inventory.GetItemCount();

        weightText.text = count > 0
            ? string.Format("Weight: {0:F1} kg ({1})", weight, count)
            : "Weight: Empty";

        float ratio = Mathf.Clamp01(weight / dangerWeight);

        if (ratio < 0.5f)
            weightText.color = Color.Lerp(safeColor, warningColor, ratio * 2f);
        else
            weightText.color = Color.Lerp(warningColor, dangerColor, (ratio - 0.5f) * 2f);
    }
}


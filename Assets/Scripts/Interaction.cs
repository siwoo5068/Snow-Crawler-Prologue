using UnityEngine;

public class Interaction : MonoBehaviour
{
    public float interactRange = 3f;
    public SurvivalTimer timer;
    public PlayerInventory inventory;

    void Start()
    {
        if (timer == null) timer = GetComponent<SurvivalTimer>();
        if (inventory == null) inventory = GetComponent<PlayerInventory>();
    }

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
            if (hitCollider.CompareTag("TimeItem") ||
                hitCollider.CompareTag("MaterialItem") ||
                hitCollider.CompareTag("CraftingTable") ||
                hitCollider.CompareTag("ExitPoint") ||
                hitCollider.CompareTag("FurnitureItem"))
            {
                float distance = Vector3.Distance(radarCenter, hitCollider.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCollider = hitCollider;
                }
            }
        }

        if (closestCollider == null) return;

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
        else if (closestCollider.CompareTag("ExitPoint"))
        {
            if (timer != null && timer.isUpgraded)
                timer.WinGame();
        }
        else if (closestCollider.CompareTag("FurnitureItem"))
        {
            FurnitureItem furniture = closestCollider.GetComponent<FurnitureItem>();
            if (furniture != null && inventory != null)
            {
                inventory.AddItem(furniture.itemType);
                Destroy(closestCollider.gameObject);
            }
        }
    }
}
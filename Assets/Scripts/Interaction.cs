using UnityEngine;

public class Interaction : MonoBehaviour
{
    public float interactRange = 3f;
    public SurvivalTimer timer;
    public PlayerInventory inventory;
    public GameProgress gameProgress;

    void Start()
    {
        if (timer == null) timer = GetComponent<SurvivalTimer>();
        if (inventory == null) inventory = GetComponent<PlayerInventory>();
        if (gameProgress == null) gameProgress = GameObject.FindObjectOfType<GameProgress>();
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
            if (hitCollider.CompareTag("FurnitureItem") ||
                hitCollider.CompareTag("TimeItem") ||
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
            if (gameProgress != null) gameProgress.TryWin();
        }
        else if (closestCollider.CompareTag("FurnitureItem"))
        {
            FurnitureItem furniture = closestCollider.GetComponent<FurnitureItem>();
            if (furniture != null && inventory != null)
            {
                if (inventory.AddItem(furniture.itemType))
                    Destroy(closestCollider.gameObject);
            }
        }
    }
}
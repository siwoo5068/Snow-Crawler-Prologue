using UnityEngine;

public class FurnitureDrop : MonoBehaviour
{
    [Header("References")]
    public PlayerInventory inventory;

    [Header("Drop Settings")]
    public float dropDistance = 2f;
    public float snowBurialTime = 20f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip dropSound;

    void Start()
    {
        if (inventory == null)
            inventory = GetComponent<PlayerInventory>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
            TryDrop();
    }

    void TryDrop()
    {
        if (inventory == null) return;

        ItemType? dropped = inventory.DropLastItem();
        if (dropped == null) return;

        Vector3 spawnPos = transform.position + transform.forward * dropDistance;
        spawnPos.y = transform.position.y;

        GameObject droppedObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        droppedObj.name = "Dropped_" + dropped.Value;
        droppedObj.transform.position = spawnPos;
        droppedObj.transform.localScale = Vector3.one * 0.6f;
        droppedObj.tag = "FurnitureItem";

        FurnitureItem fi = droppedObj.AddComponent<FurnitureItem>();
        fi.itemType = dropped.Value;

        SnowBurial burial = droppedObj.AddComponent<SnowBurial>();
        burial.burialTime = snowBurialTime;

        if (audioSource != null && dropSound != null)
            audioSource.PlayOneShot(dropSound);
    }
}

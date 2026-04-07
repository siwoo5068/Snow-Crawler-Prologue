using UnityEngine;

public class FootstepSound : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;
    public AudioSource audioSource;
    public AudioClip[] stepClips;

    [Header("Step Settings")]
    public float baseStepInterval = 0.45f;
    private float stepTimer;

    [Header("Weight Feedback")]
    public PlayerInventory inventory;
    public float intervalPerWeight = 0.03f;
    public float maxStepInterval = 0.85f;

    void Start()
    {
        if (inventory == null)
            inventory = GetComponent<PlayerInventory>();
    }

    void Update()
    {
        if (controller.isGrounded && controller.velocity.magnitude > 0.1f)
        {
            stepTimer += Time.deltaTime;

            float effectiveInterval = baseStepInterval;
            if (inventory != null)
                effectiveInterval = Mathf.Min(maxStepInterval, baseStepInterval + inventory.TotalWeight * intervalPerWeight);

            if (stepTimer >= effectiveInterval)
            {
                PlayRandomStep();
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    void PlayRandomStep()
    {
        if (stepClips.Length > 0)
        {
            int randomIndex = Random.Range(0, stepClips.Length);
            audioSource.clip = stepClips[randomIndex];

            float weightFactor = (inventory != null) ? Mathf.Clamp01(inventory.TotalWeight / 15f) : 0f;
            audioSource.pitch = Random.Range(0.8f, 1.2f) - weightFactor * 0.2f;
            audioSource.volume = Random.Range(0.8f, 1.0f) + weightFactor * 0.15f;

            audioSource.Play();
        }
    }
}
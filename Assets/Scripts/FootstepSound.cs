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
        if (controller == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
                controller = player.GetComponent<CharacterController>();
        }

        if (inventory == null)
            inventory = GetComponent<PlayerInventory>();
    }

    void Update()
    {
        if (controller == null) return;

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
        if (stepClips == null || stepClips.Length == 0) return;
        if (audioSource == null) return;

        int randomIndex = Random.Range(0, stepClips.Length);
        float weightFactor = (inventory != null) ? Mathf.Clamp01(inventory.TotalWeight / 15f) : 0f;
        float pitch  = Random.Range(0.8f, 1.2f) - weightFactor * 0.2f;
        float volume = Random.Range(0.8f, 1.0f) + weightFactor * 0.15f;

        audioSource.pitch = pitch;
        audioSource.PlayOneShot(stepClips[randomIndex], volume);
    }
}
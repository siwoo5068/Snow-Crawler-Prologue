using UnityEngine;

public class FootstepSound : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;
    public AudioSource audioSource;
    
    [Header("Audio Clips")]
    [Tooltip("기본 발소리 (눈밭)")]
    public AudioClip[] snowClips;
    [Tooltip("나무 바닥 발소리 (별장 안)")]
    public AudioClip[] woodClips;

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
        if (audioSource == null) return;

        AudioClip[] targetClips = snowClips; // 기본은 눈 소리

        // 바닥 재질 검사 (발 밑으로 Raycast)
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 0.5f))
        {
            // 바닥의 Tag가 Wood면 나무 소리로 변경
            if (hit.collider.CompareTag("Wood"))
            {
                targetClips = woodClips;
            }
        }

        if (targetClips == null || targetClips.Length == 0) return;

        int randomIndex = Random.Range(0, targetClips.Length);
        float weightFactor = (inventory != null) ? Mathf.Clamp01(inventory.TotalWeight / 15f) : 0f;
        
        float pitch  = Random.Range(0.8f, 1.2f) - weightFactor * 0.2f;
        float volume = Random.Range(0.8f, 1.0f) + weightFactor * 0.15f;

        audioSource.pitch = pitch;
        audioSource.PlayOneShot(targetClips[randomIndex], volume);
    }
}

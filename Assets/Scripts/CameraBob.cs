using UnityEngine;

public class CameraBob : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;
    public PlayerInventory inventory;

    [Header("Bob Settings")]
    public float baseBobFrequency = 8f;
    public float baseBobAmplitude = 0.03f;
    public float weightAmplitudeBonus = 0.005f;
    public float maxAmplitude = 0.08f;
    public float smoothSpeed = 12f;

    private float bobTimer;
    private float defaultCamY;
    private bool initialized;

    void Start()
    {
        if (controller == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
                controller = player.GetComponent<CharacterController>();
        }

        if (inventory == null && controller != null)
            inventory = controller.GetComponent<PlayerInventory>();

        defaultCamY = transform.localPosition.y;
        initialized = true;
    }

    void LateUpdate()
    {
        if (!initialized || controller == null) return;

        bool isMoving = controller.isGrounded && controller.velocity.magnitude > 0.2f;

        if (isMoving)
        {
            float weight = (inventory != null) ? inventory.TotalWeight : 0f;
            float amplitude = Mathf.Min(maxAmplitude, baseBobAmplitude + weight * weightAmplitudeBonus);
            float frequency = baseBobFrequency - weight * 0.15f;
            frequency = Mathf.Max(frequency, 4f);

            bobTimer += Time.deltaTime * frequency;
            bobTimer %= Mathf.PI * 2f;
            float bobOffset = Mathf.Sin(bobTimer) * amplitude;

            Vector3 pos = transform.localPosition;
            pos.y = Mathf.Lerp(pos.y, defaultCamY + bobOffset, Time.deltaTime * smoothSpeed);
            transform.localPosition = pos;
        }
        else
        {
            bobTimer = 0f;
            Vector3 pos = transform.localPosition;
            pos.y = Mathf.Lerp(pos.y, defaultCamY, Time.deltaTime * smoothSpeed);
            transform.localPosition = pos;
        }
    }
}

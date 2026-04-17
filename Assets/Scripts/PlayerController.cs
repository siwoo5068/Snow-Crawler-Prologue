using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float baseSpeed = 5f;
    public float mouseSensitivity = 2f;

    [Header("Weight Penalty")]
    public PlayerInventory inventory;
    public float speedLossPerWeight = 0.3f;
    public float minSpeedRatio = 0.2f;

    private CharacterController controller;
    private Transform cameraTransform;
    private float verticalLookRotation;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        var cam = GetComponentInChildren<Camera>(true);
        if (cam != null) cameraTransform = cam.transform;

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (inventory == null)
            inventory = GetComponent<PlayerInventory>();
    }

    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (cameraTransform == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
        cameraTransform.localEulerAngles = Vector3.right * verticalLookRotation;

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        float currentWeight = (inventory != null) ? inventory.TotalWeight : 0f;
        float speed = Mathf.Max(baseSpeed * minSpeedRatio, baseSpeed - currentWeight * speedLossPerWeight);

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.SimpleMove(move * speed);
    }
}
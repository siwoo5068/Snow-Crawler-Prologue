using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float baseSpeed = 5f;
    public float mouseSensitivity = 2f;

    [Header("Sprint")]
    [Tooltip("달리기 속도 배율")]
    public float sprintMultiplier = 1.6f;
    [Tooltip("달리기 키")]
    public KeyCode sprintKey = KeyCode.LeftShift;

    /// <summary>
    /// true면 달리기 불가 (라스트 찬스 등 외부에서 잠금용)
    /// </summary>
    [HideInInspector]
    public bool sprintLocked = false;

    [Header("Weight Penalty")]
    public PlayerInventory inventory;
    public float speedLossPerWeight = 0.3f;
    public float minSpeedRatio = 0.2f;

    /// <summary>
    /// 현재 플레이어가 달리는 중인지 (FootstepSound 등에서 참조 가능)
    /// </summary>
    public bool IsSprinting { get; private set; }

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

        // 달리기: Shift 누르고 있고, 잠금 안 걸려있고, 앞으로 이동 중일 때
        IsSprinting = Input.GetKey(sprintKey) && !sprintLocked && moveZ > 0.1f;
        if (IsSprinting)
            speed *= sprintMultiplier;

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.SimpleMove(move * speed);
    }
}
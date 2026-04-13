using UnityEngine;
using UnityEngine.InputSystem;
using Fusion; 

/// <summary>
/// Player movement controller for 3D top-down.
/// Reads input from InputManager, moves a CharacterController
/// relative to the camera and rotates the visual model to face movement direction.
/// </summary>
public class PlayerController : NetworkBehaviour
{
    [Header("References")]
    [Tooltip("Player stats component (health, move speed, etc.).")]
    [SerializeField] private PlayerStats playerStats;

    [Tooltip("Camera transform used as reference for movement (usually main camera or Cinemachine virtual camera).")]
    [SerializeField] private Transform cameraTransform;

    [Tooltip("Root transform of the visual model (rotates to face movement direction).")]
    [SerializeField] private Transform visualRoot;

    private Rigidbody rb;
    private Vector2 _moveContext; // Stores the WASD/Joystick value

    /// <summary>
    /// Инициализирует ссылки на CharacterController, PlayerStats и камеру.
    /// </summary>
    public override void Spawned()
    {
        rb = GetComponent<Rigidbody>();

        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (HasInputAuthority) // Only the local player should be followed
        {
            // Find the camera and give it our transform
            var cam = Camera.main.GetComponent<CameraTarget>(); 
            if (cam != null) cam.target = transform; 
            Camera.main.GetComponent<CameraTarget>().target = transform;
        }
    }

    /// <summary>
    /// Главный игровой цикл контроллера.
    /// </summary>
    private void Update()
    {
        if (InputManager.Instance == null)
            return;

        HandleMovement();

        InputManager.Instance.ResetButtonFlags();
    }

    /// <summary>
    /// Считает движение относительно камеры и двигает CharacterController.
    /// Вращает визуальную модель по направлению движения.
    /// </summary>

    public void OnMove(InputValue value)
    {
        _moveContext = value.Get<Vector2>(); 
    }

    public void HandleMovement() 
    {
        Vector3 moveInput = InputManager.Instance.MoveInput;
        Vector3 moveDirection = Vector3.zero;

        if (moveInput.sqrMagnitude > 0.001f && cameraTransform != null)
        {
            // Для топ-даун: движение по XZ, игнорируем Y
            Vector3 forward = cameraTransform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 right = cameraTransform.right;
            right.y = 0f;
            right.Normalize();

            moveDirection = forward * moveInput.x + right * moveInput.x;
            moveDirection.Normalize();
        }

        float speed = 5f;
        float rotationSpeed = 720f;

        if (playerStats != null && playerStats.playerData != null)
        {
            speed = playerStats.playerData.moveSpeed;
            rotationSpeed = playerStats.playerData.rotationSpeed;
        }

        if (HasInputAuthority)
        {
            rb.linearVelocity = _moveContext * speed;
        }

        // Вращаем модель по направлению движения (если есть движение)
        if (visualRoot != null && moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            visualRoot.rotation = Quaternion.Slerp(
                visualRoot.rotation,
                targetRotation, 
                rotationSpeed * Mathf.Deg2Rad * Time.deltaTime
            );
        }
    }
}
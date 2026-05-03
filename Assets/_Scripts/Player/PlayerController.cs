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
        Debug.Log($"[SPAWN CHECK] {gameObject.name} has appeared! Is it active? {gameObject.activeInHierarchy}", gameObject);

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
    public void HandleMovement()
    {
        if (InputManager.Instance == null)
            return;

        FixedUpdateNetwork();

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

    public override void FixedUpdateNetwork()
    {
        Vector3 moveInput = InputManager.Instance.MoveInput;
        Vector3 moveDirection = Vector3.zero;

        float speed = playerStats.playerData.moveSpeed;

        if (GetInput(out NetworkInputData data))
        {
            Vector3 forward = cameraTransform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 right = cameraTransform.right;
            right.y = 0f;
            right.Normalize();

            // Fix the X/Y mapping
            Vector3 move = (forward * data.direction.y) + (right * data.direction.x);

            if (move.magnitude > 0.1f)
            {
                transform.position += move.normalized * speed * Runner.DeltaTime;
                if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
                    playerStats.playerAnim.SetTrigger("walk_trig");
            }
        }
    }
}
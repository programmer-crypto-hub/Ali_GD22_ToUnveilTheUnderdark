using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public InputActionAsset inputActions;

    // Action Maps
    private InputActionMap playerActionMap;
    private InputActionMap uiActionMap;

    // Player Actions
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction attackAction;
    private InputAction interactAction;
    //private InputAction sprintAction;
    //private InputAction crouchAction;
    private InputAction pauseAction;
    private InputAction zoomAction;

    // UI Actions
    private InputAction cancelAction;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public float ZoomInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool AttackPressed { get; private set; }
    public bool InteractPressed { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool CrouchHeld { get; private set; }


    public Action OnJumpPressed;
    public Action OnAttackPressed;
    public Action OnInteractPressed;
    public Action OnPausePressed;
    public Action OnCancelPressed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeInputSystem();
    }

    private void InitializeInputSystem()
    {
        if (inputActions == null)
        {
            Debug.LogError("InputManager: No Input Actions Asset!");
            return;
        }

        playerActionMap = inputActions.FindActionMap("Player");
        uiActionMap = inputActions.FindActionMap("UI");

        if (playerActionMap == null)
        {
            Debug.LogError("InputManager: No Action Map 'Player'­!");
            return;
        }
        moveAction = playerActionMap.FindAction("Move");
        lookAction = playerActionMap.FindAction("Look");
        jumpAction = playerActionMap.FindAction("Jump");
        attackAction = playerActionMap.FindAction("Attack");
        interactAction = playerActionMap.FindAction("Interact");
        //sprintAction = playerActionMap.FindAction("Sprint");
        //crouchAction = playerActionMap.FindAction("Crouch");
        pauseAction = playerActionMap.FindAction("Pause");
        cancelAction = uiActionMap.FindAction("Cancel");
        zoomAction = playerActionMap.FindAction("Zoom");
        if (jumpAction != null)
            jumpAction.performed += OnJumpPerformed;
        if (attackAction != null)
            attackAction.performed += OnAttackPerformed;
        if (interactAction != null)
            interactAction.performed += OnInteractPerformed;
        if (pauseAction != null)
            pauseAction.performed += OnPausePerformed;
        if (cancelAction != null)
            cancelAction.performed += OnCancelPerformed;

        EnablePlayerInput();
    }

    private void OnEnable()
    {
        if (inputActions != null)
            inputActions.Enable();

        if (EventBus.Instance != null)
        {
            EventBus.Instance.OnGamePaused += HandleGamePaused;
            EventBus.Instance.OnGameResumed += HandleGameResumed;
        }
    }

    private void OnDisable()
    {
        if (inputActions != null)
            inputActions.Disable();
        if (EventBus.Instance != null)
        {
            EventBus.Instance.OnGamePaused -= HandleGamePaused;
            EventBus.Instance.OnGameResumed -= HandleGameResumed;
        }
    }

    private void OnDestroy()
    {
        if (jumpAction != null)
            jumpAction.performed -= OnJumpPerformed;
        if (attackAction != null)
            attackAction.performed -= OnAttackPerformed;
        if (interactAction != null)
            interactAction.performed -= OnInteractPerformed;
        if (pauseAction != null)
            pauseAction.performed -= OnPausePerformed;
        if (cancelAction != null)
            cancelAction.performed -= OnCancelPerformed;
    }

    private void Update()
    {
        UpdateInputValues();
    }

    private void UpdateInputValues()
    {
        MoveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        LookInput = lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero;
        //SprintHeld = sprintAction != null && sprintAction.IsPressed();
        //CrouchHeld = crouchAction != null && crouchAction.IsPressed();
        ZoomInput = zoomAction != null ? zoomAction.ReadValue<Vector2>().y : 0f;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        JumpPressed = true;
        OnJumpPressed?.Invoke();
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        AttackPressed = true;
        OnAttackPressed?.Invoke();
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        InteractPressed = true;
        OnInteractPressed?.Invoke();
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        OnPausePressed?.Invoke();
    }

    private void OnCancelPerformed(InputAction.CallbackContext context)
    {
        OnCancelPressed?.Invoke();
    }


    public void ResetButtonFlags()
    {
        JumpPressed = false;
        AttackPressed = false;
        InteractPressed = false;
    }

    public void EnablePlayerInput()
    {
        if (playerActionMap != null)
            playerActionMap.Enable();
        if (uiActionMap != null)
            uiActionMap.Disable();
    }

    public void EnableUIInput()
    {
        if (playerActionMap != null)
            playerActionMap.Disable();
        if (uiActionMap != null)
            uiActionMap.Enable();
    }

    private void HandleGamePaused()
    {
        if (playerActionMap != null)
            playerActionMap.Disable();

        Debug.Log("InputManager: Player input disabled (game paused)");
    }

    private void HandleGameResumed()
    {
        playerActionMap.Enable();

        Debug.Log("InputManager: Player input enabled (game resumed)");
    }

    public Vector2 GetMoveInput()
    {
        return MoveInput;
    }

    public Vector2 GetLookInput()
    {
        return LookInput;
    }

    public float GetZoomInput()
    {
        return ZoomInput;
    }

    public bool IsJumpPressed()
    {
        return JumpPressed;
    }

    public bool IsAttackPressed()
    {
        return AttackPressed;
    }

    public bool IsInteractPressed()
    {
        return InteractPressed;
    }

    //public bool IsSprintHeld()
    //{
    //    return SprintHeld;
    //}

    //public bool IsCrouchHeld()
    //{
    //    return CrouchHeld;
    //}
}
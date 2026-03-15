using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public InputActionAsset inputActions;

    private InputActionMap playerActionMap;
    private InputActionMap uiActionMap;

    private InputAction moveAction;
    private InputAction turnEndAction;
    private InputAction lookAction;
    private InputAction zoomAction;
    private InputAction jumpAction;
    private InputAction attackAction;
    private InputAction interactAction;
    private InputAction sprintAction;
    private InputAction crouchAction;
    private InputAction pauseAction;
    private InputAction cancelAction;
    private InputAction weaponNextAction;
    private InputAction weaponPrevAction;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public float ZoomInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool AttackPressed { get; private set; }
    public bool TurnEndPressed { get; private set; }
    public bool InteractPressed { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool CrouchHeld { get; private set; }

    public Action OnJumpPressed;
    public Action OnAttackPressed;
    public Action OnInteractPressed;
    public Action OnPausePressed;
    public Action OnCancelPressed;
    public Action OnTurnEndPressed;
    public Action OnDiceRolled;
    public Action OnWeaponNextPressed;
    public Action OnWeaponPrevPressed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        playerActionMap = null;
    }

    private void Start()
    {
        InitializeUIInputSystem();
    }

    public void InitializePlayerInputSystem()
    {
        if (inputActions == null)
        {
            Debug.LogError("InputManager: Input Actions Asset не назначен!");
            return;
        }
        playerActionMap = inputActions.FindActionMap("Player");
        if (playerActionMap == null)
        {
            Debug.LogError("InputManager: Action Map 'Player' не найден!");
            return;
        }
        moveAction = playerActionMap.FindAction("Move");
        turnEndAction = playerActionMap.FindAction("TurnEnd");
        lookAction = playerActionMap.FindAction("Look");
        zoomAction = playerActionMap.FindAction("Zoom");
        jumpAction = playerActionMap.FindAction("Jump");
        attackAction = playerActionMap.FindAction("Attack");
        interactAction = playerActionMap.FindAction("Interact");
        sprintAction = playerActionMap.FindAction("Sprint");
        crouchAction = playerActionMap.FindAction("Crouch");
        pauseAction = playerActionMap.FindAction("Pause");
        weaponNextAction = playerActionMap.FindAction("Next");
        weaponPrevAction = playerActionMap.FindAction("Previous");
        if (jumpAction != null)
            jumpAction.performed += _ => OnJumpPerformed();
        if (turnEndAction != null)
            turnEndAction.performed += _ => OnTurnEndPerformed();
        if (attackAction != null)
            attackAction.performed += _ => OnAttackPerformed();
        if (interactAction != null)
            interactAction.performed += _ => OnInteractPerformed();
        if (pauseAction != null)
            pauseAction.performed += _ => OnPausePerformed();
        if (cancelAction != null)
            cancelAction.performed += _ => OnCancelPerformed();
        if (weaponNextAction != null)
            weaponNextAction.performed += _ => OnWeaponNextPerformed();
        if (weaponPrevAction != null)
            weaponPrevAction.performed += _ => OnWeaponPrevPerformed();
        EnablePlayerInput();
    }
    public void InitializeUIInputSystem()
    {
        if (inputActions == null)
        {
            Debug.LogError("InputManager: Input Actions Asset не назначен!");
            return;
        }
        uiActionMap = inputActions.FindActionMap("UI");
        if (uiActionMap != null)
            cancelAction = uiActionMap.FindAction("Cancel");
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

    public void OnDestroy()
    {
        if (jumpAction != null)
            jumpAction.performed -= _ => OnJumpPerformed();
        if (turnEndAction != null)
            turnEndAction.performed -= _ => OnTurnEndPerformed();
        if (attackAction != null)
            attackAction.performed -= _ => OnAttackPerformed();
        if (interactAction != null)
            interactAction.performed -= _ => OnInteractPerformed();
        if (pauseAction != null)
            pauseAction.performed -= _ => OnPausePerformed();
        if (cancelAction != null)
            cancelAction.performed -= _ => OnCancelPerformed();
        if (weaponNextAction != null)
            weaponNextAction.performed -= _ => OnWeaponNextPerformed();
        if (weaponPrevAction != null)
            weaponPrevAction.performed -= _ => OnWeaponPrevPerformed();
    }

    private void Update()
    {
        UpdateInputValues();
    }

    private void UpdateInputValues()
    {
        MoveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        LookInput = lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero;
        ZoomInput = zoomAction != null ? zoomAction.ReadValue<Vector2>().y : 0f;
        SprintHeld = sprintAction != null && sprintAction.IsPressed();
        CrouchHeld = crouchAction != null && crouchAction.IsPressed();
    }

    private void OnJumpPerformed()
    {
        JumpPressed = true;
        OnJumpPressed?.Invoke();
    }

    private void OnAttackPerformed()
    {
        AttackPressed = true;
        OnAttackPressed?.Invoke();
    }

    private void OnTurnEndPerformed()
    {
        TurnEndPressed = true;
        OnTurnEndPressed?.Invoke();
    }

    private void OnInteractPerformed()
    {
        InteractPressed = true;
        OnInteractPressed?.Invoke();
    }

    private void OnPausePerformed()
    {
        OnPausePressed?.Invoke();
    }

    private void OnCancelPerformed()
    {
        OnCancelPressed?.Invoke();
    }

    private void OnWeaponNextPerformed()
    {
        OnWeaponNextPressed?.Invoke();
    }

    private void OnWeaponPrevPerformed()
    {
        OnWeaponPrevPressed?.Invoke();
    }

    public void ResetButtonFlags()
    {
        JumpPressed = false;
        AttackPressed = false;
        InteractPressed = false;
        TurnEndPressed = false;
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
        if (playerActionMap != null)
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

    public bool IsTurnEndPressed()
    {
        return TurnEndPressed;
    }

    public bool IsInteractPressed()
    {
        return InteractPressed;
    }

    public bool IsSprintHeld()
    {
        return SprintHeld;
    }

    public bool IsCrouchHeld()
    {
        return CrouchHeld;
    }
}
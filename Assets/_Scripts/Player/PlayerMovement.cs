using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Stats to Obtain Data")]
    [Tooltip("Reference to PlayerStats component to access player data and events.")]
    public PlayerStats playerStats;

    public InputManager inputManager;

    [Header("Animation settings")]
    public Animator playerAnim { get; private set; }
    public AnimationClip playerRun;

    public void Awake()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();
        if (inputManager == null)
            inputManager = InputManager.Instance;
        startBTN.onClick.AddListener(InputManager.Instance.IsTurnStartPressed());
    }
}

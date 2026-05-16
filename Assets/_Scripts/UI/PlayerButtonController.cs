using Fusion;
using UnityEngine;
using UnityEngine.UI;

/* 
 * Central Script for applying functions to buttons.
 * Reason of Usage: Fusion-inherited buttons' OnClick() cannot directly call functions that require parameters, so we use this script to bind them together.
 */
public class PlayerButtonController : NetworkBehaviour
{
    [Header("Player")]
    public NetworkObject playerNetworkObject;

    [SerializeField] public Button rollDiceButton;
    [SerializeField] public Button shopButton;
    [SerializeField] public Button endTurnButton;
    [SerializeField] public Button nextWeapon;
    [SerializeField] public Button prevButton;
    [SerializeField] public Button winMenuButton;
    [SerializeField] public Button loseMenuButton;
    [SerializeField] public Button menuQuitButton;
    [SerializeField] public Button menuSettingsButton;
    [SerializeField] public Button quitButton;
    [SerializeField] public Button settingsButton;

    public override void Spawned()
    {
        if (playerNetworkObject == null)
        {
            Debug.LogError("Player NetworkObject reference is missing!");
            return;
        }
        else
        {
            // Optionally disable buttons for non-local players to avoid confusion
            rollDiceButton.interactable = false;
            shopButton.interactable = false;
            endTurnButton.interactable = false;
            nextWeapon.interactable = false;
            prevButton.interactable = false;
        }
        BindToPlayer();
    }
    private void OnEnable()
    {
        RemoveListeners();
    }
    private void RemoveListeners()
    {
        rollDiceButton.onClick.RemoveAllListeners();
        endTurnButton.onClick.RemoveAllListeners();
        shopButton.onClick.RemoveAllListeners();
        nextWeapon.onClick.RemoveAllListeners();
        prevButton.onClick.RemoveAllListeners();
    }
    public void BindToPlayer()
    {
        rollDiceButton.onClick.AddListener(() =>
        {
            if (DiceRoller.Instance != null)
            {
                DiceRoller.Instance.RPC_RequestRollDice();
            }
        });
        endTurnButton.onClick.AddListener(() =>
        {
            if (GameSession.Instance != null)
            {
                GameSession.Instance.RPC_RequestEndTurn();
            }
        });
        shopButton.onClick.AddListener(() =>
        {
            if (ShopUIManager.Instance != null)
            {
                ShopUIManager.Instance.ToggleShop(true);
            }
        });
        nextWeapon.onClick.AddListener(() =>
        {
            if (playerNetworkObject != null)
                playerNetworkObject.GetComponent<WeaponManager>()?.SwitchToNextWeapon();
        });
        prevButton.onClick.AddListener(() =>
        {
            if (playerNetworkObject != null)
                playerNetworkObject.GetComponent<WeaponManager>()?.SwitchToPrevWeapon();
        });
        winMenuButton.onClick.AddListener(() =>
        {
            if (GameLoopFlowController.Instance != null)
            {
                GameLoopFlowController.Instance.HandleMenuClicked();
            }
        });
        loseMenuButton.onClick.AddListener(() =>
        {
            if (GameLoopFlowController.Instance != null)
            {
                GameLoopFlowController.Instance.HandleMenuClicked();
            }
        });
        menuQuitButton.onClick.AddListener(() => Application.Quit());
        quitButton.onClick.AddListener(() => Application.Quit());
        menuSettingsButton.onClick.AddListener(() =>
        {
            if (MainMenuController.Instance != null)
            {
                MainMenuController.Instance.HandleSettingsClicked();
            }
        });
        settingsButton.onClick.AddListener(() =>
        {
            if (IngameSettingsBinder.Instance != null)
            {
                IngameSettingsBinder.Instance.OpenSettingsMenu();
            }
        });
    }
}

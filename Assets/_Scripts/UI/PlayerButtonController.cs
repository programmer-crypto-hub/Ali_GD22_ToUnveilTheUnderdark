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
        if (rollDiceButton != null) rollDiceButton.onClick.RemoveAllListeners();
        if (endTurnButton != null) endTurnButton.onClick.RemoveAllListeners();
        if (shopButton != null) shopButton.onClick.RemoveAllListeners();
        if (nextWeapon != null) nextWeapon.onClick.RemoveAllListeners();
        if (prevButton != null) prevButton.onClick.RemoveAllListeners();
    }
    public void BindToPlayer()
    {
        if (rollDiceButton != null && DiceRoller.Instance != null)
        {
            rollDiceButton.onClick.AddListener(() =>
            {
                DiceRoller.Instance.RPC_RequestRollDice();
            });
        }
        if (endTurnButton != null && GameSession.Instance != null)
        {
            endTurnButton.onClick.AddListener(() =>
            {
                GameSession.Instance.RPC_RequestEndTurn();
            });
        }
        if (shopButton != null && ShopUIManager.Instance != null)
        {
            shopButton.onClick.AddListener(() =>
            {
                ShopUIManager.Instance.ToggleShop(true);
            });
        }
        if (nextWeapon != null && playerNetworkObject != null)
        {
            nextWeapon.onClick.AddListener(() =>
            {
                playerNetworkObject.GetComponent<WeaponManager>()?.SwitchToNextWeapon();
            });
        }
        if (prevButton != null && playerNetworkObject != null)
        {
            prevButton.onClick.AddListener(() =>
            {
                playerNetworkObject.GetComponent<WeaponManager>()?.SwitchToPrevWeapon();
            });
        }
        if (winMenuButton != null && GameLoopFlowController.Instance != null)
        {
            winMenuButton.onClick.AddListener(() =>
            {
                GameLoopFlowController.Instance.HandleMenuClicked();
            });
        }
        if (loseMenuButton != null && GameLoopFlowController.Instance != null)
        {
            loseMenuButton.onClick.AddListener(() =>
            {
                GameLoopFlowController.Instance.HandleMenuClicked();
            });
        }
        if (menuQuitButton != null)
        {
            menuQuitButton.onClick.AddListener(() => Application.Quit());
        }
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(() => Application.Quit());
        }
        if (menuSettingsButton != null && MainMenuController.Instance != null)
        {
            menuSettingsButton.onClick.AddListener(() =>
            {
                MainMenuController.Instance.HandleSettingsClicked();
            });
        }
        if (settingsButton != null && IngameSettingsBinder.Instance != null)
        {
            settingsButton.onClick.AddListener(() =>
            {
                IngameSettingsBinder.Instance.OpenSettingsMenu();
            });
        }
    }
}

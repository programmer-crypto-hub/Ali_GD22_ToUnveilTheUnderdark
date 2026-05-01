using Fusion;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Collections.Unicode;

public class PlayerButtonController : MonoBehaviour
{
    [Header("Buttons")]
    public Button rollDiceButton;
    public Button shopButton;
    public Button endTurnButton;

    private void OnEnable()
    {
        // Listen for the event we created earlier in the EventBus
        if (EventBus.Instance != null)
            EventBus.Instance.OnMapGenerated += HandleMapReady;
    }

    private void HandleMapReady()
    {
        // The map is generated! Now find the local player.
        // In Fusion 2, the local player is the one with InputAuthority.
        var runner = FindFirstObjectByType<NetworkRunner>();
        var localPlayer = runner.GetPlayerObject(runner.LocalPlayer).GetComponent<PlayerStats>();

        if (localPlayer != null)
        {
            RemoveListeners(); // Clear any old listeners to prevent duplicates
            BindToPlayer();
        }
    }
    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        if (EventBus.Instance != null)
            EventBus.Instance.OnMapGenerated -= HandleMapReady;
    }
    private void RemoveListeners()
    {
        rollDiceButton.onClick.RemoveAllListeners();
        endTurnButton.onClick.RemoveAllListeners();
        shopButton.onClick.RemoveAllListeners();
    }
    public void BindToPlayer()
    {
        rollDiceButton.onClick.AddListener(DiceRoller.Instance.RPC_RequestRollDice);
        endTurnButton.onClick.AddListener(GameSession.Instance.RPC_RequestEndTurn);
        shopButton.onClick.AddListener(() => ShopUIManager.Instance.ToggleShop(true));

        Debug.Log("UI Bound to local player after Edgar generation.");
    }
}

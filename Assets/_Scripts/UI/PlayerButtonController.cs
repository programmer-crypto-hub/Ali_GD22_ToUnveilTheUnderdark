using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class PlayerButtonController : MonoBehaviour
{
    [Header("Buttons")]
    public Button rollDiceButton;
    public Button shopButton;

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
            BindToPlayer(localPlayer);
        }
    }

    public void BindToPlayer(PlayerStats player)
    {
        // Clear and re-bind (to avoid double-subscriptions)
        rollDiceButton.onClick.RemoveAllListeners();
        rollDiceButton.onClick.AddListener(DiceManager.Instance.RPC_RequestRollDice);

        shopButton.onClick.AddListener(() => ShopUIManager.Instance.ToggleShop(true));

        Debug.Log("UI Bound to local player after Edgar generation.");
    }
}

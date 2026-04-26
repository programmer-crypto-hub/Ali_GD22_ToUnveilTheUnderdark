using Fusion;
using UnityEngine;
using static Unity.Collections.Unicode;

public class ShopSpaceTrigger : MonoBehaviour
{
    private bool _isPlayerInZone = false;

    private void OnEnable()
    {
        // Subscribe to the event
        if (InputManager.Instance != null)
            InputManager.Instance.OnInteractPressed += TryOpenShop;
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        if (InputManager.Instance != null)
            InputManager.Instance.OnInteractPressed -= TryOpenShop;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Use HasInputAuthority to ensure only the LOCAL player triggers this
        var networkObj = other.GetComponent<NetworkObject>();
        if (networkObj != null && networkObj.HasInputAuthority)
        {
            _isPlayerInZone = true;
            // Optional: Show "Press E" UI here
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var networkObj = other.GetComponent<NetworkObject>();
        if (networkObj != null && networkObj.HasInputAuthority)
        {
            _isPlayerInZone = false;
            ShopUIManager.Instance.ToggleShop(false);
        }
    }

    private void TryOpenShop()
    {
        if (!_isPlayerInZone) return;
        var Runner = GameSession.Instance.Runner;
        // Fusion 2 uses Runner.LocalPlayer to identify 'you'
        if (GameSession.Instance.CurrentTurnPlayer != Runner.LocalPlayer)
        {
            Debug.Log("It is not your turn to trade!");
            // Optional: Trigger a "Wait for your turn" UI message here
            return;
        }

        // If both pass, open the UI
        ShopUIManager.Instance.ToggleShop(true);
    }
}

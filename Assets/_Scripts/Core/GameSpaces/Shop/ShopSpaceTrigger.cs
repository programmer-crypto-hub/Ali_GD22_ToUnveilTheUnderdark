using Fusion;
using UnityEngine;
using static Unity.Collections.Unicode;

public class ShopSpaceTrigger : MonoBehaviour
{
    private bool _isPlayerInZone = false;

    private void OnEnable()
    {
        if (InputManager.Instance == null) Debug.LogError("InputManager instance not found! Make sure it is in the scene and properly initialized.");
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
            if (ShopUIManager.Instance == null) Debug.LogError("ShopUIManager instance not found! Make sure it is in the scene and properly initialized.");
            ShopUIManager.Instance.ToggleShop(false);
        }
    }

    private void TryOpenShop()
    {
        if (!_isPlayerInZone) return;
        var Runner = FindFirstObjectByType<NetworkRunner>();
        if (GameSession.Instance == null) Debug.LogError("GameSession instance not found! Make sure it is in the scene and properly initialized.");
        // Fusion 2 uses Runner.LocalPlayer to identify 'you'
        if (GameSession.Instance.CurrentTurnPlayer != Runner.LocalPlayer)
        {
            Debug.Log("It is not your turn to trade!");
            // Optional: Trigger a "Wait for your turn" UI message here
            return;
        }

        // If both pass, open the UI
        if (ShopUIManager.Instance == null) Debug.LogError("ShopUIManager instance not found! Make sure it is in the scene and properly initialized.");
        ShopUIManager.Instance.ToggleShop(true);
    }
}

using Fusion;
using UnityEngine;

public class ShopSpaceTrigger : MonoBehaviour
{
    // We only want to trigger the UI for the player who actually stepped here
    private void OnTriggerEnter2D(Collider2D other)
    {
        var networkObj = other.GetComponent<NetworkObject>();

        // Check if the object that entered is a Player and if WE control it
        if (networkObj != null && networkObj.HasInputAuthority)
        {
            Debug.Log("Local player entered shop space!");
            // Call your UI Manager to turn on the Shop Canvas
            ShopUIManager.Instance.ToggleShop(true);
        }
    }
}
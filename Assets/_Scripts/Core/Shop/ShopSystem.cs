using Fusion;
using UnityEngine;

public class ShopSystem : NetworkBehaviour
{
    // The Client calls this, but the logic runs on the Host (StateAuthority)
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestPurchase(int itemCost, int requiredRole)
    {
        // 'Object.InputAuthority' tells the Host which player clicked 'Buy'
        var player = Runner.GetPlayerObject(Object.InputAuthority).GetComponent<PlayerData>();

        if (player.currentPlayerCaveCoins >= itemCost && PlayerRolesController.Instance.ReturnRoleId() == requiredRole)
        {
            player.currentPlayerCaveCoins -= itemCost;
            Debug.Log("Purchase Successful on Server!");
            // Add item to inventory logic here
        }
    }
}

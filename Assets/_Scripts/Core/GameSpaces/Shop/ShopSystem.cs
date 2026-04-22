using Fusion;
using UnityEngine;
using static Unity.Collections.Unicode;

public class ShopSystem : NetworkBehaviour
{
    [SerializeField] private ItemDatabase masterDatabase;

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestPurchase(int itemID)
    {
        // 1. Turn Check
        if (Object.InputAuthority != GameSession.Instance.CurrentTurnPlayer) return;

        // 2. Lookup item data from the SO Database
        ShopItem item = masterDatabase.GetItemByID(itemID);
        if (item == null) return;

        // 3. Get the Player's Networked component
        var player = Runner.GetPlayerObject(Object.InputAuthority).GetComponent<PlayerStats>();

        // 4. Validate Funds & Space
        bool hasMoney = player.Gold >= item.cost;
        bool hasSpace = false;
        int emptySlotIndex = -1;

        // Find first empty slot (assuming 0 is 'empty' or use -1)
        for (int i = 0; i < player.InventoryItemIDs.Length; i++)
        {
            if (player.InventoryItemIDs[i] == 0)
            {
                emptySlotIndex = i;
                hasSpace = true;
                break;
            }
        }

        if (hasMoney && hasSpace)
        {
            player.Gold -= item.cost;
            player.InventoryItemIDs.Set(emptySlotIndex, itemID); // NetworkArray requires .Set()

            Debug.Log($"Server: {player.PlayerName} bought {item.itemName}");
        }
    }
}

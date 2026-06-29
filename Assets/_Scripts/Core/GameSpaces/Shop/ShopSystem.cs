using Fusion;
using UnityEngine;
using static Unity.Collections.Unicode;

public class ShopSystem : NetworkBehaviour
{
    [SerializeField] private ItemDatabase masterDatabase;

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestPurchase(int itemID)
    {
        if (!HasStateAuthority) return;

        var player = Runner.GetPlayerObject(Object.InputAuthority).GetComponent<PlayerStats>();
        if (player == null || GameSession.Instance == null) return;

        // FIX: Verify current session turn using the verified integer Network ID match!
        if (GameSession.Instance.CurrentTurnID != (int)player.Object.Id.Raw) return;

        ShopItem item = masterDatabase.GetItemByID(itemID);
        if (item == null) return;

        bool hasMoney = player.Gold >= item.cost;
        bool hasSpace = false;
        int emptySlotIndex = -1;

        // Process free slot lookup across the 30 NetworkArray spaces
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
            player.InventoryItemIDs.Set(emptySlotIndex, itemID);

            Debug.LogWarning($"{player.gameObject.name} successfully purchased {item.itemName}!");

            // Force the Inventory layout UI on the player's screen to instantly re-render with the new item icon!
            var invUI = FindFirstObjectByType<InventoryUI>();
            if (invUI != null) invUI.RefreshUI();
        }
    }

}

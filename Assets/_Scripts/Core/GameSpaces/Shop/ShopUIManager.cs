using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class ShopUIManager : NetworkBehaviour
{
    public static ShopUIManager Instance;

    [SerializeField] public GameObject shopPanel;
    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject itemPrefab; // UI button with an icon/text
    [SerializeField] private ItemDatabase masterDatabase;

    public List<ShopItem> allItems; // Drag all shop items here in Inspector

    private bool _isCurrentlySubscribed = false;
    private void Awake() => Instance = this;

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);

        // Clean up using the corrected boolean tracker
        if (GameSession.Instance != null && _isCurrentlySubscribed)
        {
            GameManager.Events.OnTurnChanged -= HandleTurnChanged;
            _isCurrentlySubscribed = false;
        }
    }

    private void HandleTurnChanged(int currentTurnId)
    {
        if (currentTurnId != Runner.LocalPlayer.PlayerId)
        {
            ToggleShop(false);
        }
    }
    public void HandleShopToggleInput()
    {
        Debug.Log($"HandleShopToggleInput called! Did the shop open?" + (shopPanel.activeSelf ? "Yes" : "No"));
        if (shopPanel == null) return;

        bool targetState = !shopPanel.activeSelf;
        ToggleShop(targetState);
    }
    public void ToggleShop(bool isOpen)
    {
        // FIX: Match your explicit Network ID turn system!
        var runner = FindFirstObjectByType<NetworkRunner>();
        if (runner == null || GameSession.Instance == null) return;

        var playerObj = runner.GetPlayerObject(runner.LocalPlayer);
        if (playerObj == null) return;

        var localPlayerStats = playerObj.GetComponent<PlayerStats>();

        // Local check to prevent UI from opening out of turn using your verified Object.Id.Raw structure
        if (isOpen && GameSession.Instance.CurrentTurnID != (int)localPlayerStats.Object.Id.Raw)
        {
            Debug.LogWarning("[SHOP SECURITY] It's not your turn to shop!");
            return;
        }

        shopPanel.SetActive(isOpen);
        if (isOpen) RefreshShop();
    }

    private void RefreshShop()
    {
        // 1. COLLECT STALE CLONES (Leaving the original editor template if it matches)
        // To ensure clean rendering, we rename our base template or skip it if it's named "TemplateSlot"
        foreach (Transform child in itemContainer)
        {
            // If it's a clone from a previous open, destroy it instantly!
            if (child.gameObject.name.Contains("(Clone)"))
            {
                Destroy(child.gameObject);
            }
        }

        var runner = FindFirstObjectByType<NetworkRunner>();
        var playerObj = runner.GetPlayerObject(runner.LocalPlayer);
        var localPlayer = playerObj.GetComponent<PlayerStats>();

        int itemIndex = 0;

        foreach (var item in allItems)
        {
            GameObject btnObj = null;

            // 2. THE TEMPLATE HIJACK HOOK
            // Check if there is an existing design template slot sitting inside the grid container unassigned
            if (itemContainer.childCount > 0 && itemIndex == 0)
            {
                Transform existingTemplate = itemContainer.GetChild(0);

                // If it's not a clone, hijack it!
                if (!existingTemplate.gameObject.name.Contains("(Clone)"))
                {
                    btnObj = existingTemplate.gameObject;
                    btnObj.name = $"ShopSlot_{item.itemName}";
                }
            }

            // If no template is available to hijack, spawn a clean new prefab button slot
            if (btnObj == null)
            {
                btnObj = Instantiate(itemPrefab, itemContainer);
                btnObj.name = $"ShopSlot_{item.itemName} (Clone)";
            }

            ShopItemSlot slot = btnObj.GetComponent<ShopItemSlot>();
            if (slot == null)
            {
                Debug.LogError($"[SHOP ERROR] ShopItemSlot component is missing on: {btnObj.name}");
                continue;
            }

            // 3. OVERWRITE ALL GRAPHICS & VALUES DYNAMICALLY FROM THE S.O. DATA!
            slot.iconImage.sprite = item.icon;
            slot.nameText.text = item.itemName;
            slot.costText.text = $"{item.cost}g";

            // Verification checks against player profile data parameters
            bool canAfford = localPlayer.Gold >= item.cost;
            bool roleMatch = localPlayer.CurrentRoleId == item.requiredRole;
            bool levelMatch = localPlayer.currentPlayerLevel >= item.requiredLevel;

            slot.buyButton.interactable = canAfford && roleMatch && levelMatch;

            if (!levelMatch)
            {
                if (slot.lockOverlay != null) slot.lockOverlay.SetActive(true);
                if (slot.lockLevelText != null) slot.lockLevelText.text = $"Reach Level {item.requiredLevel}";
                slot.buyButton.interactable = false;
            }
            else
            {
                if (slot.lockOverlay != null) slot.lockOverlay.SetActive(false);
            }

            slot.Setup(item, canAfford, roleMatch);

            // Clear duplicated runtime button events to prevent click memory stacking
            slot.buyButton.onClick.RemoveAllListeners();

            slot.buyButton.onClick.AddListener(() => {
                var shopSystem = localPlayer.GetComponent<ShopSystem>();
                if (shopSystem != null)
                {
                    shopSystem.RPC_RequestPurchase(item.itemID);
                    Debug.LogWarning($"Requested item ID purchase: {item.itemID}");
                }
                ToggleShop(false); // Close shop canvas pane seamlessly after clicking
            });

            itemIndex++;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestPurchase(int itemID)
    {
        if (!HasStateAuthority) return;

        var player = Runner.GetPlayerObject(Object.InputAuthority).GetComponent<PlayerStats>();
        if (player == null || GameSession.Instance == null) return;

        // Turn security check
        if (GameSession.Instance.CurrentTurnID != (int)player.Object.Id.Raw) return;

        ShopItem item = null;

        if (masterDatabase != null)
        {
            item = masterDatabase.GetItemByID(itemID);
        }
        else
        {
            // EMERGENCY FALLBACK: Search your assigned list of catalog templates directly!
            item = allItems.Find(x => x.itemID == itemID);
        }

        if (item == null)
        {
            Debug.LogError($"[SHOP ERROR] Item ID {itemID} could not be found anywhere in the databases!");
            return;
        }

        // Validate Funds & Inventory Grid Space
        bool hasMoney = player.Gold >= item.cost;
        bool hasSpace = false;
        int emptySlotIndex = -1;

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
            // Subtract currency values from network wallet counters
            player.Gold -= item.cost;

            // Write the item integer code into the network slot array
            player.InventoryItemIDs.Set(emptySlotIndex, itemID);

            Debug.LogWarning($"[SERVER SUCCESS] Bought: {item.itemName}. Remaining Gold: {player.Gold}");

            // Force the local Canvas layout grid UI to update and render the new icons immediately!
            RPC_ForceInventoryUIRefresh();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ForceInventoryUIRefresh()
    {
        var invUI = FindFirstObjectByType<InventoryUI>();
        if (invUI != null)
        {
            // Use your working double-toggle method to clean and redraw the icons
            invUI.ToggleInventoryExpansion();
            invUI.ToggleInventoryExpansion();
        }
    }
}
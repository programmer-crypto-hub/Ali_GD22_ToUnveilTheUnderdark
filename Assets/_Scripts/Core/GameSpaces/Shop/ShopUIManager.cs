using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class ShopUIManager : NetworkBehaviour
{
    public static ShopUIManager Instance;

    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject itemPrefab; // UI button with an icon/text
    [SerializeField] private ItemDatabase masterDatabase;

    public List<ShopItem> allItems; // Drag all shop items here in Inspector

    private void Awake() => Instance = this;

    private void OnEnable()
    {
        if (GameSession.Instance != null)
            GameSession.Instance.OnTurnChanged += HandleTurnChanged;
    }

    private void HandleTurnChanged()
    {
        // If the turn is no longer mine, force the shop to close
        if (GameSession.Instance.CurrentTurnPlayer != Runner.LocalPlayer)
        {
            ToggleShop(false);
        }
    }
    public void ToggleShop(bool isOpen)
    {
        // Local check to prevent UI from opening out of turn
        if (isOpen && Runner.LocalPlayer != GameSession.Instance.CurrentTurnPlayer)
        {
            Debug.Log("It's not your turn to shop!");
            return;
        }

        shopPanel.SetActive(isOpen);
        if (isOpen) RefreshShop();
    }

    private void RefreshShop()
    {
        foreach (Transform child in itemContainer) Destroy(child.gameObject);

        // Get local player via Fusion 2 Runner
        var localPlayer = Runner.GetPlayerObject(Runner.LocalPlayer).GetComponent<PlayerStats>();
        int myRole = PlayerRolesController.Instance.ReturnRoleId();

        foreach (var item in allItems)
        {
            GameObject btnObj = Instantiate(itemPrefab, itemContainer);
            ShopItemSlot slot = btnObj.GetComponent<ShopItemSlot>();

            slot.iconImage.sprite = item.icon;
            slot.nameText.text = item.itemName;
            slot.costText.text = $"{item.cost}g";

            bool canAfford = localPlayer.Gold >= item.cost;
            bool roleMatch = localPlayer.CurrentRoleId == item.requiredRole;
            bool levelMatch = localPlayer.currentPlayerLevel >= item.requiredLevel;

            slot.buyButton.interactable = canAfford && roleMatch && levelMatch;
            slot.lockOverlay.SetActive(!levelMatch);

            if (localPlayer.currentPlayerLevel < item.requiredLevel)
            {
                slot.lockOverlay.SetActive(true);
                slot.lockLevelText.text = $"Reach Level {item.requiredLevel}";
                slot.buyButton.interactable = false;
            }
            else
            {
                slot.lockOverlay.SetActive(false);
            }

            bool levelMet = localPlayer.currentPlayerLevel >= item.requiredLevel;
            slot.lockOverlay.SetActive(!levelMet);
            if (!levelMet) slot.lockLevelText.text = $"Level {item.requiredLevel}";
            slot.Setup(item, canAfford, roleMatch);

            slot.buyButton.onClick.AddListener(() => {
                localPlayer.GetComponent<ShopSystem>().RPC_RequestPurchase(item.itemID);
                ToggleShop(false); // Close shop after buying
            });
        }
        foreach (int id in localPlayer.InventoryItemIDs)
        {
            if (id == 0) continue; // Skip empty slots

            // Get the visual data from your Master Database
            ShopItem item = masterDatabase.GetItemByID(id);

            // Instantiate a simple icon prefab
            GameObject icon = Instantiate(itemPrefab, itemContainer);
            icon.GetComponent<Image>().sprite = item.icon;

            // Add a tooltip trigger here so you can see the description!
        }
    }
}
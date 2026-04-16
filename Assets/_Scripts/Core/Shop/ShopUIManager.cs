using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShopUIManager : MonoBehaviour
{
    public static ShopUIManager Instance;

    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject itemPrefab; // UI button with an icon/text

    public List<ShopItem> allItems; // Drag all shop items here in Inspector

    private void Awake() => Instance = this;

    public void ToggleShop(bool isOpen)
    {
        shopPanel.SetActive(isOpen);
        if (isOpen) RefreshShop();
    }

    private void RefreshShop()
    {
        // Clear old buttons
        foreach (Transform child in itemContainer) Destroy(child.gameObject);

        // Find the local player's data 
        var localPlayer = GameObject.FindWithTag("Player").GetComponent<PlayerData>();

        foreach (var item in allItems)
        {
            GameObject btnObj = Instantiate(itemPrefab, itemContainer);
            Button btn = btnObj.GetComponent<Button>();

            // ROLE LOCK: Disable button if the role doesn't match
            bool canBuy = (int)PlayerRolesController.Instance.ReturnRoleName() == item.requiredRole && localPlayer.currentPlayerCaveCoins >= item.cost;
            btn.interactable = canBuy;

            // Handle the click: Link it to the Player's RPC logic
            btn.onClick.AddListener(() => {
                localPlayer.GetComponent<ShopSystem>().RPC_RequestPurchase(item.cost, item.requiredRole);
                ToggleShop(false); // Close shop after purchase
            });
        }
    }
}
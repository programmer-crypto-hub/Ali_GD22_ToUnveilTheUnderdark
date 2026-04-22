using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public GameObject iconPrefab; // A simple UI Image prefab
    public Transform container;
    public ItemDatabase database;

    public void OpenInventory()
    {
        gameObject.SetActive(true);
        RefreshUI();
    }

    private void RefreshUI()
    {
        // 1. Clear old icons
        foreach (Transform child in container) Destroy(child.gameObject);

        // 2. Get the local player
        var runner = FindFirstObjectByType<NetworkRunner>();
        var localPlayer = runner.GetPlayerObject(runner.LocalPlayer).GetComponent<PlayerStats>();

        // 3. Loop through your [Networked] Array
        foreach (int id in localPlayer.InventoryItemIDs)
        {
            if (id == 0) continue; // Skip empty slots

            // 4. Create the icon using the Database
            ShopItem item = database.GetItemByID(id);
            GameObject iconObj = Instantiate(iconPrefab, container);
            iconObj.GetComponent<Image>().sprite = item.icon;

            // Optional: Add a button to the icon to show tooltips!
        }
    }
}

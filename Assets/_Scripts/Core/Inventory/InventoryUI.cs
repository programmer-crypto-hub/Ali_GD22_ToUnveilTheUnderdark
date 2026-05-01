using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Data References")]
    public ItemDatabase database;

    [Header("UI Panels")]
    public GameObject upperGridPanel;  // Your 30-slot background
    public GameObject lowerBarPanel;   // Your 10-slot hotbar at the bottom

    [Header("Prefabs & Parents")]
    public GameObject slotPrefab; // A simple blank UI image prefab
    public Transform gridContainer; // The GameObject with the Grid Layout Group

    [Header("Static References")]
    [Tooltip("Keep manually dragging your 10 lower hotbar images here!")]
    public Image[] lowerBarSlots;

    // This array will hold the 30 spawned upper images automatically
    private Image[] _upperGridSlots = new Image[30];
    private bool _hasGeneratedGrid = false;

    public enum InventoryVisualState
    {
        DefaultLower,
        Expanded
    }
    public InventoryVisualState currentState = InventoryVisualState.DefaultLower;

    private bool _isExpanded = false;
    private void Start()
    {
        GenerateUpperGrid();
    }

    private void GenerateUpperGrid()
    {
        if (_hasGeneratedGrid) return;

        for (int i = 0; i < 30; i++)
        {
            // 1. Instantiate the blank icon
            GameObject newSlot = Instantiate(slotPrefab, gridContainer);

            // 2. Name it nicely for debugging
            newSlot.name = $"Slot_{i}";

            // 3. Grab the Image component and save it to our array
            _upperGridSlots[i] = newSlot.GetComponent<Image>();
        }

        _hasGeneratedGrid = true;
    }

    public void ToggleInventoryExpansion()
    {
        // Simple toggle switch
        currentState = (currentState == InventoryVisualState.DefaultLower)
            ? InventoryVisualState.Expanded
            : InventoryVisualState.DefaultLower;

        RefreshUI();
    }

    private void RefreshUI()
    {
        var runner = FindFirstObjectByType<NetworkRunner>();
        if (runner == null) return;

        var playerObj = runner.GetPlayerObject(runner.LocalPlayer);
        if (playerObj == null) return;

        var localPlayerStats = playerObj.GetComponent<PlayerStats>();

        // CLEAN SWITCH CASE: Managing UI displays and index mappings
        switch (currentState)
        {
            case InventoryVisualState.DefaultLower:
                upperGridPanel.SetActive(false);
                lowerBarPanel.SetActive(true);

                for (int i = 0; i < lowerBarSlots.Length; i++)
                {
                    // Pulls strictly from network indices 20 to 29
                    int itemID = localPlayerStats.InventoryItemIDs[20 + i];
                    UpdateSlotVisual(lowerBarSlots[i], itemID);
                }
                break;

            case InventoryVisualState.Expanded:
                upperGridPanel.SetActive(true);
                lowerBarPanel.SetActive(false);

                for (int i = 0; i < _upperGridSlots.Length; i++)
                {
                    int itemID;

                    // Classic shift: The first 10 slots of the upper grid 
                    // grab data from the hotbar's network indices (20-29)
                    if (i < 10)
                    {
                        itemID = localPlayerStats.InventoryItemIDs[20 + i];
                    }
                    else
                    {
                        // The remaining 20 slots pull from indices 0-19
                        itemID = localPlayerStats.InventoryItemIDs[i - 10];
                    }

                    UpdateSlotVisual(_upperGridSlots[i], itemID);
                }
                break;
        }
    }

    private void UpdateSlotVisual(Image slotImage, int itemID)
    {
        if (itemID == 0)
        {
            slotImage.enabled = false;
            slotImage.sprite = null;
        }
        else
        {
            ShopItem item = database.GetItemByID(itemID);
            if (item != null)
            {
                slotImage.sprite = item.icon;
                slotImage.enabled = true;
            }
        }
    }
}

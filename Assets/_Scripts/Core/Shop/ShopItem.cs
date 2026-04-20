using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewShopItem", menuName = "Shop/Item")]
public class ShopItem : ScriptableObject
{
    public int itemID;
    public string itemName;
    [TextArea] public string description; // For the tooltip
    public int cost;
    public Sprite icon;
    public int requiredRole;
    public int requiredLevel;
}

// ItemDatabase.cs - The Master List
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Shop/Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ShopItem> allItems;

    public ShopItem GetItemByID(int id)
    {
        return allItems.Find(item => item.itemID == id);
    }
}
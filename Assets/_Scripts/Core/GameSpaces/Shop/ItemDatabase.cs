// ItemDatabase.cs - The Master List
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Shop/Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ShopItem> allItems;

    public ShopItem GetItemByID(int id)
    {
        return allItems.Find(item => item.itemID == id);
    }
}
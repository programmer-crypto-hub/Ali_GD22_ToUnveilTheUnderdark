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
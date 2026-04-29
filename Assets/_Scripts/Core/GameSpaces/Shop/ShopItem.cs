using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewShopItem", menuName = "Shop/Item")]
public class ShopItem : ScriptableObject
{
    public static ShopItem Instance { get; private set; }
    public int itemID;
    public string itemName;
    [TextArea] public string description; // For the tooltip
    public int cost;
    public Sprite icon;
    public int requiredRole;
    // If value is 0, then all Roles can access it (default)
    // Else, only the Role with that ID can access it (PlayerRoles.RoleType enum) (1 or 2)
    // Only Mage and Medic should have limited items (medkits, potions)
    public int requiredLevel = 0;
}
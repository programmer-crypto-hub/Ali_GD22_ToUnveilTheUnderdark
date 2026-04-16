using UnityEngine;

[CreateAssetMenu(fileName = "NewShopItem", menuName = "Shop/Item")]
public class ShopItem : ScriptableObject
{
    public string itemName;
    public int cost;
    public Sprite icon;
    public int requiredRole; // 0=Warrior, 1=Mage, etc.
}
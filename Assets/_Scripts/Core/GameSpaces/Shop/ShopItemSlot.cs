using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ShopItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static ShopItemSlot Instance; // Singleton for easy access in PlayerButtonController

    public Image iconImage;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI nameText;
    public Button buyButton;

    public GameObject lockOverlay;
    public TextMeshProUGUI lockLevelText;

    private ShopItem itemData;

    public void Setup(ShopItem item, bool canAfford, bool correctRole)
    {
        itemData = item;
        iconImage.sprite = item.icon;
        nameText.text = item.itemName;
        costText.text = $"{item.cost}g";

        bool valid = canAfford && correctRole;
        buyButton.interactable = valid;

        // Visual feedback for wrong role
        if (!correctRole)
        {
            iconImage.color = new Color(1, 0, 0, 0.5f); // Tint red/transparent
            nameText.color = Color.gray;
        }
        else if (!canAfford)
        {
            costText.color = Color.red; // Highlight price if too expensive
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // itemData is the ScriptableObject assigned in Setup()
        TooltipSystem.Instance.Show(itemData.description, itemData.itemName);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipSystem.Instance.Hide();
    }
}
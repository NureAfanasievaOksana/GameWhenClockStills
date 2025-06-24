using UnityEngine;

public class ItemHover : MonoBehaviour
{
    public string itemId;

    private ItemTooltipController tooltipController;
    private ItemsDatabase database;

    void Start()
    {
        tooltipController = FindAnyObjectByType<ItemTooltipController>();
        database = ItemsDatabase.LoadFromResources();
    }

    void OnMouseEnter()
    {
        ItemData item = database.GetItemById(itemId);
        if (item != null)
        {
            tooltipController.ShowTooltip(item.name);
        }
        else
        {
            Debug.LogWarning($"Item with ID {itemId} not found in database!");
        }
    }

    void OnMouseExit()
    {
        tooltipController.HideTooltip();
    }
}
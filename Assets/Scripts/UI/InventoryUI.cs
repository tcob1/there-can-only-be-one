using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Inventory inventory;

    [Header("UI Slots")]
    public Image[] slotImages;
    public Image activeSlotHighlight;

    //private void Update()
    //{
    //    RefreshUI();
    //}

    private void OnEnable()
    {
        if (inventory != null)
            inventory.OnInventoryChanged += RefreshUI; // subscribe
    }

    private void OnDisable()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= RefreshUI; // unsubscribe
    }

    private void RefreshUI()
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            InventorySlot slot = inventory.itemSlots[i];

            if (!slot.IsEmpty() && slot.itemData != null)
            {
                slotImages[i].sprite = slot.itemData.icon;
                slotImages[i].color = Color.white;
            }
            else
            {
                slotImages[i].sprite = null;
                slotImages[i].color = new Color(1, 1, 1, 0f);
            }
        }

        // highlight active slot
        if (inventory.activeSlotIndex >= 0 && inventory.activeSlotIndex < slotImages.Length)
        {
            activeSlotHighlight.transform.position = slotImages[inventory.activeSlotIndex].transform.position;
            activeSlotHighlight.enabled = true;
        }
        else
        {
            activeSlotHighlight.enabled = false;
        }
    }
}
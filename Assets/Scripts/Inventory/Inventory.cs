using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class InventorySlot
{
    public ItemData itemData;
    public int quantity;

    public InventorySlot()
    {
        itemData = null;
        quantity = 0;
    }

    public InventorySlot(ItemData itemData, int quantity)
    {
        this.itemData = itemData;
        this.quantity = quantity;
    }
}


public class Inventory : MonoBehaviour
{
    public InventorySlot weaponSlot;
    public List<InventorySlot> itemSlots = new();

    public bool AddItem(ItemData itemData)
    {
        // Check if the item can be stacked with an existing slot.
        foreach (InventorySlot slot in itemSlots)
        {
            if (slot.itemData == itemData && slot.quantity < itemData.maxStackSize)
            {
                slot.quantity++;
                return true;
            }
        }

        // If not, add it to a new slot.
        InventorySlot newSlot = new(itemData, 1);
        itemSlots.Add(newSlot);
        return true;
    }

    public void DropItem(int slotIndex, Vector3 dropPosition)
    {
        if (slotIndex < 0 || slotIndex >= itemSlots.Count)
        {
            Debug.LogWarning("Invalid slot index!");
            return;
        }

        InventorySlot slot = itemSlots[slotIndex];
        if (slot.itemData == null || slot.quantity <= 0)
        {
            Debug.LogWarning("No item to drop in this slot!");
            return;
        }

        // Instantiate the world prefab at the drop position.
        Instantiate(slot.itemData.worldPrefab, dropPosition, Quaternion.identity);

        // Decrease the quantity or remove the slot if it was the last item.
        slot.quantity--;
        if (slot.quantity <= 0)
        {
            itemSlots.RemoveAt(slotIndex);
        }
    }

    public bool HasItem(ItemData itemData)
    {
        foreach (InventorySlot slot in itemSlots)
        {
            if (slot.itemData == itemData && slot.quantity > 0)
            {
                return true;
            }
        }
        return false;
    }

    public bool HasItem(string itemName)
    {
        foreach (InventorySlot slot in itemSlots)
        {
            if (slot.itemData != null && slot.itemData.itemName == itemName && slot.quantity > 0)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsEmpty()
    {
        return itemSlots.Count == 0 && (weaponSlot.itemData == null || weaponSlot.quantity <= 0);
    }
}

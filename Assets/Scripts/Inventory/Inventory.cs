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

    public Transform rightHoldPosition;
    public Transform leftHoldPosition;

    // to track items in ur hands
    private Dictionary<InventorySlot, GameObject> heldItems = new();

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

        // Decide which hand to use
        Transform targetHand = null;

        bool rightOccupied = false;
        bool leftOccupied = false;

        foreach (var held in heldItems.Values)
        {
            if (held.transform.parent == rightHoldPosition)
                rightOccupied = true;
            if (held.transform.parent == leftHoldPosition)
                leftOccupied = true;
        }

        if (!rightOccupied)
            targetHand = rightHoldPosition;
        else if (!leftOccupied)
            targetHand = leftHoldPosition;
        else
        {
            Debug.Log("hands full! Cant pick up: " + itemData.itemName);
            return false;
        }

        // Instantiate item in chosen hand
        GameObject heldItem = null;
        if (itemData.worldPrefab != null)
        {
            heldItem = Instantiate(itemData.worldPrefab, targetHand);
            WorldItem worldItemComponent = heldItem.GetComponent<WorldItem>();
            if (worldItemComponent != null)
                worldItemComponent.isHeld = true;

            heldItem.transform.localPosition = Vector3.zero;
            heldItem.transform.localRotation = Quaternion.identity;
        }

        // Add to inventory slot
        InventorySlot newSlot = new(itemData, 1);
        itemSlots.Add(newSlot);

        if (heldItem != null)
            heldItems[newSlot] = heldItem;

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

        if (heldItems.TryGetValue(slot, out GameObject heldInstance))
        {
            // Move it to the drop position
            heldInstance.transform.parent = null;
            heldInstance.transform.position = dropPosition;
            WorldItem worldItemComponent = heldInstance.GetComponent<WorldItem>();
            if (worldItemComponent != null)
            {
                worldItemComponent.isHeld = false;
            }

            // Remove reference from dictionary
            heldItems.Remove(slot);
        }
        else
        {
            // If we somehow didn’t have the instance, instantiate a new one
            Instantiate(slot.itemData.worldPrefab, dropPosition, Quaternion.identity);
        }

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

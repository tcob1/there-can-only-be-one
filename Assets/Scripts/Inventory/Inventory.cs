using UnityEngine;
using System;
using System.Collections.Generic;
using NUnit.Framework.Internal.Execution;

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

    // checks specific item slot
    public bool IsEmpty()
    {
        return itemData == null || quantity <= 0;
    }

    // clears item slot
    public void Clear()
    {
        itemData = null;
        quantity = 0;
    }
}


public class Inventory : MonoBehaviour
{
    private const int NUM_ITEM_SLOTS = 5;

    //public InventorySlot weaponSlot;
    public InventorySlot[] itemSlots;


    public Transform rightHoldPosition;

    public GameObject currentHeldItem;

    [SerializeField] private CooldownBar cooldownBar;

    // which slot is equipped
    public int activeSlotIndex = -1;

    public bool isGuard = false;
    public GuardNav guardNav;

    public ItemData[] startItems;

    [Header("Events")]
    public Action OnInventoryChanged;


    private void Awake()
    {
        // inventory has fixed items
        itemSlots = new InventorySlot[NUM_ITEM_SLOTS];

        //initialize the fixed # of slots
        for (int i = 0; i < itemSlots.Length; i++)
        {
            itemSlots[i] = new InventorySlot();
        }

        if (isGuard)
        {
            for (int i = 0; i < startItems.Length; i++)
            {
                AddItem(startItems[i]);
            }
        }
    }

    public bool AddItem(ItemData itemData)
    {
        // Try stacking first
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (!itemSlots[i].IsEmpty() &&
                itemSlots[i].itemData == itemData &&
                itemSlots[i].quantity < itemData.maxStackSize)
            {
                itemSlots[i].quantity++;

                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        // Find empty slot
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i].IsEmpty())
            {
                itemSlots[i].itemData = itemData;
                itemSlots[i].quantity = 1;

                if (!isGuard)
                {
                    activeSlotIndex = i;
                    EquipSlot(i);
                }
                OnInventoryChanged?.Invoke();

                return true;
            }
        }

        Debug.Log("Inventory full!");
        return false;
    }

    // equip item
    public void EquipSlot(int index)
    {
        if (index < 0 || index >= itemSlots.Length)
            return;


        //get rid of item currently in hand
        if (currentHeldItem != null)
        {
            Destroy(currentHeldItem);
        }

        activeSlotIndex = index;

        // Instantiate new held object
        if (!itemSlots[index].IsEmpty())
        {
            currentHeldItem = Instantiate(itemSlots[index].itemData.worldPrefab, rightHoldPosition);
            if (currentHeldItem.name == "Pistol(Clone)" && isGuard)
            {
                Gun GunScript = currentHeldItem.GetComponent<Gun>();
                GunScript.guard = guardNav;
            }

            if (!isGuard)
            {
                currentHeldItem.transform.localPosition = Vector3.zero;
            }

            // Set world item to held state so it can adjust scale and disable interactions
            WorldItem worldItemComponent = currentHeldItem.GetComponent<WorldItem>();
            if (worldItemComponent != null && gameObject.tag == "Player")
            {
                worldItemComponent.isHeld = true;
            }

            

            //add cooldown bar if equipping a weapon
            if (gameObject.tag == "Player")
            {
                // player cant repickup while in hand
                Interactable interactable = currentHeldItem.GetComponent<Interactable>();
                if (interactable != null)
                {
                    interactable.enabled = false;
                    Collider col = currentHeldItem.GetComponent<Collider>();
                    if (col != null) col.enabled = false;
                }
                // Show the name of the equipped item UI
                UIManager.Instance.ShowEquipText(itemSlots[index].itemData.itemName);

                Weapon weapon = currentHeldItem.GetComponent<Weapon>();
                if (weapon != null && cooldownBar != null)
                {
                    cooldownBar.Bind(weapon);
                }
            }

        }

        OnInventoryChanged?.Invoke();
    }

    // function for guards to equip items by name like switching from gun to keys and vice versa
    public void GuardEquipByName(string itemName)
    {
        if (!isGuard) return;

        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (!itemSlots[i].IsEmpty() &&
                 itemSlots[i].itemData.itemName == itemName)
            {
                Debug.Log("Equipping: " +  itemName);
                EquipSlot(i);
                return;
            }
        }
    }

    public void DropItem(Vector3 dropPosition)
    {
        if (activeSlotIndex == -1)
            return;

        InventorySlot slot = itemSlots[activeSlotIndex];

        if (slot.IsEmpty())
            return;

        if (currentHeldItem != null)
        {
            Interactable interactable = currentHeldItem.GetComponent<Interactable>();
            if (interactable != null)
            {
                interactable.enabled = true;
                Collider col = currentHeldItem.GetComponent<Collider>();
                if (col != null) col.enabled = true;
            }

            currentHeldItem.transform.parent = null;
            currentHeldItem.transform.position = dropPosition;

            WorldItem worldItemComponent = currentHeldItem.GetComponent<WorldItem>();
            if (worldItemComponent != null)
                worldItemComponent.isHeld = false;
                worldItemComponent.SetToWorldScale();

            currentHeldItem = null;
        }
        else
        {
            Instantiate(slot.itemData.worldPrefab, dropPosition, Quaternion.identity);
        }

        slot.quantity--;

        if (slot.quantity <= 0)
        {
            slot.Clear();
            activeSlotIndex = -1;
        }

        OnInventoryChanged?.Invoke();
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
        // Updated to work with array
        foreach (InventorySlot slot in itemSlots)
        {
            if (!slot.IsEmpty())
                return false;
        }

        return true;
    }

    public void DropAll()
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            InventorySlot slot = itemSlots[i];
            if (slot.IsEmpty())
            {
                continue;
            }

            for (int q = 0; q < slot.quantity; q++)
            {
                Vector3 dropPos = transform.position + GetScatterOffset(i, q);

                GameObject dropped = Instantiate(slot.itemData.worldPrefab, dropPos, UnityEngine.Random.rotation);

                WorldItem worldItem = dropped.GetComponent<WorldItem>();
                if (worldItem != null)
                {
                    worldItem.isHeld = false;
                }
            }

            slot.Clear();
        }

        activeSlotIndex = -1;
        OnInventoryChanged?.Invoke();
    }

    public void DropAllPlayer()
    {
        if (currentHeldItem != null)
        {
            Destroy(currentHeldItem);
            currentHeldItem = null;
        }

        DropAll();
    }

    private Vector3 GetScatterOffset(int slotIndex, int stackIndex)
    {
        // Spread items in a circle based on slot
        float angle = (slotIndex / (float)itemSlots.Length) * Mathf.PI * 2f + stackIndex * 0.4f;
        float radius = 0.5f + stackIndex * 0.2f;

        return new Vector3(Mathf.Cos(angle) * radius, 0.2f, Mathf.Sin(angle) * radius);
    }

    public GameObject GetEquippedItem()
    {
        if (currentHeldItem == null)
        {
            return null;
        }
        return currentHeldItem;
    }

    public bool HasItemInHand()
    {
        return currentHeldItem != null;
    }
}

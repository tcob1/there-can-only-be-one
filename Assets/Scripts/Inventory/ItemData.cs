using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    // These are all shared properties for item types.
    public string itemName;
    public Sprite icon;
    public GameObject worldPrefab;
    public int maxStackSize = 1;
}

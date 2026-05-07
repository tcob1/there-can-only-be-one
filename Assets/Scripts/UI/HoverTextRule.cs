using UnityEngine;

[System.Serializable]
public class HoverTextRule
{
    //items needed for certain text to appear on hover
    // ex: dont have key -> "Unlock(Need Key)", has key -> "Unlock(E)"
    [SerializeField] public string state;
    [SerializeField] public ItemData requiredItem;
    [SerializeField] public string hoverText;
}
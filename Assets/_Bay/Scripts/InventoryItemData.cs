using UnityEngine;

[CreateAssetMenu(fileName = "InventoryItemData", menuName = "Scriptable Objects/InventoryItemData")]
public class InventoryItemData : ScriptableObject
{
    public string itemName;
    [Multiline] public string culturalOrigin;
    [Multiline] public string description;
    [Multiline] public string effectDescription;
    public Sprite icon;
    public ItemEffectType effectType;
    public bool isStackable;
    public float cooldown;
    public bool isActive;
}
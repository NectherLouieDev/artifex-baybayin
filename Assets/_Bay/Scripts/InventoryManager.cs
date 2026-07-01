using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [Header("Input Action")]
    [SerializeField] private InputAction _inventoryToggleInputAction;
    [SerializeField] private InputAction _useItem1InputAction;
    [SerializeField] private InputAction _useItem2InputAction;
    [SerializeField] private InputAction _useItem3InputAction;
    [SerializeField] private InputAction _useItem4InputAction;
    [SerializeField] private InputAction _useItem5InputAction;

    [Header("Inventory Configuration")]
    [SerializeField] private int maxItemSlots = 5;
    [SerializeField] private int maxFuelSlots = 1; // Fixed at 1

    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Image[] itemSlotImages; // 5 slots
    [SerializeField] private Image fuelSlotImage; // 1 fuel slot
    [SerializeField] private Text[] itemSlotCountTexts; // For stackable items (optional)
    [SerializeField] private Text fuelCountText;
    [SerializeField] private GameObject itemTooltipPanel;
    [SerializeField] private Text tooltipNameText;
    [SerializeField] private Text tooltipOriginText;
    [SerializeField] private Text tooltipDescriptionText;
    [SerializeField] private Text tooltipEffectText;
    [SerializeField] private Button useItemButton;
    [SerializeField] private Button dropItemButton;

    [Header("Item Prefabs")]
    [SerializeField] private GameObject itemPickupPrefab;

    // Runtime data
    private InventoryItem[] itemSlots = new InventoryItem[5];
    private int fuelAmount = 0; // Emberlight fuel
    private int selectedSlotIndex = -1;

    // Events
    public System.Action<int> OnItemUsed;
    public System.Action<int> OnItemDropped;
    public System.Action<int> OnFuelChanged;

    // Singleton
    public static InventoryManager Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Initialize empty slots
        for (int i = 0; i < maxItemSlots; i++)
        {
            itemSlots[i] = null;
        }

        UpdateUI();
        CloseTooltip();

        // Hide inventory initially
        //inventoryPanel.SetActive(false);

        _inventoryToggleInputAction.performed += InventoryToggleInputAction_performed; ;
        _inventoryToggleInputAction.Enable();

        _useItem1InputAction.performed += UseItem1InputAction_performed;
        _useItem1InputAction.Enable();

        _useItem2InputAction.performed += UseItem2InputAction_performed;
        _useItem2InputAction.Enable();

        _useItem3InputAction.performed += UseItem3InputAction_performed;
        _useItem3InputAction.Enable();

        _useItem4InputAction.performed += UseItem4InputAction_performed;
        _useItem4InputAction.Enable();

        _useItem5InputAction.performed += UseItem5InputAction_performed;
        _useItem5InputAction.Enable();
    }

    private void UseItem1InputAction_performed(InputAction.CallbackContext obj)
    {
        UseItem(0);
    }
    private void UseItem2InputAction_performed(InputAction.CallbackContext obj)
    {
        UseItem(1);
    }
    private void UseItem3InputAction_performed(InputAction.CallbackContext obj)
    {
        UseItem(2);
    }
    private void UseItem4InputAction_performed(InputAction.CallbackContext obj)
    {
        UseItem(3);
    }
    private void UseItem5InputAction_performed(InputAction.CallbackContext obj)
    {
        UseItem(4);
    }

    private void InventoryToggleInputAction_performed(InputAction.CallbackContext obj)
    {
        ToggleInventory();
    }

    void Update()
    {
        //// Toggle inventory with Tab or I key
        //if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.I))
        //{
        //    ToggleInventory();
        //}

        //// Quick use items with number keys 1-5
        //for (int i = 0; i < maxItemSlots; i++)
        //{
        //    if (Input.GetKeyDown(KeyCode.Alpha1 + i))
        //    {
        //        UseItem(i);
        //    }
        //}
    }

    #region Inventory Management

    public bool AddItem(InventoryItem item)
    {
        // Check if item already exists in inventory (for stackables)
        if (item.isStackable)
        {
            for (int i = 0; i < maxItemSlots; i++)
            {
                if (itemSlots[i] != null && itemSlots[i].itemName == item.itemName)
                {
                    itemSlots[i].currentStack++;
                    UpdateUI();
                    return true;
                }
            }
        }

        // Find empty slot
        for (int i = 0; i < maxItemSlots; i++)
        {
            if (itemSlots[i] == null)
            {
                itemSlots[i] = new InventoryItem(item);
                UpdateUI();
                return true;
            }
        }

        // Inventory full
        Debug.Log("Inventory full! Cannot add: " + item.itemName);
        UIManager.Instance.ShowMessage("Inventory is full!");
        return false;
    }

    public bool AddFuel(int amount)
    {
        fuelAmount += amount;
        OnFuelChanged?.Invoke(fuelAmount);
        UpdateUI();
        return true;
    }

    public bool UseItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxItemSlots) return false;
        if (itemSlots[slotIndex] == null) return false;

        InventoryItem item = itemSlots[slotIndex];

        // Apply item effect
        ApplyItemEffect(item);

        // Reduce stack or remove item
        if (item.isStackable && item.currentStack > 1)
        {
            item.currentStack--;
        }
        else
        {
            itemSlots[slotIndex] = null;
        }

        OnItemUsed?.Invoke(slotIndex);
        UpdateUI();
        CloseTooltip();

        Debug.Log("Used: " + item.itemName);
        return true;
    }

    public bool DropItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxItemSlots) return false;
        if (itemSlots[slotIndex] == null) return false;

        InventoryItem item = itemSlots[slotIndex];

        // Spawn item in world
        SpawnItemInWorld(item);

        // Remove from inventory
        itemSlots[slotIndex] = null;

        OnItemDropped?.Invoke(slotIndex);
        UpdateUI();
        CloseTooltip();

        Debug.Log("Dropped: " + item.itemName);
        return true;
    }

    public int GetFuelAmount()
    {
        return fuelAmount;
    }

    public bool HasItem(string itemName)
    {
        for (int i = 0; i < maxItemSlots; i++)
        {
            if (itemSlots[i] != null && itemSlots[i].itemName == itemName)
            {
                return true;
            }
        }
        return false;
    }

    public InventoryItem GetItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxItemSlots) return null;
        return itemSlots[slotIndex];
    }

    public int GetItemCount(string itemName)
    {
        int count = 0;
        for (int i = 0; i < maxItemSlots; i++)
        {
            if (itemSlots[i] != null && itemSlots[i].itemName == itemName)
            {
                count += itemSlots[i].currentStack;
            }
        }
        return count;
    }

    public bool IsInventoryFull()
    {
        for (int i = 0; i < maxItemSlots; i++)
        {
            if (itemSlots[i] == null) return false;
        }
        return true;
    }

    #endregion

    #region Item Effects

    private void ApplyItemEffect(InventoryItem item)
    {
        switch (item.effectType)
        {
            case ItemEffectType.DecayReduction:
                // Tnalak Cloth - 30% decay reduction for 30 seconds
                LanternManager.Instance.ApplyDecayReduction(0.7f, 30f);
                UIManager.Instance.ShowMessage("Tnalak Cloth: Lantern decay reduced by 30% for 30s!");
                break;

            case ItemEffectType.RangeBoost:
                // Kulintang Gong Essence - 50% range boost for 20 seconds
                LanternManager.Instance.ApplyRangeBoost(1.5f, 20f);
                UIManager.Instance.ShowMessage("Kulintang Gong: Lantern range boosted for 20s!");
                break;

            case ItemEffectType.InstantFuel:
                // Annatto Seed Oil - Restores 10 fuel instantly
                LanternManager.Instance.Refuel(10f);
                UIManager.Instance.ShowMessage("Annatto Oil: +10 Emberlight!");
                break;

            case ItemEffectType.CraftTorch:
                // Kawayan Bamboo Torch - Craftable, serves as backup light
                // This would create a torch item in world or temporary light
                UIManager.Instance.ShowMessage("Kawayan Torch crafted!");
                // Could spawn a torch object or provide temporary light
                break;

            case ItemEffectType.PulseBoost:
                // Maguindanao Inaul Cloth - Pulse effect pushes back fog
                //FogManager.Instance.PushBackFog(transform.position, 15f);
                UIManager.Instance.ShowMessage("Inaul Cloth: Fog pushed back!");
                break;

            default:
                Debug.Log("Unknown effect type: " + item.effectType);
                break;
        }
    }

    #endregion

    #region World Interaction

    void SpawnItemInWorld(InventoryItem item)
    {
        if (itemPickupPrefab == null) return;

        // Spawn at player position with slight offset
        Vector3 spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 1.5f;
        GameObject droppedItem = Instantiate(itemPickupPrefab, spawnPos, Quaternion.identity);

        // Set item data on the pickup
        //ItemPickup pickup = droppedItem.GetComponent<ItemPickup>();
        //if (pickup != null)
        //{
        //    pickup.SetItem(item);
        //}
    }

    #endregion

    #region UI

    void UpdateUI()
    {
        /* Local update
        //// Update item slots
        //for (int i = 0; i < maxItemSlots; i++)
        //{
        //    if (itemSlots[i] != null)
        //    {
        //        itemSlotImages[i].sprite = itemSlots[i].icon;
        //        itemSlotImages[i].color = Color.white;
        //        itemSlotImages[i].gameObject.SetActive(true);

        //        // Show stack count if stackable and > 1
        //        if (itemSlots[i].isStackable && itemSlots[i].currentStack > 1)
        //        {
        //            itemSlotCountTexts[i].text = itemSlots[i].currentStack.ToString();
        //            itemSlotCountTexts[i].gameObject.SetActive(true);
        //        }
        //        else
        //        {
        //            itemSlotCountTexts[i].gameObject.SetActive(false);
        //        }
        //    }
        //    else
        //    {
        //        itemSlotImages[i].sprite = null;
        //        itemSlotImages[i].color = new Color(0, 0, 0, 0);
        //        itemSlotCountTexts[i].gameObject.SetActive(false);
        //    }
        //}

        //// Update fuel slot
        //if (fuelAmount > 0)
        //{
        //    fuelSlotImage.sprite = GetFuelIcon();
        //    fuelSlotImage.color = Color.white;
        //    fuelCountText.text = fuelAmount.ToString();
        //    fuelCountText.gameObject.SetActive(true);
        //}
        //else
        //{
        //    fuelSlotImage.sprite = null;
        //    fuelSlotImage.color = new Color(0, 0, 0, 0);
        //    fuelCountText.gameObject.SetActive(false);
        //}
        */

        UIManager.Instance.UpdateInventoryUI(itemSlots, fuelAmount);
    }

    Sprite GetFuelIcon()
    {
        // Return fuel icon based on amount
        // Could use different icons for different fuel levels
        // For now, just return a default fuel icon
        return Resources.Load<Sprite>("Icons/EmberlightIcon");
    }

    void ToggleInventory()
    {
        UIManager.Instance.ToggleInventory();

        if (!UIManager.Instance.InventoryPanelActiveSelf)
        {
            CloseTooltip();
        }

        //inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        //if (!inventoryPanel.activeSelf)
        //{
        //    CloseTooltip();
        //}
    }

    public void OpenTooltip(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxItemSlots) return;
        if (itemSlots[slotIndex] == null) return;

        selectedSlotIndex = slotIndex;
        InventoryItem item = itemSlots[slotIndex];

        tooltipNameText.text = item.itemName;
        tooltipOriginText.text = "Origin: " + item.culturalOrigin;
        tooltipDescriptionText.text = item.description;
        tooltipEffectText.text = "Effect: " + item.effectDescription;

        itemTooltipPanel.SetActive(true);
    }

    public void CloseTooltip()
    {
        //itemTooltipPanel.SetActive(false);
        //selectedSlotIndex = -1;

        UIManager.Instance.CloseTooltip();
    }

    public void OnUseButtonPressed()
    {
        if (selectedSlotIndex >= 0)
        {
            UseItem(selectedSlotIndex);
        }
    }

    public void OnDropButtonPressed()
    {
        if (selectedSlotIndex >= 0)
        {
            DropItem(selectedSlotIndex);
        }
    }

    #endregion

    #region Save/Load (Optional for future)

    public InventorySaveData GetSaveData()
    {
        InventorySaveData data = new InventorySaveData();
        data.fuelAmount = fuelAmount;
        data.itemData = new List<ItemSaveData>();

        for (int i = 0; i < maxItemSlots; i++)
        {
            if (itemSlots[i] != null)
            {
                ItemSaveData itemData = new ItemSaveData();
                itemData.itemName = itemSlots[i].itemName;
                itemData.currentStack = itemSlots[i].currentStack;
                data.itemData.Add(itemData);
            }
        }

        return data;
    }

    public void LoadSaveData(InventorySaveData data)
    {
        // Clear inventory
        for (int i = 0; i < maxItemSlots; i++)
        {
            itemSlots[i] = null;
        }

        fuelAmount = data.fuelAmount;

        // Load items
        for (int i = 0; i < data.itemData.Count && i < maxItemSlots; i++)
        {
            ItemSaveData itemData = data.itemData[i];
            InventoryItem item = new InventoryItem(GetItemDataByName(itemData.itemName));
            item.currentStack = itemData.currentStack;
            itemSlots[i] = item;
        }

        UpdateUI();
    }

    InventoryItemData GetItemDataByName(string itemName)
    {
        // Look up item data from a scriptable object database
        // For now, return null
        return null;
    }

    #endregion
}

#region Data Classes

[System.Serializable]
public class InventoryItem
{
    public string itemName;
    public string culturalOrigin;
    public string description;
    public string effectDescription;
    public Sprite icon;
    public ItemEffectType effectType;
    public bool isStackable;
    public int currentStack;

    public InventoryItem(InventoryItemData data)
    {
        if (data != null)
        {
            itemName = data.itemName;
            culturalOrigin = data.culturalOrigin;
            description = data.description;
            effectDescription = data.effectDescription;
            icon = data.icon;
            effectType = data.effectType;
            isStackable = data.isStackable;
            currentStack = 1;
        }
    }

    // Constructor for copying
    public InventoryItem(InventoryItem other)
    {
        itemName = other.itemName;
        culturalOrigin = other.culturalOrigin;
        description = other.description;
        effectDescription = other.effectDescription;
        icon = other.icon;
        effectType = other.effectType;
        isStackable = other.isStackable;
        currentStack = other.currentStack;
    }
}

[System.Serializable]
public class InventoryItemData : ScriptableObject
{
    public string itemName;
    public string culturalOrigin;
    public string description;
    public string effectDescription;
    public Sprite icon;
    public ItemEffectType effectType;
    public bool isStackable;
}

public enum ItemEffectType
{
    None,
    DecayReduction,      // Tnalak Cloth
    RangeBoost,          // Kulintang Gong Essence
    InstantFuel,         // Annatto Seed Oil
    CraftTorch,          // Kawayan Bamboo Torch
    PulseBoost           // Maguindanao Inaul Cloth
}

[System.Serializable]
public class InventorySaveData
{
    public int fuelAmount;
    public List<ItemSaveData> itemData;
}

[System.Serializable]
public class ItemSaveData
{
    public string itemName;
    public int currentStack;
}

#endregion
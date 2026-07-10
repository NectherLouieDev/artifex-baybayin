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
    [SerializeField] private int maxItemSlots = 4;
    [SerializeField] private int maxFuelSlots = 1; // Fixed at 1

    [Header("Item Prefabs")]
    [SerializeField] private GameObject itemPickupPrefab;

    // Runtime data
    private InventoryItem[] itemSlots = new InventoryItem[4];
    private int emberlightAmount = 0;
    private int selectedSlotIndex = -1;
    private List<GGTimer> _cdTimers = new List<GGTimer>();

    // Events
    public System.Action<int> OnItemUsed;
    public System.Action<int> OnItemDropped;
    public System.Action<int> OnFuelChanged;

    // Singleton
    public static InventoryManager Instance;

    // Timer Event Args
    public class CustomTimerEventArgs : TimerEventArgs
    {
        public int SlotIndex { get; set; }
    }

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

            GGTimer cdTimer = gameObject.AddComponent<GGTimer>();
            cdTimer.timerId = "CD Timer " + i;
            cdTimer.OnTimerCompleted += CDTimer_OnTimerCompleted;
            cdTimer.OnTimerUpdated += CDTimer_OnTimerUpdated;
            _cdTimers.Add(cdTimer);
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
        if (LanternManager.Instance.FuelPercentage >= 0.9f)
            return;

        UseEmberlight();
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
                itemSlots[i].isActive = true;
                UpdateUI();
                return true;
            }
        }

        // Inventory full
        Debug.Log("Inventory full! Cannot add: " + item.itemName);
        UIManager.Instance.ShowMessage("Inventory is full!");
        return false;
    }

    public bool AddEmberlight(int amount)
    {
        emberlightAmount += amount;
        OnFuelChanged?.Invoke(emberlightAmount);
        UpdateUI();
        return true;
    }

    public void UseEmberlight()
    {
        if (emberlightAmount <= 0)
            return;

        LanternManager.Instance.Refuel(5);
        emberlightAmount -= 1;
        UpdateUI();
    }

    public bool UseItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxItemSlots) 
            return false;

        if (itemSlots[slotIndex] == null) 
            return false;

        InventoryItem item = itemSlots[slotIndex];

        if (!item.isActive)
            return false;

        // Apply item effect
        ApplyItemEffect(item);

        // Reduce stack or remove item
        if (item.isStackable && item.currentStack > 1)
        {
            item.currentStack--;
        }
        else
        {
            // Cooldown
            //itemSlots[slotIndex] = null;
            item.isActive = false;
            _cdTimers[slotIndex].StartTimer(item.cooldown, 1, true, new CustomTimerEventArgs
            {
                SlotIndex = slotIndex,
            });
        }

        OnItemUsed?.Invoke(slotIndex);
        UpdateUI();
        CloseTooltip();

        return true;
    }

    private void CDTimer_OnTimerUpdated(object sender, GGTimer e)
    {
        int slotIndex = (e.EventArgs as CustomTimerEventArgs).SlotIndex;
        int currentTime = Mathf.FloorToInt(e.GetCurrentTime());

        UIManager.Instance.UpdateItemSlotCounter(slotIndex, currentTime);
    }

    private void CDTimer_OnTimerCompleted(object sender, GGTimer e)
    {
        int slotIndex = (e.EventArgs as CustomTimerEventArgs).SlotIndex;
        int currentTime = Mathf.FloorToInt(e.GetCurrentTime());

        UIManager.Instance.UpdateItemSlotCounter(slotIndex, currentTime);

        InventoryItem item = itemSlots[slotIndex];
        item.isActive = true;
        UpdateUI();
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

    public int GetEmberlightAmount()
    {
        return emberlightAmount;
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
            case ItemEffectType.TnalakDecayReduction:
                // Tnalak Cloth - 30% decay reduction for 30 seconds
                LanternManager.Instance.ApplyDecayReduction(0.1f, 30f);
                UIManager.Instance.ShowMessage("Lantern decay reduced by 90%!");
                break;

            case ItemEffectType.KulintangEcho:
                // Kulintang Gong Essence - 50% range boost for 20 seconds
                // Enable Echo Component
                LanternManager.Instance.ApplyRangeBoost(1.5f, 20f);
                UIManager.Instance.ShowMessage("Lantern range boosted!");
                break;

            case ItemEffectType.AnnattoRefill:
                // Annatto Seed Oil - Refills fuel
                LanternManager.Instance.RespawnFuel();
                UIManager.Instance.ShowMessage("Fuel refilled!");
                break;

            case ItemEffectType.KawayanTorch:
                // Kawayan Bamboo Torch - Craftable, serves as backup light
                // This would create a torch item in world or temporary light
                LanternManager.Instance.SpawnBambooTorch();
                UIManager.Instance.ShowMessage("Kawayan Torch crafted!");
                // Could spawn a torch object or provide temporary light
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
        UIManager.Instance.UpdateInventoryUI(itemSlots, emberlightAmount);
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
    }

    public void CloseTooltip()
    {
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
        data.emberlightAmount = emberlightAmount;
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

        emberlightAmount = data.emberlightAmount;

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
    public float cooldown;
    public bool isActive;

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
            cooldown = data.cooldown;
            isActive = data.isActive;
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
        cooldown = other.cooldown;
        isActive = other.isActive;
    }
}

public enum ItemEffectType
{
    None,
    TnalakDecayReduction,      // Tnalak Cloth
    KulintangEcho,          // Kulintang Gong Essence
    AnnattoRefill,         // Annatto Seed Oil
    KawayanTorch          // Kawayan Bamboo Torch
}

[System.Serializable]
public class InventorySaveData
{
    public int emberlightAmount;
    public List<ItemSaveData> itemData;
}

[System.Serializable]
public class ItemSaveData
{
    public string itemName;
    public int currentStack;
}

#endregion
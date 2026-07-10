using Unity.VisualScripting;
using UnityEngine;


public class ItemEquip : InteractableObject, ICarryable
{
    [SerializeField] private GameData_ItemConfig _itemConfig;
    [SerializeField] private bool _interactable = false;

    [Header("Inventory Item Data")]
    [SerializeField] private InventoryItemData _inventoryItemData;

    private SphereCollider _sphereCollider;
    private Rigidbody _rb;

    public ERecipeInputType RecipeInputType { get { return _itemConfig.recipeInputType; } }

    public GameData_ItemConfig ItemConfig {  get { return _itemConfig; } }

    private bool isPlayer = false;
    private ItemCarrierComponent _playerCarrier;

    protected virtual void Awake()
    {
        _sphereCollider = GetComponent<SphereCollider>();
        _rb = GetComponent<Rigidbody>();
    }

    public override void Interact(Transform interactor)
    {
        base.Interact(interactor);

        if (!_interactable)
            return;

        if (interactor.TryGetComponent(out ItemCarrierComponent itemCarrierComponent))
        {
            if (interactor.TryGetComponent(out PlayerIdentity identity))
            {
                isPlayer = true;
                _playerCarrier = itemCarrierComponent;
            }
            else
            {
                isPlayer = false;
                _playerCarrier = null;
            }

            // drop carried item first
            if (itemCarrierComponent.IsCarrying)
            {
                itemCarrierComponent.DropItem();
            }

            itemCarrierComponent.PickupItem(this);
        }
    }

    public void Pickup(Transform carryPoint)
    {
        _interactable = false;

        transform.SetParent(carryPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // Disable Components
        _rb.isKinematic = true;
        _sphereCollider.enabled = false;
        _sphereCollider.radius = 0.25f;

        //PlayPickupFeedbacks();

        if (isPlayer)
        {
            InventoryItem item = new InventoryItem(_inventoryItemData);

            InventoryManager.Instance.AddItem(item);

            UIManager.Instance.ShowItemScreen(item);

            _playerCarrier.DespawnItem();
        }

    }

    public void Drop()
    {
        transform.localPosition += new Vector3(-0.1f,0,0);
        transform.SetParent(null);

        // Enable Components
        _rb.isKinematic = false;
        _sphereCollider.enabled = true;
        _sphereCollider.radius = 1.0f;

        _interactable = true;

        _rb.AddForce(transform.forward * 2.0f, ForceMode.Impulse);

        //PlayDropFeedbacks();
    }

    public void Despawn()
    {
        transform.SetParent(null);

        // Enable Components
        _rb.isKinematic = false;
        _sphereCollider.enabled = true;
        _sphereCollider.radius = 1.0f;

        Destroy(gameObject);
    }

    public GameData_ItemConfig GetItemConfig()
    {
        return ItemConfig;
    }
}

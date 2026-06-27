using UnityEngine;

public interface ICarryable
{
    void Pickup(Transform carryPoint);
    void Drop();

    void Despawn();

    GameData_ItemConfig GetItemConfig();
}

public class ItemCarrierComponent : MonoBehaviour
{
    [SerializeField] private Transform _carryPoint;
    [SerializeField] private AudioClip _pickupClip;
    [SerializeField] private AudioClip _dropClip;

    private ICarryable _carriedItem;
    public bool IsCarrying { get { return _carriedItem != null; } }
    public ICarryable CarriedItem { get { return _carriedItem; } }

    private void Start()
    {
        if (_carryPoint.childCount > 0)
        {
            ICarryable _c = _carryPoint.GetComponentInChildren<ICarryable>();
            PickupItem(_c);
        }
    }

    public void PickupItem(ICarryable item)
    {
        if (IsCarrying)
            return;

        _carriedItem = item;

        //if (_pickupClip)
        //    AudioManager.Instance.PlaySFX(_pickupClip);

        _carriedItem.Pickup(_carryPoint);

        // Notify event system
        GameplayEventBus.Instance.Invoke(new GameplayEvents.PlayerPickedUpItem
        {
            Identity = transform.GetComponent<PlayerIdentity>(),
            ItemCarried = item
        });
    }

    public ICarryable DropItem()
    {
        if (!IsCarrying)
            return null;

        var item = _carriedItem;
        item.Drop();

        _carriedItem = null;

        //if (_dropClip)
        //    AudioManager.Instance.PlaySFX(_dropClip);

        // Notify event system
        GameplayEventBus.Instance.Invoke(new GameplayEvents.PlayerDroppedItem
        {
            Identity = transform.GetComponent<PlayerIdentity>(),
            ItemCarried = item
        });

        return item;
    }

    public void DespawnItem()
    {
        var item = _carriedItem;
        item.Despawn();

        _carriedItem = null;
    }
}

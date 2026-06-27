using UnityEngine;

public class CookStation : InteractableObject
{
    private ItemCarrierComponent _cookCarrierComponent;

    private void Awake()
    {
        _cookCarrierComponent = GetComponent<ItemCarrierComponent>();
    }

    public override void Interact(Transform interactor)
    {
        base.Interact(interactor);

        // if prep station is empty
        // then we can drop
        if (!_cookCarrierComponent.IsCarrying)
        {
            // if interactor has carrier component
            if (interactor.TryGetComponent(out ItemCarrierComponent actorCarrierComponent))
            {
                // interactor has item
                if (actorCarrierComponent.IsCarrying)
                {
                    ICarryable itemCarried = actorCarrierComponent.DropItem();
                    _cookCarrierComponent.PickupItem(itemCarried);
                }
                else
                {
                    // interactor not carrying anything
                }
            }
        }
        // if prep station has item
        else
        {
            if (interactor.TryGetComponent(out ItemCarrierComponent actorCarrierComponent))
            {
                // if actor is not carrying anything we can pick up
                if (!actorCarrierComponent.IsCarrying)
                {
                    ICarryable stationItem = _cookCarrierComponent.DropItem();
                    actorCarrierComponent.PickupItem(stationItem);
                }
                else
                {
                    // actor is carrying something
                    // drop that first
                    actorCarrierComponent.DropItem();
                }
            }
        }
    }
}

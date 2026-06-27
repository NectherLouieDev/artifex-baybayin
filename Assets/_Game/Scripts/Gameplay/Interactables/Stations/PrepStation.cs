using System.Collections.Generic;
using UnityEngine;

public class PrepStation : InteractableObject
{
    private ItemCarrierComponent _prepCarrierComponent;

    private void Awake()
    {
        _prepCarrierComponent = GetComponent<ItemCarrierComponent>();
    }

    public override void Interact(Transform interactor)
    {
        base.Interact(interactor);

        // if prep station is empty
        // then we can drop
        if (!_prepCarrierComponent.IsCarrying)
        {
            // if interactor has carrier component
            if (interactor.TryGetComponent(out ItemCarrierComponent actorCarrierComponent))
            {
                // interactor has item
                if (actorCarrierComponent.IsCarrying)
                {
                    ICarryable itemCarried = actorCarrierComponent.DropItem();
                    _prepCarrierComponent.PickupItem(itemCarried);
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
                    ICarryable stationItem = _prepCarrierComponent.DropItem();
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

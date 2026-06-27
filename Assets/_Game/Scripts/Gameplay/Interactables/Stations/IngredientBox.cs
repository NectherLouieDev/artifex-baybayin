using UnityEngine;

public class IngredientBox : InteractableObject
{
    [SerializeField] private GameObject _prefab;

    public override void Interact(Transform interactor)
    {
        base.Interact(interactor);
        
        // if interactor has carrier
        if (interactor.TryGetComponent(out ItemCarrierComponent itemCarrierComponent))
        {
            // drop carried item first
            if (itemCarrierComponent.IsCarrying)
            {
                itemCarrierComponent.DropItem();
            }

            // spawn and pickup
            GameObject g = Instantiate(_prefab, transform.position, Quaternion.identity);

            itemCarrierComponent.PickupItem(g.GetComponent<ItemIngredient>());
        }
    }
}

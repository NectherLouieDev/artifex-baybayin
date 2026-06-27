using System.Security.Principal;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IInteractable
{
    void Interact(Transform interactor);
}

public class InteractionHandler : MonoBehaviour
{
    [SerializeField] private PlayerController _controller;
    [SerializeField] private ItemCarrierComponent _itemCarrier;

    [Header("Interact")]
    [SerializeField] private float _interactDistance = 2f;
    [SerializeField] private LayerMask _interactableLayer;
    [SerializeField] private Vector3 _lastInteractDirection;

    private IInteractable _currentInteractableObject;
    public IInteractable CurrentInteractableObject
    {
        get { return _currentInteractableObject; }
    }

    private PushableComponent _currentPushable;

    public void OnInteract(InputValue value)
    {
        if (!_controller.InputEnabled) 
            return;

        TryInteract();
    }

    public void TryInteract()
    {
        // Case 1: if there is an interactable object
        if (_currentInteractableObject != null)
        {
            _currentInteractableObject.Interact(transform);
            return;
        }

        // Case 2: if we are carrying an item then we can drop
        if (_itemCarrier.IsCarrying)
        {
            // NOTE: Allow dropping of all other items?
            _itemCarrier.DropItem();
            //if (_itemCarrier.CarriedType == ResourceType.Bomb)
            //{
            //    _itemCarrier.DropItem();
            //}
        }
    }

    public void OnAttack(InputValue value)
    {
        if (!_controller.InputEnabled)
            return;

        TryPush();
    }

    public void TryPush()
    {
        // Case 1: if there is a pushable rigidbody
        if (_currentPushable != null)
        {
            _currentPushable.Push(_lastInteractDirection);

            if (_currentPushable.TryGetComponent(out InteractionHandler pl))
            {
                pl._itemCarrier.DropItem();
            }

            // If this player is carrying
            if (_itemCarrier.IsCarrying)
            {
                _itemCarrier.DropItem();
            }
        }
    }

    public void OnDash(InputValue value)
    {
        if (!_controller.InputEnabled)
            return;

        if (TryGetComponent(out DashComponent dashComponent))
        {
            dashComponent.Dash(_lastInteractDirection);
        }
    }

    private void Update()
    {
        Vector3 moveDirection = transform.forward;

        if (moveDirection != Vector3.zero)
        {
            _lastInteractDirection = moveDirection;
        }

        TryRayCast();
    }

    private void TryRayCast()
    {
        // Interact Check
        if (Physics.Raycast(transform.position, _lastInteractDirection, out RaycastHit interactHit, _interactDistance, _interactableLayer))
        {
            if (interactHit.transform.TryGetComponent(out IInteractable interactableObject))
            {
                ChangeCurrentInteractableObject(interactableObject);
            }
            else
            {
                ChangeCurrentInteractableObject(null);
            }

        }
        else
        {
            ChangeCurrentInteractableObject(null);
        }

        // Push and Dash Check
        if (Physics.Raycast(transform.position, _lastInteractDirection, out RaycastHit pushHit, _interactDistance, _interactableLayer))
        {
            if (pushHit.collider.TryGetComponent(out PushableComponent pushableComponent))
            {
                _currentPushable = pushableComponent;
            }
            else
            {
                _currentPushable = null;
            }
        }
        else
        {
            _currentPushable = null;
        }
    }

    private void ChangeCurrentInteractableObject(IInteractable interactableObject)
    {
        _currentInteractableObject = interactableObject;

        GameplayEventBus.Instance.Invoke(new GameplayEvents.InteractableChanged
        {
            SelectedObject = _currentInteractableObject
        });
    }
}

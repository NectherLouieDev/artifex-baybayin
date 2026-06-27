using Microlight.MicroBar;
using UnityEngine;
using UnityEngine.AI;

public class BaseCreep : InteractableObject, ICarryable
{
    [SerializeField] private CreepAgentComponent _agentComponent;
    [SerializeField] private bool _interactable = true;

    private NavMeshAgent _agent;
    private SphereCollider _sphereCollider;
    private Rigidbody _rb;

    protected virtual void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
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
        _agentComponent.SetActive(false);
        _agent.enabled = false;
        _rb.isKinematic = true;
        _sphereCollider.enabled = false;
        _sphereCollider.radius = 0.25f;

        //PlayPickupFeedbacks();
    }

    public void Drop()
    {
        transform.localPosition += new Vector3(-0.1f, 0, 0);
        transform.SetParent(null);

        // Enable Components
        _agent.enabled = true;
        _agentComponent.SetActive(true);
        _rb.isKinematic = false;
        _sphereCollider.enabled = true;
        _sphereCollider.radius = 1.0f;

        _interactable = true;

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
        return null;
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.TryGetComponent(out DestructibleBlock destructibleBlock))
        {
            if (destructibleBlock.IsBroken)
                return;

            transform.GetComponent<CreepAgentComponent>().Boom();

            if (transform.TryGetComponent(out HealthComponent healthComponent))
            {
                healthComponent.TakeDamage(1000);
            }
        }
    }
}

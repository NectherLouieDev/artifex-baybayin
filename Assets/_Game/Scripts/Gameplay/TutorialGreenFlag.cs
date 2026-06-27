using UnityEngine;

public class TutorialGreenFlag : MonoBehaviour
{
    private LevelManager _levelManager;

    private void Awake()
    {
        _levelManager = FindFirstObjectByType<LevelManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerIdentity identity))
        {
            ItemCarrierComponent carrierComponent = identity.GetComponent<ItemCarrierComponent>();
            if (carrierComponent != null && carrierComponent.CarriedItem != null)
            {
                // celebrate sequence
                GameplayEventBus.Instance.Invoke(new GameplayEvents.Tutorial00_GreenFlagTriggered
                {
                    Identity = _levelManager.Identity
                });
            }
        }
    }
}

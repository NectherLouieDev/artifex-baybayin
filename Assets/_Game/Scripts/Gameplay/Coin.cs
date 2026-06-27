using UnityEngine;

public class Coin : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerIdentity playerIdentity))
        {
            GameplayEventBus.Instance.Invoke(new GameplayEvents.CoinCollected
            {
                CoinObject = this,
                Identity = playerIdentity
            });

            Destroy(gameObject);
        }
    }
}

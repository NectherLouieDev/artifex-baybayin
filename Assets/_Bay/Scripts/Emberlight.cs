using UnityEngine;

public class Emberlight : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerIdentity identity))
        {
            LanternManager.Instance.Refuel(5f);
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.TryGetComponent(out PlayerIdentity identity))
        {
            LanternManager.Instance.Refuel(5f);
            Destroy(gameObject);
        }
    }
}

using DG.Tweening;
using UnityEngine;

public class SafeAreaTrigger : MonoBehaviour
{
    [SerializeField] private MemoryStone _memoryStone;

    public void Activate()
    {
        transform.localScale = Vector3.zero;

        transform.DOScale(1, 0.5f)
            .OnComplete(OnActivateComplete);
    }

    private void OnActivateComplete()
    {
        Debug.Log("OnActivateComplete");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_memoryStone.IsActivated)
            return;

        if (other.TryGetComponent(out PlayerIdentity identity))
        {
            LanternManager.Instance.EnterSafeZone();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!_memoryStone.IsActivated)
            return;

        if (other.TryGetComponent(out PlayerIdentity identity))
        {
            LanternManager.Instance.ExitSafeZone();
        }
    }
}

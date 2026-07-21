using UnityEngine;

public class ShadowDecayComponent : MonoBehaviour
{
    [SerializeField] private ShadowAgentStateHandler _shadowAgentStateHandler;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerIdentity identity))
        {
            LanternManager.Instance.IsShadowNear = true;
            UIManager.Instance.ShowMessage("Shadow is nearby!");
        }

        if (other.TryGetComponent(out MemoryStone memoryStone))
        {
            _shadowAgentStateHandler.TriggerOnMemoryStone();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out MemoryStone memoryStone))
        {
            _shadowAgentStateHandler.TriggerOnMemoryStone();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerIdentity identity))
        {
            LanternManager.Instance.IsShadowNear = false;
        }
    }
}

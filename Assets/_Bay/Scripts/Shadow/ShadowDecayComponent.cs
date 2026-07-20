using UnityEngine;

public class ShadowDecayComponent : MonoBehaviour
{
    [SerializeField] private ShadowAgentStateHandler _shadowAgentStateHandler;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerIdentity identity))
        {
            _shadowAgentStateHandler.MovementEnabled = true;
            LanternManager.Instance.IsShadowNear = true;
            UIManager.Instance.ShowMessage("Shadow is nearby!");
        }

        if (other.TryGetComponent(out MemoryStone memoryStone))
        {
            _shadowAgentStateHandler.MovementEnabled = false;
            UIManager.Instance.ShowMessage("Shadow has stopped!");
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

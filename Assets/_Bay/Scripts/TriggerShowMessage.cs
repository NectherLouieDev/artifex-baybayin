using UnityEngine;

public class TriggerShowMessage : MonoBehaviour
{
    [SerializeField] private string _msg;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerIdentity identity))
        {
            UIManager.Instance.ShowMessage(_msg);
        }
    }
}

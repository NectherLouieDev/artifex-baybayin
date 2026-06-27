using UnityEngine;

public class PushableComponent : MonoBehaviour
{
    [SerializeField] private float _pushForce = 2f;
    private Rigidbody _rb;

    public float PushForce
    {
        get { return _pushForce; }
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void Push(Vector3 direction, float force = 0)
    {
        if (_rb == null)
            return;

        _rb.AddForce(direction * (force != 0 ? force : _pushForce), ForceMode.Impulse);
    }
}

using UnityEngine;

public class DashComponent : MonoBehaviour
{
    [SerializeField] private float _dashForce = 16.0f;
    [SerializeField] private float _cooldownDuration = 1.0f;

    private Rigidbody _rb;
    private GGTimer _dashCooldownTimer;
    private bool _canDash = true;

    public float DashForce
    {
        get { return _dashForce; }
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        _dashCooldownTimer = gameObject.AddComponent<GGTimer>();
        _dashCooldownTimer.timerId = "Dash Cooldown Timer";
        _dashCooldownTimer.OnTimerCompleted += DashCooldownTimer_OnTimerCompleted;
    }

    private void DashCooldownTimer_OnTimerCompleted(object sender, GGTimer e)
    {
        _canDash = true;
    }

    public void Dash(Vector3 direction, float force = 0)
    {
        if (_rb == null)
            return;

        if (!_canDash)
            return;

        _canDash = false;

        _rb.mass = 0.75f;
        _rb.AddForce(direction * (force > 0 ? force : _dashForce), ForceMode.Impulse);
        _rb.mass = 1;

        _dashCooldownTimer.StartTimer(_cooldownDuration, 1);
    }
}

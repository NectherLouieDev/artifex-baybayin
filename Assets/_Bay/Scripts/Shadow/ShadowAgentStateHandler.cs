using Unity.AI.Assistant.Agents;
using UnityEngine;
using UnityEngine.AI;

public enum EShadowAgentState
{
    Idle,
    Chase,
    Flee
}

public class ShadowAgentStateHandler : MonoBehaviour
{
    [SerializeField] private NavMeshAgent _agent;

    [SerializeField] private float followSpeed = 3.5f;
    [SerializeField] private float stoppingDistance = 0.5f;
    [SerializeField] private float updatePathInterval = 0.5f;

    private bool _movementEnabled = false;

    public bool MovementEnabled
    {
        get { return _movementEnabled; }
        set { _movementEnabled = value; }
    }
    
    private GGTimer pathUpdateTimer;

    private void Start()
    {
        pathUpdateTimer = gameObject.AddComponent<GGTimer>();
        pathUpdateTimer.timerId = $"{gameObject.name}_PathUpdateTimer";
        pathUpdateTimer.OnTimerLoop += PathUpdateTimer_OnTimerLoop;

        MovementEnabled = true;
        pathUpdateTimer.StartTimer(updatePathInterval, 0);
    }

    private void PathUpdateTimer_OnTimerLoop(object sender, GGTimer e)
    {
        if (!_movementEnabled)
        {
            _agent.speed = 0;
            return;
        }

        ShadowTarget _target = FindFirstObjectByType<ShadowTarget>();

        if (_target != null && _agent != null && _agent.isOnNavMesh)
        {
            _agent.speed = followSpeed;
            _agent.stoppingDistance = stoppingDistance;
            _agent.autoBraking = true;
            _agent.updateRotation = true;
            _agent.updatePosition = true;

            _agent.SetDestination(_target.transform.position);
        }
    }
}

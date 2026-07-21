using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public enum EShadowAgentState
{
    Idle,           // Wandering aimlessly
    Investigating,  // Moving toward a point of interest (light, sound)
    Approaching,    // Moving toward the player
    Retreating,     // Moving away from the player/lantern
    Fleeing,        // Fast retreat when stunned or at Memory Stone
    Stunned         // Brief pause when flashed by lantern
}

public class ShadowAgentStateHandler : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private ShadowTarget _target;

    [Header("Movement Settings")]
    [SerializeField] private float wanderSpeed = 1.5f;
    [SerializeField] private float approachSpeed = 3.5f;
    [SerializeField] private float retreatSpeed = 2.5f;
    [SerializeField] private float fleeSpeed = 5.0f;
    [SerializeField] private float stoppingDistance = 0.5f;
    [SerializeField] private float updatePathInterval = 0.5f;

    [Header("State Detection Settings")]
    [SerializeField] private float investigationRadius = 20f;
    [SerializeField] private float approachRadius = 15f;
    [SerializeField] private float retreatRadius = 5f;
    [SerializeField] private float fleeRadius = 3f;
    [SerializeField] private float minWanderDistance = 5f;
    [SerializeField] private float maxWanderDistance = 20f;

    [Header("State Timers")]
    [SerializeField] private float idleDurationMin = 3f;
    [SerializeField] private float idleDurationMax = 8f;
    [SerializeField] private float stunDuration = 2f;
    [SerializeField] private float investigateDuration = 4f;

    [Header("Detection")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float lineOfSightCheckInterval = 1f;

    // State Machine
    private EShadowAgentState _currentState = EShadowAgentState.Idle;
    private EShadowAgentState _previousState = EShadowAgentState.Idle;

    // State Data
    private float _stateTimer = 0f;
    private Vector3 _wanderTarget;
    private Vector3 _investigationPoint;
    private bool _hasLineOfSight = false;
    private float _distanceToPlayer = Mathf.Infinity;
    private bool _isInMemoryStoneZone = false;

    // Timers
    private GGTimer _pathUpdateTimer;
    private GGTimer _lineOfSightTimer;
    private GGTimer _stateTransitionTimer;

    // Events for UI/audio feedback
    public System.Action<EShadowAgentState> OnStateChanged;
    public System.Action<float> OnProximityChanged; // 0 = far, 1 = very close

    // Properties
    public EShadowAgentState CurrentState => _currentState;
    public float DistanceToPlayer => _distanceToPlayer;
    public bool IsInMemoryStoneZone => _isInMemoryStoneZone;
    public bool HasLineOfSight => _hasLineOfSight;
    public NavMeshAgent Agent => _agent;

    private void Start()
    {
        InitializeTimers();
        FindTarget();
        SetState(EShadowAgentState.Approaching);
    }

    private void Update()
    {
        if (_target == null)
        {
            FindTarget();
            return;
        }

        UpdateDistanceToPlayer();
        UpdateLineOfSight();
        //UpdateMemoryStoneZone();
        UpdateStateTimer();
        EvaluateStateTransition();
    }

    #region Initialization

    private void InitializeTimers()
    {
        _pathUpdateTimer = gameObject.AddComponent<GGTimer>();
        _pathUpdateTimer.timerId = $"{gameObject.name}_PathUpdateTimer";
        _pathUpdateTimer.OnTimerLoop += PathUpdateTimer_OnTimerLoop;
        _pathUpdateTimer.StartTimer(updatePathInterval, 0);

        _lineOfSightTimer = gameObject.AddComponent<GGTimer>();
        _lineOfSightTimer.timerId = $"{gameObject.name}_LineOfSightTimer";
        _lineOfSightTimer.OnTimerLoop += LineOfSightTimer_OnTimerLoop;
        _lineOfSightTimer.StartTimer(lineOfSightCheckInterval, 0);

        _stateTransitionTimer = gameObject.AddComponent<GGTimer>();
        _stateTransitionTimer.timerId = $"{gameObject.name}_StateTransitionTimer";
        _stateTransitionTimer.OnTimerCompleted += StateTransitionTimer_OnTimerComplete;
    }

    private void FindTarget()
    {
        _target = FindFirstObjectByType<ShadowTarget>();
        if (_target == null)
        {
            Debug.LogWarning($"ShadowAgent {gameObject.name}: No ShadowTarget found in scene!");
        }
    }

    #endregion

    #region State Management

    private void SetState(EShadowAgentState newState)
    {
        if (_currentState == newState) return;

        _previousState = _currentState;
        _currentState = newState;
        _stateTimer = 0f;

        // Stop any pending state transition timers
        _stateTransitionTimer.StopTimer();

        // Handle state entry
        OnStateEnter(newState);

        // Notify listeners
        OnStateChanged?.Invoke(newState);

        Debug.Log($"{gameObject.name} State Changed: {_previousState} -> {_currentState}");
    }

    private void OnStateEnter(EShadowAgentState state)
    {
        switch (state)
        {
            case EShadowAgentState.Idle:
                GenerateNewWanderTarget();
                break;

            case EShadowAgentState.Investigating:
                // Movement handled in PathUpdate
                _stateTransitionTimer.StartTimer(investigateDuration, 1);
                break;

            case EShadowAgentState.Approaching:
                // Movement handled in PathUpdate
                break;

            case EShadowAgentState.Retreating:
                // Movement handled in PathUpdate
                break;

            case EShadowAgentState.Fleeing:
                // Movement handled in PathUpdate
                _stateTransitionTimer.StartTimer(5f, 1); // Flee for 5 seconds then re-evaluate
                break;

            case EShadowAgentState.Stunned:
                _agent.speed = 0;
                _stateTransitionTimer.StartTimer(stunDuration, 1);
                break;
        }
    }

    private void EvaluateStateTransition()
    {
        if (_target == null) return;

        // Stunned state is locked until timer completes
        if (_currentState == EShadowAgentState.Stunned) return;

        // Priority 1: Memory Stone zone - immediate flee
        if (_isInMemoryStoneZone)
        {
            if (_currentState != EShadowAgentState.Fleeing && _currentState != EShadowAgentState.Stunned)
            {
                SetState(EShadowAgentState.Fleeing);
            }
            return;
        }

        // Priority 2: Very close to player - retreat
        if (_distanceToPlayer <= retreatRadius)
        {
            if (_currentState != EShadowAgentState.Retreating && _currentState != EShadowAgentState.Fleeing)
            {
                SetState(EShadowAgentState.Retreating);
            }
            return;
        }

        // Priority 3: Close to player - approach
        if (_distanceToPlayer <= approachRadius && _hasLineOfSight)
        {
            if (_currentState != EShadowAgentState.Approaching && _currentState != EShadowAgentState.Retreating)
            {
                SetState(EShadowAgentState.Approaching);
            }
            return;
        }

        // Priority 4: Investigation area - investigate
        if (_distanceToPlayer <= investigationRadius && _hasLineOfSight)
        {
            if (_currentState != EShadowAgentState.Investigating && _currentState != EShadowAgentState.Approaching)
            {
                SetState(EShadowAgentState.Investigating);
            }
            return;
        }

        // Priority 5: Default - idle
        if (_currentState != EShadowAgentState.Idle)
        {
            SetState(EShadowAgentState.Idle);
        }
    }

    private void UpdateStateTimer()
    {
        _stateTimer += Time.deltaTime;

        // Calculate proximity (0 = far, 1 = very close)
        float proximity = 1f - Mathf.Clamp01(_distanceToPlayer / investigationRadius);

        // Fire proximity event for UI/audio feedback
        OnProximityChanged?.Invoke(proximity);

        if (LanternManager.Instance == null)
            return;

        // Only apply decay effect if shadow is within approach radius
        if (_distanceToPlayer <= approachRadius)
        {
            // Calculate decay multiplier based on proximity
            // proximity = 0 (far) -> multiplier = 1x (normal decay)
            // proximity = 0.5 (medium) -> multiplier = 1.5x (50% faster)
            // proximity = 1 (very close) -> multiplier = 3x (200% faster)
            float decayMultiplier = 1f + (proximity * 8f); // Ranges from 1x to 8x

            LanternManager.Instance?.ApplyDecayMultiplier(decayMultiplier);
        }
        else
        {
            // Reset to normal decay when shadow is far
            LanternManager.Instance?.ResetDecayMultiplier();
        }
    }

    #endregion

    #region Detection Methods

    private void UpdateDistanceToPlayer()
    {
        if (_target == null) return;
        _distanceToPlayer = Vector3.Distance(transform.position, _target.transform.position);
    }

    public void TriggerOnMemoryStone()
    {
        _isInMemoryStoneZone = true;
    }

    public void ResetOnMemoryStone()
    {
        _isInMemoryStoneZone = false;
    }

    private void UpdateMemoryStoneZone()
    {
        // Check if we're inside any Memory Stone zone
        MemoryStone[] stones = FindObjectsByType<MemoryStone>(FindObjectsSortMode.None);
        _isInMemoryStoneZone = false;

        foreach (var stone in stones)
        {
            if (stone.IsActivated && Vector3.Distance(transform.position, stone.transform.position) < stone.SafeZoneRadius)
            {
                _isInMemoryStoneZone = true;
                break;
            }
        }
    }

    private void UpdateLineOfSight()
    {
        if (_target == null)
        {
            _hasLineOfSight = false;
            return;
        }

        Vector3 directionToPlayer = (_target.transform.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, _target.transform.position);

        // Check if we can see the player
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out hit, distance, obstacleMask))
        {
            _hasLineOfSight = false;
        }
        else
        {
            _hasLineOfSight = true;
        }
    }

    #endregion

    #region Navigation Methods

    private void GenerateNewWanderTarget()
    {
        if (_agent == null || !_agent.isOnNavMesh) return;

        Vector3 randomDirection = Random.insideUnitSphere * Random.Range(minWanderDistance, maxWanderDistance);
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, maxWanderDistance, NavMesh.AllAreas))
        {
            _wanderTarget = hit.position;

            // Set initial wander destination only if we're currently idle
            if (_currentState == EShadowAgentState.Idle)
            {
                _agent.SetDestination(_wanderTarget);
            }
        }
    }

    private void PathUpdateTimer_OnTimerLoop(object sender, GGTimer e)
    {
        if (_agent == null || _target == null) return;

        switch (_currentState)
        {
            case EShadowAgentState.Idle:
                HandleIdleNavigation();
                break;

            case EShadowAgentState.Investigating:
                HandleInvestigatingNavigation();
                break;

            case EShadowAgentState.Approaching:
                HandleApproachingNavigation();
                break;

            case EShadowAgentState.Retreating:
                HandleRetreatingNavigation();
                break;

            case EShadowAgentState.Fleeing:
                HandleFleeingNavigation();
                break;

            case EShadowAgentState.Stunned:
                // Movement is disabled
                _agent.speed = 0;
                break;
        }
    }

    private void HandleIdleNavigation()
    {
        if (!_agent.hasPath || _agent.remainingDistance < 0.5f)
        {
            GenerateNewWanderTarget();
        }

        _agent.speed = wanderSpeed;
        _agent.stoppingDistance = 0f;
        _agent.autoBraking = true;
        _agent.updateRotation = true;
        _agent.updatePosition = true;
    }

    private void HandleInvestigatingNavigation()
    {
        if (_target != null && _hasLineOfSight)
        {
            // Move toward the player's general direction (not directly at them)
            Vector3 directionToPlayer = (_target.transform.position - transform.position).normalized;
            Vector3 investigatePoint = transform.position + directionToPlayer * 12f;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(investigatePoint, out hit, 15f, NavMesh.AllAreas))
            {
                _investigationPoint = hit.position;
                _agent.SetDestination(_investigationPoint);
            }
        }

        _agent.speed = wanderSpeed * 0.8f;
        _agent.stoppingDistance = 0f;
    }

    private void HandleApproachingNavigation()
    {
        if (_target != null)
        {
            _agent.SetDestination(_target.transform.position);
        }

        _agent.speed = approachSpeed;
        _agent.stoppingDistance = stoppingDistance;

        // Visual feedback - increase speed when closer
        float speedMultiplier = Mathf.Lerp(1f, 1.5f, 1f - (_distanceToPlayer / approachRadius));
        _agent.speed *= speedMultiplier;
    }

    private void HandleRetreatingNavigation()
    {
        if (_target != null)
        {
            // Move directly away from the player
            Vector3 retreatDirection = (transform.position - _target.transform.position).normalized;
            Vector3 retreatPoint = transform.position + retreatDirection * 10f;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(retreatPoint, out hit, 15f, NavMesh.AllAreas))
            {
                _agent.SetDestination(hit.position);
            }
        }

        _agent.speed = retreatSpeed;
        _agent.stoppingDistance = 0f;
    }

    private void HandleFleeingNavigation()
    {
        if (_target != null)
        {
            // Flee quickly directly away from player or Memory Stone
            Vector3 fleeDirection = (transform.position - _target.transform.position).normalized;

            // If we're fleeing from Memory Stone, the target is the Memory Stone center
            if (_isInMemoryStoneZone)
            {
                MemoryStone nearestStone = GetNearestMemoryStone();
                if (nearestStone != null)
                {
                    fleeDirection = (transform.position - nearestStone.transform.position).normalized;
                }
            }

            Vector3 fleePoint = transform.position + fleeDirection * 20f;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(fleePoint, out hit, 25f, NavMesh.AllAreas))
            {
                _agent.SetDestination(hit.position);
            }
        }

        _agent.speed = fleeSpeed;
        _agent.stoppingDistance = 0f;
    }

    private MemoryStone GetNearestMemoryStone()
    {
        MemoryStone[] stones = FindObjectsByType<MemoryStone>(FindObjectsSortMode.None);
        MemoryStone nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (var stone in stones)
        {
            if (!stone.IsActivated) 
                continue;

            float dist = Vector3.Distance(transform.position, stone.transform.position);
            if (dist < nearestDistance)
            {
                nearestDistance = dist;
                nearest = stone;
            }
        }

        return nearest;
    }

    #endregion

    #region Timer Callbacks

    private void LineOfSightTimer_OnTimerLoop(object sender, GGTimer e)
    {
        UpdateLineOfSight();
    }

    private void StateTransitionTimer_OnTimerComplete(object sender, GGTimer e)
    {
        switch (_currentState)
        {
            case EShadowAgentState.Stunned:
                SetState(EShadowAgentState.Idle);
                break;

            case EShadowAgentState.Investigating:
                // Lost interest, go idle
                SetState(EShadowAgentState.Idle);
                break;

            case EShadowAgentState.Fleeing:
                // Done fleeing, re-evaluate
                ResetOnMemoryStone();
                EvaluateStateTransition();
                break;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Called when the player flashes their lantern at the shadow
    /// </summary>
    public void Stun()
    {
        if (_currentState != EShadowAgentState.Stunned)
        {
            SetState(EShadowAgentState.Stunned);
        }
    }

    /// <summary>
    /// Called when the player uses Kulintang Gong Essence
    /// </summary>
    public void Repel(float repelDistance = 15f)
    {
        if (_target != null)
        {
            Vector3 repelDirection = (transform.position - _target.transform.position).normalized;
            Vector3 repelPoint = transform.position + repelDirection * repelDistance;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(repelPoint, out hit, repelDistance + 5f, NavMesh.AllAreas))
            {
                _agent.SetDestination(hit.position);
                SetState(EShadowAgentState.Fleeing);
            }
        }
    }

    /// <summary>
    /// Force the shadow to investigate a specific point (e.g., noise from player)
    /// </summary>
    public void InvestigatePoint(Vector3 point)
    {
        _investigationPoint = point;
        SetState(EShadowAgentState.Investigating);
    }

    #endregion

    #region Debug

    private void OnDrawGizmosSelected()
    {
        // Draw detection radii
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, investigationRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, approachRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, retreatRadius);

        // Draw current destination
        if (_agent != null && _agent.hasPath)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(_agent.destination, 0.5f);
        }
    }

    #endregion
}
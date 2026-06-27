using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent))]
public class CreepAgentComponent : MonoBehaviour
{
    public event EventHandler OnWaypointReached;
    public event EventHandler<CreepAgentComponent> OnPathCompleted;
    public event EventHandler OnAllWaypointsCompleted;
    public event EventHandler<int> OnWaypointIndexChanged;
    public event EventHandler<Transform> OnTargetChanged;

    [Header("NavMesh Agent Settings")]
    [SerializeField] public float _moveSpeed = 3.5f;
    [SerializeField] private float _angularSpeed = 120f;
    [SerializeField] private float _acceleration = 8f;
    [SerializeField] private float _stoppingDistance = 0.1f;
    [SerializeField] private bool _autoBraking = true;

    [Header("Waypoint Settings")]
    [SerializeField] private List<Transform> _waypointTransforms = new List<Transform>();
    [SerializeField] public float _waitTimeAtWaypoints = 0f; // Time to pause at each waypoint

    [Header("State")]
    [SerializeField] private GameObject _projectileDamagerPrefab;
    [SerializeField] private int _damageValue = 100;
    [SerializeField] private float _explosionRadius = 3f;
    [SerializeField] private bool _isActive = true;
    [SerializeField] private bool _debugDrawWaypoints = true;

    // Private variables
    private NavMeshAgent _navMeshAgent;
    private List<Vector3> _waypointPositions = new List<Vector3>();
    private int _currentWaypointIndex = 0;
    private int _directionMultiplier = 1; // 1 for forward, -1 for reverse
    private bool _isWaiting = false;
    private float _waitTimer = 0f;
    private bool _isInitialized = false;
    private Transform _customTarget;

    // Properties
    public NavMeshAgent Agent => _navMeshAgent;
    public List<Vector3> Waypoints => _waypointPositions;
    public int CurrentWaypointIndex => _currentWaypointIndex;
    public Vector3 CurrentTarget => GetCurrentTarget();
    public bool HasWaypoints => _waypointPositions != null && _waypointPositions.Count > 0;
    public bool IsMoving => _isActive && !_isWaiting && _navMeshAgent.hasPath && _navMeshAgent.remainingDistance > _stoppingDistance;
    public bool HasReachedDestination
    {
        get { return _navMeshAgent.remainingDistance <= _stoppingDistance; }
    }

    public float Progress => HasWaypoints ? (float)_currentWaypointIndex / (_waypointPositions.Count - 1) : 0f;
    public float RemainingDistance => _navMeshAgent.remainingDistance;
    public float CurrentSpeed => _navMeshAgent.velocity.magnitude;
    public bool IsWaiting => _isWaiting;

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        InitializeNavMeshAgent();
        InitializeWaypoints();
    }

    private void InitializeNavMeshAgent()
    {
        if (_navMeshAgent == null)
        {
            Debug.LogError($"NavMeshAgent missing on {gameObject.name}");
            return;
        }

        // Apply settings
        //_navMeshAgent.speed = _moveSpeed;
        _navMeshAgent.angularSpeed = _angularSpeed;
        //_navMeshAgent.acceleration = _acceleration;
        //_navMeshAgent.stoppingDistance = _stoppingDistance;
        _navMeshAgent.autoBraking = _autoBraking;

        _isInitialized = true;
    }

    private void InitializeWaypoints()
    {
        // Convert transforms to positions
        UpdateWaypointPositions();

        if (!HasWaypoints)
            return;

        if (_isActive)
        {
            SetDestination(_waypointPositions[_currentWaypointIndex]);
        }
    }

    private void Update()
    {
        if (!_isInitialized || !_isActive)
            return;

        // Handle waiting at waypoints
        if (_isWaiting)
        {
            UpdateWaiting();
            return;
        }

        // Check if we've reached the current destination
        if (HasWaypoints && HasReachedDestination && !_isWaiting)
        {
            HandleWaypointReached();
        }
    }

    private void UpdateWaiting()
    {
        _waitTimer -= Time.deltaTime;

        if (_waitTimer <= 0f)
        {
            _isWaiting = false;
            MoveToNextWaypoint();
        }
    }

    private void HandleWaypointReached()
    {
        // Stop the agent
        _navMeshAgent.ResetPath();

        OnWaypointReached?.Invoke(this, EventArgs.Empty);

        // Handle waiting at this waypoint
        if (_waitTimeAtWaypoints > 0f && !IsLastWaypoint())
        {
            _isWaiting = true;
            _waitTimer = _waitTimeAtWaypoints;
        }
        else
        {
            MoveToNextWaypoint();
        }
    }

    private void MoveToNextWaypoint()
    {
        if (!HasWaypoints)
            return;

        int nextIndex = _currentWaypointIndex + _directionMultiplier;

        // Check if we've completed the path
        if (nextIndex >= _waypointPositions.Count || nextIndex < 0)
        {
            _isActive = false;
            _navMeshAgent.isStopped = true;
            OnAllWaypointsCompleted?.Invoke(this, EventArgs.Empty);

            OnPathCompleted?.Invoke(this, this);
        }
        else
        {
            _currentWaypointIndex = nextIndex;
            OnWaypointIndexChanged?.Invoke(this, _currentWaypointIndex);
            SetDestination(_waypointPositions[_currentWaypointIndex]);
        }
    }

    private void SetDestination(Vector3 destination)
    {
        if (!_navMeshAgent.isOnNavMesh)
        {
            Debug.LogWarning($"NavMeshAgent on {gameObject.name} is not on the NavMesh");
            return;
        }

        _navMeshAgent.SetDestination(destination);
        OnTargetChanged?.Invoke(this, null); // No transform for position targets
    }

    private Vector3 GetCurrentTarget()
    {
        if (_customTarget != null)
            return _customTarget.position;

        if (HasWaypoints && _currentWaypointIndex < _waypointPositions.Count)
            return _waypointPositions[_currentWaypointIndex];

        return transform.position;
    }

    private bool IsLastWaypoint()
    {
        if (_directionMultiplier > 0)
            return _currentWaypointIndex >= _waypointPositions.Count - 1;
        else
            return _currentWaypointIndex <= 0;
    }

    private void UpdateWaypointPositions()
    {
        _waypointPositions.Clear();
        foreach (var t in _waypointTransforms)
        {
            if (t != null)
                _waypointPositions.Add(t.position);
        }
    }

    // Public API Methods

    public void Boom()
    {
        // This will damage base
        GameObject projectileDamagerObject = Instantiate(_projectileDamagerPrefab, transform.position, transform.rotation);
        ProjectileDamager projectileDamager = projectileDamagerObject.GetComponent<ProjectileDamager>();
        projectileDamager.DamageRadius = _explosionRadius;
        projectileDamager.DamageValue = _damageValue;
        projectileDamager.Splode();
    }


    /// <summary>
    /// Set a new path using Transform waypoints
    /// </summary>
    public void SetWaypoints(List<Transform> waypoints)
    {
        _waypointTransforms = new List<Transform>(waypoints);
        UpdateWaypointPositions();
        ResetPath();
    }

    /// <summary>
    /// Set a new path using Vector3 positions
    /// </summary>
    public void SetWaypointsFromPositions(List<Vector3> waypoints)
    {
        _waypointPositions = new List<Vector3>(waypoints);
        _waypointTransforms.Clear(); // Clear transforms since we're using positions
        ResetPath();
    }

    /// <summary>
    /// Set a single target (overrides waypoint system)
    /// </summary>
    public void SetTarget(Transform target)
    {
        _customTarget = target;
        _waypointPositions.Clear();

        if (target != null && _navMeshAgent.isOnNavMesh)
        {
            _navMeshAgent.SetDestination(target.position);
        }
    }

    /// <summary>
    /// Set a single position target (overrides waypoint system)
    /// </summary>
    public void SetTargetPosition(Vector3 position)
    {
        _customTarget = null;
        _waypointPositions.Clear();

        if (_navMeshAgent.isOnNavMesh)
        {
            _navMeshAgent.SetDestination(position);
        }
    }

    /// <summary>
    /// Clear custom target and return to waypoint system
    /// </summary>
    public void ClearTarget()
    {
        _customTarget = null;

        if (HasWaypoints && _navMeshAgent.isOnNavMesh)
        {
            SetDestination(_waypointPositions[_currentWaypointIndex]);
        }
    }

    /// <summary>
    /// Reset to the beginning of the waypoint path
    /// </summary>
    public void ResetPath()
    {
        _currentWaypointIndex = 0;
        _directionMultiplier = 1;
        _isWaiting = false;
        _isActive = true;
        _customTarget = null;

        if (HasWaypoints && _navMeshAgent.isOnNavMesh)
        {
            SetDestination(_waypointPositions[_currentWaypointIndex]);
        }
    }

    /// <summary>
    /// Add a waypoint to the end of the path
    /// </summary>
    public void AddWaypoint(Transform waypoint)
    {
        if (waypoint != null)
        {
            _waypointTransforms.Add(waypoint);
            _waypointPositions.Add(waypoint.position);
        }
    }

    /// <summary>
    /// Insert a waypoint at a specific index
    /// </summary>
    public void InsertWaypoint(int index, Transform waypoint)
    {
        if (waypoint != null && index >= 0 && index <= _waypointPositions.Count)
        {
            _waypointTransforms.Insert(index, waypoint);
            _waypointPositions.Insert(index, waypoint.position);

            // Adjust current index if we inserted before it
            if (index <= _currentWaypointIndex)
                _currentWaypointIndex++;
        }
    }

    /// <summary>
    /// Remove a waypoint at the specified index
    /// </summary>
    public void RemoveWaypoint(int index)
    {
        if (index >= 0 && index < _waypointPositions.Count)
        {
            if (index < _waypointTransforms.Count)
                _waypointTransforms.RemoveAt(index);

            _waypointPositions.RemoveAt(index);

            // Adjust current index if we removed before or at it
            if (index < _currentWaypointIndex)
                _currentWaypointIndex--;
            else if (index == _currentWaypointIndex)
            {
                _currentWaypointIndex = Mathf.Clamp(_currentWaypointIndex, 0, _waypointPositions.Count - 1);
                if (HasWaypoints)
                    SetDestination(_waypointPositions[_currentWaypointIndex]);
            }
        }
    }

    /// <summary>
    /// Clear all waypoints and stop movement
    /// </summary>
    public void ClearWaypoints()
    {
        _waypointTransforms.Clear();
        _waypointPositions.Clear();
        _isActive = false;
        _navMeshAgent.ResetPath();
    }

    /// <summary>
    /// Teleport to a specific waypoint
    /// </summary>
    public void TeleportToWaypoint(int index)
    {
        if (index >= 0 && index < _waypointPositions.Count && _navMeshAgent.isOnNavMesh)
        {
            _navMeshAgent.Warp(_waypointPositions[index]);
            _currentWaypointIndex = index;
            OnWaypointIndexChanged?.Invoke(this, _currentWaypointIndex);
        }
    }

    /// <summary>
    /// Pause/unpause movement
    /// </summary>
    public void SetActive(bool active)
    {
        _isActive = active;
        _navMeshAgent.isStopped = !active;
    }

    /// <summary>
    /// Temporarily stop the agent
    /// </summary>
    public void Stop()
    {
        _navMeshAgent.isStopped = true;
    }

    /// <summary>
    /// Resume movement
    /// </summary>
    public void Resume()
    {
        if (_isActive)
        {
            _navMeshAgent.isStopped = false;
        }
    }

    /// <summary>
    /// Get the total path distance remaining
    /// </summary>
    public float GetRemainingPathDistance()
    {
        if (!HasWaypoints || _navMeshAgent.path == null)
            return 0f;

        float distance = _navMeshAgent.remainingDistance;

        // Add distances between remaining waypoints
        for (int i = _currentWaypointIndex + 1; i < _waypointPositions.Count; i++)
        {
            distance += Vector3.Distance(_waypointPositions[i - 1], _waypointPositions[i]);
        }

        return distance;
    }

    /// <summary>
    /// Check if the NavMeshAgent is on a valid NavMesh
    /// </summary>
    public bool IsOnNavMesh()
    {
        return _navMeshAgent.isOnNavMesh;
    }

    private void OnDrawGizmosSelected()
    {
        if (!_debugDrawWaypoints)
            return;

        // Draw waypoints from transforms
        List<Vector3> positionsToDraw = _waypointPositions;
        if (positionsToDraw == null || positionsToDraw.Count == 0)
        {
            positionsToDraw = new List<Vector3>();
            foreach (var t in _waypointTransforms)
            {
                if (t != null)
                    positionsToDraw.Add(t.position);
            }
        }

        // Draw waypoints
        Gizmos.color = Color.cyan;
        for (int i = 0; i < positionsToDraw.Count; i++)
        {
            // Draw waypoint sphere
            Gizmos.DrawWireSphere(positionsToDraw[i], 0.3f);

            // Draw connections
            if (i < positionsToDraw.Count - 1)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(positionsToDraw[i], positionsToDraw[i + 1]);
            }

            // Highlight current target
            if (i == _currentWaypointIndex && Application.isPlaying)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(positionsToDraw[i], 0.2f);
                Gizmos.DrawLine(transform.position, positionsToDraw[i]);
            }

            // Draw waypoint index
#if UNITY_EDITOR
            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(positionsToDraw[i] + Vector3.up * 0.5f, i.ToString());
#endif
        }
    }
}
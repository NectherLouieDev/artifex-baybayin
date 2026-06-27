using System;
using System.Collections.Generic;
using UnityEngine;

public class CreepSpawner : MonoBehaviour
{
    [SerializeField] private List<Transform> _patrolPath;
    [SerializeField] private Transform _baseTarget;
    [SerializeField] private bool _useBaseTarget = true;

    // Events
    public event EventHandler<GameObject> OnEnemySpawned;

    public GameObject SpawnEnemy(GameObject creepPrefab)
    {
        if (creepPrefab == null)
        {
            Debug.LogError("Creep prefab is null!");
            return null;
        }

        if (_patrolPath == null || _patrolPath.Count == 0)
        {
            Debug.LogError("No patrol path configured in CreepSpawner!");
            return null;
        }

        // Instantiate at first path point
        GameObject creep = Instantiate(creepPrefab, _patrolPath[0].position, Quaternion.identity);
        CreepAgentComponent creepAgent = creep.GetComponent<CreepAgentComponent>();

        if (creepAgent == null)
        {
            Debug.LogError("Spawned creep has no CreepAgentComponent!");
            Destroy(creep);
            return null;
        }

        // Subscribe to path completed event for base damage
        creepAgent.OnPathCompleted += CreepAgent_OnPathCompleted;

        // Set up the creep's path
        if (_useBaseTarget && _baseTarget != null)
        {
            // For enemies that go straight to base
            creepAgent.SetTarget(_baseTarget);
        }
        else
        {
            // For patrolling enemies
            creepAgent.SetWaypoints(_patrolPath);
        }

        // Fire spawn event
        OnEnemySpawned?.Invoke(this, creep);

        return creep;
    }

    private void CreepAgent_OnPathCompleted(object sender, CreepAgentComponent e)
    {
        // Enemy reached the end of path (base)
        e.Boom();

        if (e.TryGetComponent(out HealthComponent healthComponent))
        {
            healthComponent.TakeDamage(1000);
        }
    }

    // Editor visualization
    private void OnDrawGizmosSelected()
    {
        if (_patrolPath == null || _patrolPath.Count == 0)
            return;

        Gizmos.color = Color.magenta;

        // Draw path
        for (int i = 0; i < _patrolPath.Count; i++)
        {
            if (_patrolPath[i] == null)
                continue;

            // Draw sphere at each point
            Gizmos.DrawWireSphere(_patrolPath[i].position, 0.3f);

            // Draw line to next point
            if (i < _patrolPath.Count - 1 && _patrolPath[i + 1] != null)
            {
                Gizmos.DrawLine(_patrolPath[i].position, _patrolPath[i + 1].position);
            }
        }

        // Draw line to base target if used
        if (_useBaseTarget && _baseTarget != null && _patrolPath.Count > 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_patrolPath[_patrolPath.Count - 1].position, _baseTarget.position);
        }
    }
}
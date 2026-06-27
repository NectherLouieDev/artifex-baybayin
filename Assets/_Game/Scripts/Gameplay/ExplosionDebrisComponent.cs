using UnityEngine;
using System.Collections.Generic;

using Random = UnityEngine.Random;

public class ExplosionDebrisComponent : MonoBehaviour
{
    [Header("Debris Settings")]
    [SerializeField] private GameObject[] _debrisPrefabs;
    [SerializeField] private int _minDebrisCount = 3;
    [SerializeField] private int _maxDebrisCount = 8;
    [SerializeField] private float _debrisForceMin = 5f;
    [SerializeField] private float _debrisForceMax = 15f;
    [SerializeField] private float _debrisTorqueMin = 10f;
    [SerializeField] private float _debrisTorqueMax = 50f;
    [SerializeField] private float _debrisLifetime = 3f;
    [SerializeField] private Transform _debrisSpawnPoint;

    [Header("Fragmentation")]
    [SerializeField] private bool _fragmentIntoBlocks = false;
    [SerializeField] private GameObject[] _fragmentBlocks;
    [SerializeField] private float _fragmentForce = 10f;
    [SerializeField] private float _fragmentTorque = 20f;

    [Header("Explosion Force")]
    [SerializeField] private bool _applyExplosionForce = true;
    [SerializeField] private float _explosionRadius = 5f;
    [SerializeField] private float _explosionForce = 500f;
    [SerializeField] private LayerMask _affectedLayers = -1;

    [Header("Auto Cleanup")]
    [SerializeField] private bool _destroyParentOnComplete = true;
    [SerializeField] private float _destroyDelay = 2f;

    private void Start()
    {
        if (_debrisSpawnPoint == null)
        {
            _debrisSpawnPoint = transform;
        }
    }

    public void TriggerExplosion()
    {
        // Apply explosion force to nearby rigidbodies
        if (_applyExplosionForce)
        {
            ApplyExplosionForce();
        }

        // Spawn debris particles
        SpawnDebris();

        // Fragment into smaller blocks
        if (_fragmentIntoBlocks && _fragmentBlocks.Length > 0)
        {
            FragmentIntoBlocks();
        }
    }

    private void SpawnDebris()
    {
        if (_debrisPrefabs == null || _debrisPrefabs.Length == 0)
        {
            CreateSimpleDebris();
            return;
        }

        int debrisCount = Random.Range(_minDebrisCount, _maxDebrisCount + 1);

        for (int i = 0; i < debrisCount; i++)
        {
            // Randomly select a debris prefab
            GameObject debrisPrefab = _debrisPrefabs[Random.Range(0, _debrisPrefabs.Length)];

            // Spawn debris
            GameObject debris = Instantiate(debrisPrefab, _debrisSpawnPoint.position, Random.rotation);

            float randomScale = Random.Range(0.5f, 0.75f);
            debris.transform.localScale = new Vector3(randomScale, randomScale, randomScale);

            // Add rigidbody if not present
            Rigidbody rb = debris.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = debris.AddComponent<Rigidbody>();
            }

            // Apply random force
            Vector3 randomDirection = Random.onUnitSphere;
            float force = Random.Range(_debrisForceMin, _debrisForceMax);
            rb.AddForce(randomDirection * force, ForceMode.Impulse);

            // Apply random torque
            Vector3 randomTorque = new Vector3(
                Random.Range(-_debrisTorqueMax, _debrisTorqueMax),
                Random.Range(-_debrisTorqueMax, _debrisTorqueMax),
                Random.Range(-_debrisTorqueMax, _debrisTorqueMax)
            );
            rb.AddTorque(randomTorque, ForceMode.Impulse);

            // Auto-destroy debris after lifetime
            Destroy(debris, _debrisLifetime);
        }
    }

    private void CreateSimpleDebris()
    {
        int debrisCount = Random.Range(_minDebrisCount, _maxDebrisCount + 1);

        for (int i = 0; i < debrisCount; i++)
        {
            // Create a simple cube as debris
            GameObject debris = GameObject.CreatePrimitive(PrimitiveType.Cube);
            debris.transform.position = _debrisSpawnPoint.position;
            debris.transform.localScale = Vector3.one * Random.Range(0.2f, 0.5f);

            // Add random material color
            Renderer renderer = debris.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(
                    Random.Range(0.5f, 1f),
                    Random.Range(0.3f, 0.7f),
                    Random.Range(0.2f, 0.5f)
                );
            }

            // Add rigidbody
            Rigidbody rb = debris.AddComponent<Rigidbody>();

            // Apply random force
            Vector3 randomDirection = Random.onUnitSphere;
            float force = Random.Range(_debrisForceMin, _debrisForceMax);
            rb.AddForce(randomDirection * force, ForceMode.Impulse);

            // Apply random torque
            Vector3 randomTorque = new Vector3(
                Random.Range(-_debrisTorqueMax, _debrisTorqueMax),
                Random.Range(-_debrisTorqueMax, _debrisTorqueMax),
                Random.Range(-_debrisTorqueMax, _debrisTorqueMax)
            );
            rb.AddTorque(randomTorque, ForceMode.Impulse);

            // Auto-destroy debris
            Destroy(debris, _debrisLifetime);
        }
    }

    private void FragmentIntoBlocks()
    {
        foreach (GameObject fragment in _fragmentBlocks)
        {
            if (fragment != null)
            {
                // Instantiate fragment
                GameObject fragmentInstance = Instantiate(fragment, transform.position, transform.rotation);

                // Add explosive force to fragment
                Rigidbody rb = fragmentInstance.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 randomDirection = Random.onUnitSphere;
                    rb.AddForce(randomDirection * _fragmentForce, ForceMode.Impulse);

                    Vector3 randomTorque = new Vector3(
                        Random.Range(-_fragmentTorque, _fragmentTorque),
                        Random.Range(-_fragmentTorque, _fragmentTorque),
                        Random.Range(-_fragmentTorque, _fragmentTorque)
                    );
                    rb.AddTorque(randomTorque, ForceMode.Impulse);
                }
            }
        }
    }

    private void ApplyExplosionForce()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, _explosionRadius, _affectedLayers);

        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Apply explosion force
                rb.AddExplosionForce(_explosionForce, transform.position, _explosionRadius, 1f, ForceMode.Impulse);
            }
        }
    }

    // Visualize explosion radius in editor
    private void OnDrawGizmosSelected()
    {
        if (_applyExplosionForce)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _explosionRadius);
        }
    }

    // Public method to set debris spawn point dynamically
    public void SetDebrisSpawnPoint(Transform spawnPoint)
    {
        _debrisSpawnPoint = spawnPoint;
    }
}
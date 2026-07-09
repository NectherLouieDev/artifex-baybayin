using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using Random = UnityEngine.Random;

public class EmberlightStation : MonoBehaviour
{
    [Header("Station Configuration")]
    [SerializeField] private string stationName = "Emberlight Station";
    [SerializeField] private int maxEmberlights = 5;
    [SerializeField] private float spawnRadius = 2f;
    [SerializeField] private float spawnIntervalMin = 8f;
    [SerializeField] private float spawnIntervalMax = 16f;
    [SerializeField] private float spawnForce = 5f;

    [Header("Emberlight Prefab")]
    [SerializeField] private GameObject emberlightPrefab;
    [SerializeField] private Transform spawnParent;

    [Header("Visual Feedback")]
    [SerializeField] private Light stationGlow;
    [SerializeField] private ParticleSystem spawnParticles;
    [SerializeField] private ParticleSystem idleParticles;
    [SerializeField] private MeshRenderer stationMesh;
    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material depletedMaterial;
    [SerializeField] private AudioSource stationAudio;
    [SerializeField] private AudioClip spawnSound;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip depletedSound;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    // Runtime data
    private List<GameObject> spawnedEmberlights = new List<GameObject>();

    // Spawning timer
    private GGTimer spawnTimer;
    private int spawnCount = 0;

    // Events
    public Action<int> OnEmberlightSpawned;
    public Action OnStationDepleted;
    public Action OnStationRefilled;

    void Start()
    {
        // Setup spawn timer
        spawnTimer = gameObject.AddComponent<GGTimer>();
        spawnTimer.timerId = $"{stationName}_SpawnTimer";
        spawnTimer.OnTimerLoop += OnSpawnTimerLoop;
        spawnTimer.OnTimerCompleted += OnSpawnTimerCompleted;

        // Initial spawn
        SpawnEmberlight();

        // Update UI
        UpdateUI();

        // Start idle animation
        if (idleParticles != null)
            idleParticles.Play();
    }

    void Update()
    {
        // Update visual feedback
        UpdateVisuals();
    }

    #region Spawn Management

    void SpawnEmberlight()
    {
        if (spawnCount >= maxEmberlights)
        {
            // Stop spawning if we've reached max
            if (spawnTimer.IsRunning())
                spawnTimer.StopTimer();
            return;
        }

        if (emberlightPrefab == null)
        {
            Debug.LogError($"[{stationName}] Emberlight prefab not assigned!");
            return;
        }

        // Generate random position within radius
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0.5f, randomCircle.y);
        spawnPos.y += UnityEngine.Random.Range(-0.2f, 0.3f);

        // Instantiate emberlight
        GameObject emberlight = Instantiate(emberlightPrefab, spawnPos, Quaternion.identity);

        // Parent for organization
        if (spawnParent != null)
            emberlight.transform.parent = spawnParent;
        else
            emberlight.transform.parent = transform;

        // Get Emberlight component and set station reference
        Emberlight emberlightComponent = emberlight.GetComponent<Emberlight>();
        if (emberlightComponent == null)
        {
            emberlightComponent = emberlight.AddComponent<Emberlight>();
        }
        emberlightComponent.SetStation(this);

        // Apply force to make it fly outward
        Rigidbody rb = emberlight.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 randomDirection = Vector3.up + new Vector3(randomCircle.x, UnityEngine.Random.Range(2f, 4f), randomCircle.y).normalized;
            rb.AddForce(randomDirection * spawnForce, ForceMode.Impulse);
        }

        // Add to list
        spawnedEmberlights.Add(emberlight);
        spawnCount++;

        // Play spawn effect
        if (spawnParticles != null)
        {
            spawnParticles.transform.position = spawnPos;
            spawnParticles.Play();
        }

        if (spawnSound != null && stationAudio != null)
        {
            stationAudio.PlayOneShot(spawnSound, 0.5f);
        }

        if (showDebugLogs)
            Debug.Log($"[{stationName}] Spawned emberlight ({spawnCount}/{maxEmberlights})");

        OnEmberlightSpawned?.Invoke(spawnCount);
        UpdateUI();

        // Check if we've reached max
        if (spawnCount >= maxEmberlights)
        {
            OnStationRefilled?.Invoke();
            if (showDebugLogs)
                Debug.Log($"[{stationName}] Station is fully stocked!");
        }
        else
        {
            // Start timer for next spawn
            if (!spawnTimer.IsRunning())
            {
                spawnTimer.StartTimer(GetSpawnInterval(), 0); // 0 = infinite loops
            }
        }
    }

    private float GetSpawnInterval()
    {
        float spawnInterval = Random.Range(spawnIntervalMin, spawnIntervalMax);
        return spawnInterval;
    }

    void OnSpawnTimerLoop(object sender, GGTimer timer)
    {
        // Only spawn if we haven't reached max and not depleted
        if (spawnCount < maxEmberlights)
        {
            SpawnEmberlight();
        }
        else if (spawnCount >= maxEmberlights)
        {
            // Stop timer if we've reached max
            spawnTimer.StopTimer();
        }
    }

    void OnSpawnTimerCompleted(object sender, GGTimer timer)
    {
        // This shouldn't happen with infinite loops, but just in case
        if (spawnCount < maxEmberlights)
        {
            SpawnEmberlight();
        }
    }

    #endregion

    #region Station Interaction

    public void OnEmberlightPickedUp(GameObject emberlight)
    {
        if (spawnedEmberlights.Contains(emberlight))
        {
            spawnedEmberlights.Remove(emberlight);
            spawnCount--;

            if (pickupSound != null && stationAudio != null)
            {
                stationAudio.PlayOneShot(pickupSound, 0.7f);
            }

            if (showDebugLogs)
                Debug.Log($"[{stationName}] Emberlight picked up. Remaining: {spawnCount}");

            if (!spawnTimer.IsRunning() && spawnCount < maxEmberlights)
            {
                spawnTimer.StartTimer(GetSpawnInterval(), 0);
            }

            UpdateUI();
            UpdateVisuals();
        }
    }

    void SetDepleted()
    {
        if (depletedSound != null && stationAudio != null)
        {
            stationAudio.PlayOneShot(depletedSound);
        }

        if (showDebugLogs)
            Debug.Log($"[{stationName}] Station depleted. Respawning started.");

        OnStationDepleted?.Invoke();
        UpdateUI();
        UpdateVisuals();

        // Start respawning if timer isn't already running
        if (!spawnTimer.IsRunning())
        {
            spawnTimer.StartTimer(GetSpawnInterval(), 0);
        }

        // Show message to player
        UIManager.Instance?.ShowMessage($"Emberlight Station depleted. Respawning...");
    }

    #endregion

    #region Visual Feedback

    void UpdateVisuals()
    {
        float fuelRatio = (float)spawnCount / maxEmberlights;

        // Update station glow
        if (stationGlow != null)
        {
            stationGlow.intensity = Mathf.Lerp(0.2f, 2f, fuelRatio);
            stationGlow.color = Color.Lerp(new Color(0.5f, 0.3f, 0.1f), new Color(1f, 0.8f, 0.3f), fuelRatio);
        }

        // Update material
        if (stationMesh != null && activeMaterial != null && depletedMaterial != null)
        {
            stationMesh.material = activeMaterial;
        }

        // Update idle particles based on count
        if (idleParticles != null)
        {
            var emission = idleParticles.emission;
            emission.rateOverTime = Mathf.Lerp(0f, 20f, fuelRatio);

            var main = idleParticles.main;
            main.startColor = Color.Lerp(new Color(0.5f, 0.3f, 0.1f), new Color(1f, 0.8f, 0.3f), fuelRatio);
        }
    }

    void UpdateUI()
    {
        // UpdateUI
    }

    #endregion

    #region Getters

    public int GetEmberlightCount()
    {
        return spawnCount;
    }

    public int GetMaxEmberlights()
    {
        return maxEmberlights;
    }

    public List<GameObject> GetSpawnedEmberlights()
    {
        return spawnedEmberlights;
    }

    #endregion

    #region Editor Debug

    void OnDrawGizmosSelected()
    {
        // Draw spawn radius
        Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }

    #endregion
}
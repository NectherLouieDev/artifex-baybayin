using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EmberlightStation : MonoBehaviour
{
    [Header("Station Configuration")]
    [SerializeField] private string stationName = "Emberlight Station";
    [SerializeField] private int maxEmberlights = 5;
    [SerializeField] private int currentEmberlightCount = 0;
    [SerializeField] private float spawnRadius = 2f;
    [SerializeField] private float respawnDelay = 10f; // Time before respawning spent emberlights

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

    [Header("UI")]
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private TextMeshProUGUI emberlightCountText;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    // Runtime data
    private List<GameObject> spawnedEmberlights = new List<GameObject>();
    private List<Vector3> spawnPositions = new List<Vector3>();
    private bool isPlayerInRange = false;
    private bool isDepleted = false;
    private float respawnTimer = 0f;
    private bool isRespawning = false;

    // Events
    public System.Action<int> OnEmberlightSpawned;
    //public System.Action<int> OnEmberlightPickedUp;
    public System.Action OnStationDepleted;
    public System.Action OnStationRefilled;

    void Start()
    {
        // Generate spawn positions
        GenerateSpawnPositions();

        // Initial spawn
        SpawnAllEmberlights();

        // Update UI
        UpdateUI();

        // Start idle animation
        if (idleParticles != null)
            idleParticles.Play();

        // Hide interact prompt initially
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    void Update()
    {
        // Check for player input when in range
        if (isPlayerInRange && !isDepleted)
        {
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            {
                TriggerStation();
            }
        }

        // Handle respawning if depleted
        if (isDepleted && isRespawning)
        {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0f)
            {
                RespawnEmberlights();
            }
        }

        // Update glowing effect based on count
        UpdateVisuals();
    }

    #region Spawn Management

    void GenerateSpawnPositions()
    {
        spawnPositions.Clear();

        for (int i = 0; i < maxEmberlights; i++)
        {
            // Generate random position within radius
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0.5f, randomCircle.y);

            // Add some height variation
            spawnPos.y += Random.Range(-0.2f, 0.3f);

            spawnPositions.Add(spawnPos);
        }
    }

    void SpawnAllEmberlights()
    {
        // Clear existing
        ClearSpawnedEmberlights();

        currentEmberlightCount = 0;
        spawnedEmberlights.Clear();

        // Spawn at each position
        for (int i = 0; i < spawnPositions.Count; i++)
        {
            SpawnEmberlightAt(i);
        }

        isDepleted = false;
        isRespawning = false;

        if (showDebugLogs)
            Debug.Log($"[{stationName}] Spawned {currentEmberlightCount} emberlights");

        OnEmberlightSpawned?.Invoke(currentEmberlightCount);
        UpdateUI();
    }

    void SpawnEmberlightAt(int index)
    {
        if (index >= spawnPositions.Count) return;
        if (index >= maxEmberlights) return;
        if (currentEmberlightCount >= maxEmberlights) return;

        if (emberlightPrefab == null)
        {
            Debug.LogError($"[{stationName}] Emberlight prefab not assigned!");
            return;
        }

        // Instantiate emberlight
        GameObject emberlight = Instantiate(emberlightPrefab, spawnPositions[index], Quaternion.identity);

        // Parent for organization
        if (spawnParent != null)
            emberlight.transform.parent = spawnParent;
        else
            emberlight.transform.parent = transform;

        // Add pickup script if not already attached
        EmberlightPickup pickup = emberlight.GetComponent<EmberlightPickup>();
        if (pickup == null)
        {
            pickup = emberlight.AddComponent<EmberlightPickup>();
        }

        // Set reference to this station
        pickup.SetStation(this);

        // Add to list
        spawnedEmberlights.Add(emberlight);
        currentEmberlightCount++;

        // Play spawn effect
        if (spawnParticles != null)
        {
            spawnParticles.transform.position = spawnPositions[index];
            spawnParticles.Play();
        }

        if (spawnSound != null && stationAudio != null)
        {
            stationAudio.PlayOneShot(spawnSound, 0.5f);
        }
    }

    void ClearSpawnedEmberlights()
    {
        foreach (GameObject emberlight in spawnedEmberlights)
        {
            if (emberlight != null)
                Destroy(emberlight);
        }
        spawnedEmberlights.Clear();
        currentEmberlightCount = 0;
    }

    #endregion

    #region Station Interaction

    void TriggerStation()
    {
        if (isDepleted)
        {
            if (showDebugLogs)
                Debug.Log($"[{stationName}] Station is depleted. Waiting for respawn.");
            return;
        }

        if (currentEmberlightCount <= 0)
        {
            SetDepleted();
            return;
        }

        // Can't trigger if no emberlights left
        // Individual pickup handles the rest
    }

    public void OnEmberlightPickedUp(GameObject emberlight)
    {
        if (spawnedEmberlights.Contains(emberlight))
        {
            spawnedEmberlights.Remove(emberlight);
            currentEmberlightCount--;

            if (pickupSound != null && stationAudio != null)
            {
                stationAudio.PlayOneShot(pickupSound, 0.7f);
            }

            if (showDebugLogs)
                Debug.Log($"[{stationName}] Emberlight picked up. Remaining: {currentEmberlightCount}");

            //OnEmberlightPickedUp?.Invoke(currentEmberlightCount);

            // Check if depleted
            if (currentEmberlightCount <= 0)
            {
                SetDepleted();
            }

            UpdateUI();
            UpdateVisuals();
        }
    }

    void SetDepleted()
    {
        isDepleted = true;
        isRespawning = true;
        respawnTimer = respawnDelay;

        if (depletedSound != null && stationAudio != null)
        {
            stationAudio.PlayOneShot(depletedSound);
        }

        if (showDebugLogs)
            Debug.Log($"[{stationName}] Station depleted. Respawn in {respawnDelay} seconds.");

        OnStationDepleted?.Invoke();
        UpdateUI();
        UpdateVisuals();

        // Show message to player
        UIManager.Instance?.ShowMessage($"Emberlight Station depleted. Respawning in {respawnDelay}s...");
    }

    void RespawnEmberlights()
    {
        isRespawning = false;

        // Regenerate spawn positions slightly to avoid same spots
        for (int i = 0; i < spawnPositions.Count; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            spawnPositions[i] = transform.position + new Vector3(randomCircle.x, 0.5f, randomCircle.y);

            float _y = spawnPositions[i].y;
            _y += Random.Range(-0.2f, 0.3f);
            spawnPositions[i] = new Vector3(spawnPositions[i].x, _y, spawnPositions[i].z);
        }

        SpawnAllEmberlights();

        if (showDebugLogs)
            Debug.Log($"[{stationName}] Station refilled!");

        OnStationRefilled?.Invoke();

        // Show message to player
        UIManager.Instance?.ShowMessage($"Emberlight Station refilled!");
    }

    #endregion

    #region Player Detection

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;

            if (interactPrompt != null)
            {
                interactPrompt.SetActive(true);
                // Update prompt text based on station state
                if (isDepleted)
                {
                    UpdateInteractPrompt("Depleted - Wait for respawn");
                }
                else if (currentEmberlightCount > 0)
                {
                    UpdateInteractPrompt($"Press E to collect Emberlight ({currentEmberlightCount} available)");
                }
                else
                {
                    UpdateInteractPrompt("No Emberlights available");
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;

            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }
        }
    }

    void UpdateInteractPrompt(string text)
    {
        if (interactPrompt != null)
        {
            TextMeshProUGUI promptText = interactPrompt.GetComponentInChildren<TextMeshProUGUI>();
            if (promptText != null)
            {
                promptText.text = text;
            }
        }
    }

    #endregion

    #region Visual Feedback

    void UpdateVisuals()
    {
        float fuelRatio = (float)currentEmberlightCount / maxEmberlights;

        // Update station glow
        if (stationGlow != null)
        {
            stationGlow.intensity = Mathf.Lerp(0.2f, 2f, fuelRatio);
            stationGlow.color = Color.Lerp(new Color(0.5f, 0.3f, 0.1f), new Color(1f, 0.8f, 0.3f), fuelRatio);

            // Dim when depleted
            if (isDepleted)
            {
                stationGlow.intensity = 0.1f;
            }
        }

        // Update material
        if (stationMesh != null && activeMaterial != null && depletedMaterial != null)
        {
            if (isDepleted || currentEmberlightCount <= 0)
            {
                stationMesh.material = depletedMaterial;
            }
            else
            {
                stationMesh.material = activeMaterial;
            }
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
        if (emberlightCountText != null)
        {
            if (isDepleted)
            {
                emberlightCountText.text = "⏳ Depleted";
                emberlightCountText.color = Color.red;
            }
            else
            {
                emberlightCountText.text = $"✦ {currentEmberlightCount} / {maxEmberlights}";
                emberlightCountText.color = Color.yellow;
            }
        }
    }

    #endregion

    #region Getters

    public int GetEmberlightCount()
    {
        return currentEmberlightCount;
    }

    public int GetMaxEmberlights()
    {
        return maxEmberlights;
    }

    public bool IsDepleted()
    {
        return isDepleted;
    }

    public bool IsPlayerInRange()
    {
        return isPlayerInRange;
    }

    public float GetRespawnTimer()
    {
        return respawnTimer;
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

        // Draw spawn positions
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            foreach (Vector3 pos in spawnPositions)
            {
                Gizmos.DrawSphere(pos, 0.2f);
            }
        }
    }

    #endregion
}

// EmberlightPickup.cs - Attached to each emberlight prefab
public class EmberlightPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float floatAmplitude = 0.2f;
    [SerializeField] private float floatSpeed = 1.5f;
    [SerializeField] private int emberlightValue = 5; // Fuel amount when picked up

    [Header("Visuals")]
    [SerializeField] private Light emberlightGlow;
    [SerializeField] private ParticleSystem emberlightParticles;

    // Reference to station
    private EmberlightStation station;
    private Vector3 startPosition;
    private float floatTimer = 0f;
    private bool isPickedUp = false;

    void Start()
    {
        startPosition = transform.position;

        // Randomize floating offset
        floatTimer = Random.Range(0f, 2f * Mathf.PI);
    }

    void Update()
    {
        if (isPickedUp) return;

        // Rotate
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Float
        floatTimer += Time.deltaTime * floatSpeed;
        float floatOffset = Mathf.Sin(floatTimer) * floatAmplitude;
        transform.position = startPosition + Vector3.up * floatOffset;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isPickedUp) return;

        if (other.CompareTag("Player"))
        {
            Pickup();
        }
    }

    void Pickup()
    {
        isPickedUp = true;

        // Add fuel to lantern
        LanternManager.Instance?.Refuel(emberlightValue);

        // Play pickup effects
        if (emberlightParticles != null)
        {
            emberlightParticles.transform.parent = null;
            emberlightParticles.Play();
            Destroy(emberlightParticles.gameObject, 1f);
        }

        // Notify station
        if (station != null)
        {
            station.OnEmberlightPickedUp(gameObject);
        }

        // Show feedback
        UIManager.Instance?.ShowMessage($"✦ +{emberlightValue} Emberlight");

        // Destroy this emberlight
        Destroy(gameObject, 0.1f);
    }

    public void SetStation(EmberlightStation stationRef)
    {
        station = stationRef;
    }
}
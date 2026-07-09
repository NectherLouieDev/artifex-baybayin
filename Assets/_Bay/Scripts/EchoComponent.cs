using UnityEngine;
using UnityEngine.InputSystem;

public class EchoComponent : MonoBehaviour
{
    [Header("Echo Configuration")]
    [SerializeField] private GameObject echoTrailPrefab;
    [SerializeField] private Transform echoSpawnPoint;
    [SerializeField] private float echoLifetime = 3f;
    [SerializeField] private float echoFollowSpeed = 4f;

    [Header("Visual Feedback")]
    [SerializeField] private ParticleSystem spawnEffect;
    [SerializeField] private AudioSource echoAudio;
    [SerializeField] private AudioClip echoSpawnSound;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    // Components
    private GGTimer spawnCooldownTimer;
    private EchoTrail currentEcho;

    // State
    private bool canSpawn = true;
    private float spawnCooldown = 0.5f; // Cooldown between spawns

    void Start()
    {
        // Setup spawn cooldown timer
        spawnCooldownTimer = gameObject.AddComponent<GGTimer>();
        spawnCooldownTimer.timerId = "EchoSpawnCooldown";
        spawnCooldownTimer.OnTimerCompleted += SpawnCooldownTimer_OnTimerCompleted;

        // Find spawn point if not set
        if (echoSpawnPoint == null)
        {
            echoSpawnPoint = transform;
        }

        // Find audio source if not set
        if (echoAudio == null)
        {
            echoAudio = GetComponent<AudioSource>();
        }

        // Validate prefab
        if (echoTrailPrefab == null)
        {
            Debug.LogError("EchoTrail prefab not assigned to EchoComponent!");
        }
    }

    void Update()
    {
        // Check if current echo still exists and is active
        if (currentEcho != null && !currentEcho.IsActive())
        {
            currentEcho = null;
        }
    }

    #region Sprint Input

    public void OnSprint(InputValue value)
    {
        if (!this.enabled)
            return;

        // Check if we can spawn
        if (!canSpawn)
            return;

        if (echoTrailPrefab == null)
            return;

        // Spawn the echo
        SpawnEcho();

        // Start cooldown
        canSpawn = false;
        spawnCooldownTimer.StartTimer(spawnCooldown, 1);
    }

    #endregion

    #region Timer Event Handlers

    private void SpawnCooldownTimer_OnTimerCompleted(object sender, GGTimer timer)
    {
        canSpawn = true;
    }

    #endregion

    #region Echo Spawning

    public void SpawnEcho()
    {
        // Get spawn position
        Vector3 spawnPos = echoSpawnPoint.position;
        Quaternion spawnRot = echoSpawnPoint.rotation;

        // Instantiate echo
        GameObject echoObj = Instantiate(echoTrailPrefab, spawnPos, spawnRot);
        EchoTrail echo = echoObj.GetComponent<EchoTrail>();

        if (echo == null)
        {
            Debug.LogError("EchoTrail component not found on prefab!");
            Destroy(echoObj);
            return;
        }

        // Configure echo
        EchoTarget _target = FindFirstObjectByType<EchoTarget>();
        echo.SetTarget(_target.transform);
        echo.SetLifetime(echoLifetime);
        echo.SetFollowSpeed(echoFollowSpeed);

        // Subscribe to despawn event
        echo.OnEchoDespawned += OnEchoDespawned;

        // Activate echo
        echo.Activate();

        // Store reference
        currentEcho = echo;

        // Play spawn effects
        if (spawnEffect != null)
        {
            spawnEffect.transform.position = spawnPos - Vector3.one;
            spawnEffect.Play();
        }

        if (echoSpawnSound != null && echoAudio != null)
        {
            echoAudio.PlayOneShot(echoSpawnSound, 0.5f);
        }

        if (showDebugLogs)
            Debug.Log($"Echo spawned at {spawnPos}");
    }

    private void OnEchoDespawned(EchoTrail echo)
    {
        // Clean up reference when echo despawns
        if (currentEcho == echo)
        {
            currentEcho = null;
        }

        // Unsubscribe from event
        echo.OnEchoDespawned -= OnEchoDespawned;
    }

    #endregion

    #region Public Methods

    public void ForceSpawnEcho()
    {
        if (echoTrailPrefab == null)
            return;

        SpawnEcho();
    }

    public void ClearCurrentEcho()
    {
        if (currentEcho != null)
        {
            currentEcho.Deactivate();
            currentEcho = null;
        }
    }

    public EchoTrail GetCurrentEcho()
    {
        return currentEcho;
    }

    public void SetEchoLifetime(float lifetime)
    {
        echoLifetime = lifetime;
    }

    public void SetEchoSpeed(float speed)
    {
        echoFollowSpeed = speed;
    }

    public void SetSpawnCooldown(float cooldown)
    {
        spawnCooldown = cooldown;
    }

    #endregion

    #region Cleanup

    void OnDisable()
    {
        // Clean up when disabled
        if (spawnCooldownTimer.IsRunning())
        {
            spawnCooldownTimer.StopTimer();
        }

        if (currentEcho != null)
        {
            currentEcho.Deactivate();
            currentEcho = null;
        }
    }

    #endregion
}
using UnityEngine;
using UnityEngine.AI;

public class EchoTrail : MonoBehaviour
{
    [Header("Echo Configuration")]
    [SerializeField] private Transform target;
    [SerializeField] private float followSpeed = 3.5f;
    [SerializeField] private float stoppingDistance = 0.5f;
    [SerializeField] private float updatePathInterval = 0.5f;

    [Header("Timing")]
    [SerializeField] private float lifetime = 5f; // How long the echo exists
    [SerializeField] private bool autoStart = true;

    [Header("Visual Feedback")]
    [SerializeField] private ParticleSystem trailParticles;
    [SerializeField] private Renderer echoRenderer;
    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material fadingMaterial;
    [SerializeField] private float fadeDuration = 1.5f;

    [Header("Audio")]
    [SerializeField] private AudioSource echoAudio;
    [SerializeField] private AudioClip spawnSound;
    [SerializeField] private AudioClip despawnSound;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    // Components
    private NavMeshAgent agent;
    private GGTimer lifetimeTimer;
    private GGTimer pathUpdateTimer;

    // State
    private bool isActive = false;
    private bool isFading = false;
    private float fadeTimer = 0f;

    // Events
    public System.Action<EchoTrail> OnEchoSpawned;
    public System.Action<EchoTrail> OnEchoDespawned;

    private void Awake()
    {
        // Get or add NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
        }

        // Configure agent
        agent.speed = followSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.autoBraking = true;
        agent.updateRotation = true;
        agent.updatePosition = true;

        // Setup timers
        lifetimeTimer = gameObject.AddComponent<GGTimer>();
        lifetimeTimer.timerId = $"{gameObject.name}_LifetimeTimer";
        lifetimeTimer.OnTimerCompleted += LifetimeTimer_OnTimerCompleted;

        pathUpdateTimer = gameObject.AddComponent<GGTimer>();
        pathUpdateTimer.timerId = $"{gameObject.name}_PathUpdateTimer";
        pathUpdateTimer.OnTimerLoop += PathUpdateTimer_OnTimerLoop;

        // Set initial position
        if (target != null)
        {
            transform.position = target.position;
        }

        // Auto start if configured
        if (autoStart)
        {
            Activate();
        }
    }

    void Update()
    {
        if (!isActive || isFading || target == null) return;

        // Update agent destination
        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(target.position);
        }

        // Handle fading visual
        if (isFading)
        {
            fadeTimer += Time.deltaTime;
            float fadeProgress = Mathf.Clamp01(fadeTimer / fadeDuration);

            // Update material
            if (echoRenderer != null && fadingMaterial != null)
            {
                Color color = fadingMaterial.color;
                color.a = Mathf.Lerp(1f, 0f, fadeProgress);
                fadingMaterial.color = color;
                echoRenderer.material = fadingMaterial;
            }

            // Update particles
            if (trailParticles != null)
            {
                var emission = trailParticles.emission;
                emission.rateOverTime = Mathf.Lerp(10f, 0f, fadeProgress);
            }
        }
    }

    #region Activation / Deactivation

    public void Activate()
    {
        if (isActive) return;

        isActive = true;
        isFading = false;
        fadeTimer = 0f;

        // Enable agent
        if (agent != null)
        {
            agent.enabled = true;
            if (target != null && agent.isOnNavMesh)
            {
                agent.SetDestination(target.position);
            }
        }

        // Enable renderer
        if (echoRenderer != null)
        {
            echoRenderer.enabled = true;
            if (activeMaterial != null)
            {
                echoRenderer.material = activeMaterial;
            }
        }

        // Start trail particles
        if (trailParticles != null)
        {
            trailParticles.Play();
        }

        // Play spawn sound
        if (spawnSound != null && echoAudio != null)
        {
            echoAudio.PlayOneShot(spawnSound);
        }

        // Start lifetime timer
        if (!lifetimeTimer.IsRunning())
        {
            lifetimeTimer.StartTimer(lifetime, 1);
        }
        else
        {
            lifetimeTimer.ResetTimer();
        }

        // Start path update timer
        if (!pathUpdateTimer.IsRunning())
        {
            pathUpdateTimer.StartTimer(updatePathInterval, 0); // Infinite loop
        }

        if (showDebugLogs)
            Debug.Log($"EchoTrail {gameObject.name} activated for {lifetime} seconds");

        OnEchoSpawned?.Invoke(this);
    }

    public void Deactivate()
    {
        if (!isActive) return;

        isActive = false;

        // Stop timers
        if (lifetimeTimer.IsRunning())
            lifetimeTimer.StopTimer();
        if (pathUpdateTimer.IsRunning())
            pathUpdateTimer.StopTimer();

        // Disable agent
        if (agent != null)
        {
            agent.enabled = false;
        }

        // Stop particles
        if (trailParticles != null)
        {
            trailParticles.Stop();
        }

        // Play despawn sound
        if (despawnSound != null && echoAudio != null)
        {
            echoAudio.PlayOneShot(despawnSound);
        }

        if (showDebugLogs)
            Debug.Log($"EchoTrail {gameObject.name} deactivated");

        OnEchoDespawned?.Invoke(this);

        // Destroy after a small delay to allow sounds/particles to play
        Destroy(gameObject, 0.5f);
    }

    public void DeactivateWithFade()
    {
        if (!isActive || isFading) return;

        isFading = true;
        fadeTimer = 0f;

        // Stop path updates
        if (pathUpdateTimer.IsRunning())
            pathUpdateTimer.StopTimer();

        // Disable agent
        if (agent != null)
        {
            agent.enabled = false;
        }

        // Start fade timer for cleanup
        if (!lifetimeTimer.IsRunning())
        {
            lifetimeTimer.StartTimer(fadeDuration, 1);
        }
        else
        {
            lifetimeTimer.ResetTimer();
        }

        if (showDebugLogs)
            Debug.Log($"EchoTrail {gameObject.name} fading out");
    }

    #endregion

    #region Timer Event Handlers

    private void LifetimeTimer_OnTimerCompleted(object sender, GGTimer timer)
    {
        if (isFading)
        {
            // Complete fade and deactivate
            Deactivate();
        }
        else
        {
            // Normal deactivation with fade
            DeactivateWithFade();
        }
    }

    private void PathUpdateTimer_OnTimerLoop(object sender, GGTimer timer)
    {
        if (isActive && !isFading && target != null && agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(target.position);
        }
    }

    #endregion

    #region Public Methods

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (isActive && agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(target.position);
        }
    }

    public void SetLifetime(float newLifetime)
    {
        lifetime = newLifetime;
        if (lifetimeTimer.IsRunning())
        {
            lifetimeTimer.ResetTimer();
        }
    }

    public void SetFollowSpeed(float newSpeed)
    {
        followSpeed = newSpeed;
        if (agent != null)
        {
            agent.speed = followSpeed;
        }
    }

    public bool IsActive()
    {
        return isActive;
    }

    public Transform GetTarget()
    {
        return target;
    }

    public float GetRemainingLifetime()
    {
        if (lifetimeTimer.IsRunning())
        {
            return lifetimeTimer.GetCurrentTime();
        }
        return 0f;
    }

    #endregion

    #region Editor Debug

    void OnDrawGizmosSelected()
    {
        if (agent != null && agent.hasPath)
        {
            // Draw path
            Gizmos.color = Color.cyan;
            Vector3[] pathCorners = agent.path.corners;
            for (int i = 0; i < pathCorners.Length - 1; i++)
            {
                Gizmos.DrawLine(pathCorners[i], pathCorners[i + 1]);
            }
        }

        // Draw stopping distance
        if (target != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(target.position, stoppingDistance);
        }
    }

    #endregion
}
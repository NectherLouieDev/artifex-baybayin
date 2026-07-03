using UnityEngine;
using System.Collections;
using System.Collections.Generic;


#region Data Classes

[System.Serializable]
public class FogArea
{
    public Vector3 position;
    public float radius;
    public float densityMultiplier = 2f; // Higher = denser fog
}

[System.Serializable]
public class FogPushData
{
    public Vector3 position;
    public float radius;
    public float duration;
    public float remainingDuration;
    public float startTime;
    public float startDensity;
    public float targetDensity;
    public float currentDensity;
}

#endregion

public class FogManager : MonoBehaviour
{
    [Header("Fog Configuration")]
    [SerializeField] private float fogDensity = 0.02f;
    [SerializeField] private float fogStartDistance = 5f;
    [SerializeField] private float fogEndDistance = 30f;
    [SerializeField] private Color fogColor = new Color(0.3f, 0.3f, 0.35f, 1f);
    [SerializeField] private float defaultFogDensity = 0.02f;

    [Header("Fog Areas")]
    [SerializeField] private List<FogArea> fogAreas = new List<FogArea>();
    [SerializeField] private float globalFogMultiplier = 1f;
    [SerializeField] private float targetFogMultiplier = 1f;
    [SerializeField] private float fogTransitionSpeed = 0.5f;

    [Header("Memory Stone Fog Push")]
    [SerializeField] private float pushBackRadius = 20f;
    [SerializeField] private float pushBackDuration = 10f;
    [SerializeField] private float pushBackFogDensity = 0.005f;
    [SerializeField] private AnimationCurve pushBackCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem fogParticles;
    [SerializeField] private Light fogLight;
    [SerializeField] private Material fogMaterial;
    [SerializeField] private Transform fogPlane;

    [Header("Audio")]
    [SerializeField] private AudioSource fogAudio;
    [SerializeField] private AudioClip fogAmbient;
    [SerializeField] private AudioClip fogPushSound;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool showDebugGizmos = true;

    // Runtime data
    private Dictionary<Vector3, FogPushData> activePushBacks = new Dictionary<Vector3, FogPushData>();
    private List<Vector3> pushBackPositions = new List<Vector3>();
    private bool isFogActive = true;
    private float currentFogDensity;
    private float currentFogStart;
    private float currentFogEnd;

    // Events
    public System.Action<float> OnFogDensityChanged;
    public System.Action<Vector3, float> OnFogPushedBack;
    public System.Action<Vector3> OnFogReturned;

    // Singleton
    public static FogManager Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Initialize fog
        currentFogDensity = fogDensity;
        currentFogStart = fogStartDistance;
        currentFogEnd = fogEndDistance;

        // Apply fog settings
        UpdateFogSettings();

        // Start ambient audio
        if (fogAudio != null && fogAmbient != null)
        {
            fogAudio.clip = fogAmbient;
            fogAudio.loop = true;
            fogAudio.Play();
        }

        // Start fog particles
        if (fogParticles != null)
        {
            fogParticles.Play();
        }
    }

    void Update()
    {
        // Smoothly transition fog multiplier
        if (Mathf.Abs(globalFogMultiplier - targetFogMultiplier) > 0.01f)
        {
            globalFogMultiplier = Mathf.Lerp(globalFogMultiplier, targetFogMultiplier, Time.deltaTime * fogTransitionSpeed);
            UpdateFogSettings();
        }

        // Update push back areas
        UpdatePushBacks();
    }

    #region Core Fog Management

    void UpdateFogSettings()
    {
        // Calculate effective fog density with multiplier
        float effectiveDensity = fogDensity * globalFogMultiplier;

        // Apply push back effects
        float minDensity = float.MaxValue;
        foreach (var push in activePushBacks.Values)
        {
            if (push.currentDensity < minDensity)
                minDensity = push.currentDensity;
        }

        if (minDensity < float.MaxValue && activePushBacks.Count > 0)
        {
            // Use the lowest density (strongest push back)
            effectiveDensity = Mathf.Min(effectiveDensity, minDensity);
        }

        // Clamp
        effectiveDensity = Mathf.Clamp(effectiveDensity, 0.001f, 0.1f);

        // Apply to render settings
        RenderSettings.fog = isFogActive;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogStartDistance = fogStartDistance;
        RenderSettings.fogEndDistance = fogEndDistance;

        // Update fog density
        currentFogDensity = effectiveDensity;

        // Update fog material if using custom shader
        if (fogMaterial != null)
        {
            fogMaterial.SetFloat("_FogDensity", effectiveDensity);
            fogMaterial.SetColor("_FogColor", fogColor);
        }

        OnFogDensityChanged?.Invoke(effectiveDensity);
    }

    public void SetFogActive(bool active)
    {
        isFogActive = active;
        RenderSettings.fog = active;

        if (fogParticles != null)
        {
            if (active)
                fogParticles.Play();
            else
                fogParticles.Stop();
        }
    }

    public void SetFogDensity(float density)
    {
        fogDensity = Mathf.Clamp(density, 0.001f, 0.1f);
        UpdateFogSettings();
    }

    public void SetFogColor(Color color)
    {
        fogColor = color;
        UpdateFogSettings();
    }

    #endregion

    #region Push Back System

    public void PushBackFog(Vector3 position, float radius)
    {
        PushBackFog(position, radius, pushBackDuration);
    }

    public void PushBackFog(Vector3 position, float radius, float duration)
    {
        if (activePushBacks.ContainsKey(position))
        {
            // Update existing push
            activePushBacks[position].remainingDuration = duration;
            activePushBacks[position].radius = radius;
            activePushBacks[position].startTime = Time.time;
        }
        else
        {
            // Create new push
            FogPushData pushData = new FogPushData
            {
                position = position,
                radius = radius,
                duration = duration,
                remainingDuration = duration,
                startTime = Time.time,
                startDensity = fogDensity,
                targetDensity = pushBackFogDensity,
                currentDensity = fogDensity
            };

            activePushBacks.Add(position, pushData);
            pushBackPositions.Add(position);

            if (showDebugLogs)
                Debug.Log($"Fog pushed back at {position} with radius {radius}");

            // Play push sound
            if (fogPushSound != null && fogAudio != null)
            {
                fogAudio.PlayOneShot(fogPushSound);
            }

            // Visual feedback - spawn particles at push location
            if (fogParticles != null)
            {
                ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
                emitParams.position = position;
                emitParams.velocity = Vector3.up * 2f;
                fogParticles.Emit(emitParams, 20);
            }

            OnFogPushedBack?.Invoke(position, radius);
        }

        UpdateFogSettings();
    }

    void UpdatePushBacks()
    {
        if (activePushBacks.Count == 0) return;

        List<Vector3> expiredKeys = new List<Vector3>();

        foreach (var kvp in activePushBacks)
        {
            FogPushData push = kvp.Value;
            push.remainingDuration -= Time.deltaTime;

            // Calculate progress
            float progress = 1f - (push.remainingDuration / push.duration);
            float curveValue = pushBackCurve.Evaluate(progress);

            // Update current density based on progress
            push.currentDensity = Mathf.Lerp(push.startDensity, push.targetDensity, curveValue);

            // If expired, mark for removal
            if (push.remainingDuration <= 0)
            {
                expiredKeys.Add(kvp.Key);
            }
        }

        // Remove expired pushes
        foreach (var key in expiredKeys)
        {
            activePushBacks.Remove(key);
            pushBackPositions.Remove(key);
            OnFogReturned?.Invoke(key);

            if (showDebugLogs)
                Debug.Log($"Fog returned at {key}");
        }

        if (expiredKeys.Count > 0)
        {
            UpdateFogSettings();
        }
    }

    public bool IsPositionInPushBack(Vector3 position)
    {
        foreach (var push in activePushBacks.Values)
        {
            float distance = Vector3.Distance(position, push.position);
            if (distance < push.radius)
                return true;
        }
        return false;
    }

    public float GetFogDensityAtPosition(Vector3 position)
    {
        float density = currentFogDensity;

        foreach (var push in activePushBacks.Values)
        {
            float distance = Vector3.Distance(position, push.position);
            if (distance < push.radius)
            {
                float t = 1f - (distance / push.radius);
                float pushDensity = Mathf.Lerp(push.currentDensity, push.targetDensity, t);
                density = Mathf.Min(density, pushDensity);
            }
        }

        return density;
    }

    #endregion

    #region Fog Area System

    public void AddFogArea(Vector3 position, float radius, float densityMultiplier)
    {
        FogArea area = new FogArea
        {
            position = position,
            radius = radius,
            densityMultiplier = densityMultiplier
        };
        fogAreas.Add(area);
    }

    public void RemoveFogArea(Vector3 position)
    {
        for (int i = fogAreas.Count - 1; i >= 0; i--)
        {
            if (Vector3.Distance(fogAreas[i].position, position) < 0.1f)
            {
                fogAreas.RemoveAt(i);
                break;
            }
        }
    }

    float GetAreaMultiplier(Vector3 position)
    {
        float multiplier = 1f;

        foreach (var area in fogAreas)
        {
            float distance = Vector3.Distance(position, area.position);
            if (distance < area.radius)
            {
                float t = 1f - (distance / area.radius);
                multiplier *= Mathf.Lerp(1f, area.densityMultiplier, t);
            }
        }

        return multiplier;
    }

    #endregion

    #region Player Fog Effects

    public void ApplyFogEffectToPlayer(GameObject player)
    {
        if (player == null) return;

        // Check if player is in push back area
        if (IsPositionInPushBack(player.transform.position))
        {
            // Player is in safe zone - reduce fog effect
            LanternManager.Instance?.RemoveFogEffect();
        }
        else
        {
            // Player is in fog - double decay
            LanternManager.Instance?.ApplyFogEffect();
        }
    }

    #endregion

    #region Visual Effects

    public void UpdateFogPlane(Vector3 playerPosition)
    {
        if (fogPlane != null)
        {
            // Move fog plane with player but keep it below
            fogPlane.position = new Vector3(playerPosition.x, 0f, playerPosition.z);

            // Scale based on fog density
            float scale = Mathf.Lerp(50f, 200f, currentFogDensity / 0.05f);
            fogPlane.localScale = new Vector3(scale, 1f, scale);
        }
    }

    public void SetFogLightIntensity(float intensity)
    {
        if (fogLight != null)
        {
            fogLight.intensity = intensity;
        }
    }

    #endregion

    #region Debug

    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // Draw fog areas
        Gizmos.color = new Color(0.3f, 0.5f, 0.8f, 0.3f);
        foreach (var area in fogAreas)
        {
            Gizmos.DrawWireSphere(area.position, area.radius);
        }

        // Draw push back areas
        foreach (var push in activePushBacks.Values)
        {
            Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.4f);
            Gizmos.DrawWireSphere(push.position, push.radius);

            // Draw inner safe zone
            Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.2f);
            Gizmos.DrawSphere(push.position, push.radius * 0.5f);
        }
    }

    [ContextMenu("Debug - Push Back Fog")]
    void DebugPushBack()
    {
        if (Camera.main != null)
        {
            PushBackFog(Camera.main.transform.position, 20f, 10f);
        }
    }

    [ContextMenu("Debug - Reset Fog")]
    void DebugResetFog()
    {
        activePushBacks.Clear();
        pushBackPositions.Clear();
        targetFogMultiplier = 1f;
        UpdateFogSettings();
    }

    #endregion
}
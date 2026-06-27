using System;
using UnityEngine;

public class RepairComponent : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _fuseTime = 3f;
    [SerializeField] private int _healValue = 100;
    [SerializeField] private float _explosionRadius = 3f;
    [SerializeField] private GameObject _projectileHealerPrefab;
    
    public float FuseTime 
    { 
        get { return _fuseTime; } 
        set { _fuseTime = value; } 
    }

    public float ExplosionRadius 
    { 
        get { return _explosionRadius; } 
        set { _explosionRadius = value; } 
    }

    private GGTimer _fuseTimer;

    private bool _isArmed = false;
    private bool _hasExploded = false;

    public event EventHandler OnArmed;
    public event EventHandler OnDisarmed;
    public event EventHandler OnExploded;
    public event EventHandler<float> OnFuseUpdated;

    private void Awake()
    {
        _fuseTimer = gameObject.AddComponent<GGTimer>();
        _fuseTimer.timerId = $"FuseTimer_{gameObject.name}";
        _fuseTimer.OnTimerCompleted += FuseTimer_OnTimerCompleted;
        _fuseTimer.OnTimerUpdated += FuseTimer_OnTimerUpdated;
    }

    public void Arm()
    {
        if (_hasExploded || _isArmed) 
            return;

        _isArmed = true;

        _fuseTimer.StartTimer(_fuseTime, 1);
        
        OnArmed?.Invoke(this, EventArgs.Empty);
    }

    public void Disarm()
    {
        if (_hasExploded) 
            return;

        _isArmed = false;

        _fuseTimer.StopTimer();

        OnDisarmed?.Invoke(this, EventArgs.Empty);
    }

    public void Explode()
    {
        if (_hasExploded)
            return;

        _hasExploded = true;

        GameObject projectileHealObject = Instantiate(_projectileHealerPrefab, transform.position, transform.rotation);
        ProjectileHealer projectileHealer = projectileHealObject.GetComponent<ProjectileHealer>();
        projectileHealer.DamageRadius = _explosionRadius;
        projectileHealer.DamageValue = _healValue;
        projectileHealer.Splode();

        // Feedbacks ---- 

        // Apply damage
        ApplyExplosionDamage();

        OnExploded?.Invoke(this, EventArgs.Empty);

        Destroy(gameObject);
    }

    private void ApplyExplosionDamage()
    {
        //Collider[] hitColliders = Physics.OverlapSphere(transform.position, _explosionRadius, _damageLayers);

        //foreach (var hitCollider in hitColliders)
        //{
        //    // Skip self if damage to self is 0
        //    //if (_damageToSelf == 0 && hitCollider.gameObject == gameObject)
        //    //    continue;

        //    // Damage walls
        //    //var wall = hitCollider.GetComponent<DestructibleWall>();
        //    //if (wall != null)
        //    //{
        //    //    wall.TakeDamage(_damageToWalls);
        //    //    continue;
        //    //}

        //    // Damage players
        //    //if (hitCollider.TryGetComponent(out PlayerStateHandler playerStateHandler))
        //    //{
        //    //    var health = playerStateHandler.GetComponent<HealthComponent>();
        //    //    if (health != null)
        //    //    {
        //    //        // Check if this is the player who dropped/threw (if we had ownership)
        //    //        // For now, just apply damage
        //    //        health.TakeDamage(_damageToPlayers);
        //    //    }
        //    //    continue;
        //    //}

        //    // Damage creeps/enemies
        //    //if (hitCollider.TryGetComponent(out CreepBase creepBase))
        //    //{
        //    //    var health = creepBase.GetComponent<HealthComponent>();
        //    //    if (health != null)
        //    //    {
        //    //        health.TakeDamage(_damageToCreeps);
        //    //    }
        //    //    continue;
        //    //}

        //    // Damage base core
        //    //if (hitCollider.TryGetComponent(out BaseCore baseCore))
        //    //{
        //    //    var health = baseCore.GetComponent<HealthComponent>();
        //    //    if (health != null)
        //    //    {
        //    //        health.TakeDamage(_damageToBase);
        //    //    }
        //    //    continue;
        //    //}
        //}
    }

    private void FuseTimer_OnTimerUpdated(object sender, GGTimer e)
    {
        if (!_isArmed) 
            return;

        float progress = _fuseTimer.GetCurrentTime() / _fuseTime;

        // Update light color/intensity
        //if (_blastLights != null && _blastLights.Length > 0)
        //{
        //    foreach (GameObject g in _blastLights)
        //    {
        //        Light light = g.GetComponent<Light>();
        //        if (light != null)
        //        {
        //            light.color = Color.Lerp(_startColor, _endColor, 1 - progress); // Inverse so it gets more intense
        //            light.intensity = Mathf.Lerp(1f, 32f, 1 - progress);
        //        }
        //    }
        //}

        OnFuseUpdated?.Invoke(this, progress);
    }

    private void FuseTimer_OnTimerCompleted(object sender, GGTimer e)
    {
        Explode();
    }

    private void OnDestroy()
    {
        if (_fuseTimer != null)
        {
            _fuseTimer.OnTimerCompleted -= FuseTimer_OnTimerCompleted;
            _fuseTimer.OnTimerUpdated -= FuseTimer_OnTimerUpdated;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _explosionRadius);
    }
}
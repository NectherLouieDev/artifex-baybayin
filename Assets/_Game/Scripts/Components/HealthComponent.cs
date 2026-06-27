// HealthComponent.cs
using UnityEngine;
using System.Collections;
using System;

public enum EHealthChangeType
{
    None,
    Damage,
    Heal,
    Respawn
}

public class HealthComponent : MonoBehaviour
{
    public class HealthChangedArgs : EventArgs
    {
        public bool isInitialDispatch = false;
        public EHealthChangeType healthChangeType;
        public int maxHealth = 100;
        public int currentHealth = 100;
        public int damage = 0;
    }
    public event EventHandler<HealthChangedArgs> OnHealthChanged;
    public event EventHandler<HealthComponent> OnDeath;

    [Header("Health Settings")]
    [SerializeField] private int _maxHealth = 100;

    public int CurrentHealth
    {
        get { return _currentHealth; }
        set { _currentHealth = value; }
    }

    public int MaxHealth
    {
        get { return _maxHealth; }
        set { _maxHealth = value; }
    }

    public bool IsDead { get { return _isDead; } }

    private int _currentHealth;
    private bool _isDead = false;

    private void Awake()
    {
        _currentHealth = _maxHealth;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(this, new HealthChangedArgs
        {
            isInitialDispatch = true,
            healthChangeType = EHealthChangeType.None,
            maxHealth = _maxHealth,
            currentHealth = _currentHealth
        });
    }

    public void RespawnHealth()
    {
        _currentHealth = _maxHealth;
        _isDead = false;

        OnHealthChanged?.Invoke(this, new HealthChangedArgs
        {
            isInitialDispatch = false,
            healthChangeType = EHealthChangeType.Respawn,
            maxHealth = _maxHealth,
            currentHealth = _currentHealth
        });
    }

    public void TakeDamage(int damage)
    {
        if (_isDead)
            return;

        _currentHealth -= damage;

        OnHealthChanged?.Invoke(this, new HealthChangedArgs
        {
            isInitialDispatch = false,
            healthChangeType = EHealthChangeType.Damage,
            maxHealth = _maxHealth,
            currentHealth = _currentHealth,
            damage = damage
        });

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int healAmount)
    {
        if (_isDead)
            return;

        _currentHealth = Mathf.Min(_maxHealth, _currentHealth + healAmount);

        OnHealthChanged?.Invoke(this, new HealthChangedArgs
        {
            isInitialDispatch = false,
            healthChangeType = EHealthChangeType.Heal,
            maxHealth = _maxHealth,
            currentHealth = _currentHealth
        });
    }

    public void RespawnHeal(int healAmount)
    {
        if (!_isDead)
            return;

        _isDead = false;

        _currentHealth = Mathf.Min(_maxHealth, _currentHealth + healAmount);

        OnHealthChanged?.Invoke(this, new HealthChangedArgs
        {
            isInitialDispatch = false,
            healthChangeType = EHealthChangeType.Heal,
            maxHealth = _maxHealth,
            currentHealth = _currentHealth
        });
    }

    private void Die()
    {
        _isDead = true;

        OnDeath?.Invoke(this, this);
    }
}
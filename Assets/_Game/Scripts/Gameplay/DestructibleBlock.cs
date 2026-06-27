using MoreMountains.Feedbacks;
using UnityEngine;

public class DestructibleBlock : MonoBehaviour
{
    [SerializeField] private HealthComponent _healthComponent;
    [SerializeField] private GameObject _visualObject;
    [SerializeField] private BoxCollider _collider;

    [Header("Feedbacks")]
    [SerializeField] MMFeedbacks _explodeFeedback;
    [SerializeField] GameObject _visualComplete;
    [SerializeField] GameObject _visualDamaged;
    [SerializeField] GameObject _visualDestroyed;

    [Header("Explosion Debris")]
    [SerializeField] private ExplosionDebrisComponent _explosionDebris;
    [SerializeField] private bool _isBroken = false;
    public bool IsBroken
    {
        get { return _isBroken; }
    }

    private void Start()
    {
        _healthComponent.OnHealthChanged += HealthComponent_OnHealthChanged;
        _healthComponent.OnDeath += HealthComponent_OnDeath;

        _explosionDebris = GetComponent<ExplosionDebrisComponent>();

        _visualComplete.SetActive(true);
        _visualDamaged.SetActive(false);
    }

    private void Update()
    {
        _isBroken = _healthComponent.CurrentHealth <= 0;
    }

    private void HealthComponent_OnHealthChanged(object sender, HealthComponent.HealthChangedArgs e)
    {
        if (!e.isInitialDispatch)
        {
            switch (e.healthChangeType)
            {
                case EHealthChangeType.Damage:
                    _explosionDebris.TriggerExplosion();

                    _visualComplete.SetActive(false);
                    _visualDamaged.SetActive(true);
                    _visualDestroyed.SetActive(false);
                    break;

                case EHealthChangeType.Heal:

                    _collider.center = new Vector3(0, 2, 0);
                    _collider.size = new Vector3(2, 4, 2);

                    if (e.currentHealth > e.maxHealth)
                        break;

                    if (e.currentHealth < e.maxHealth)
                    {
                        _visualComplete.SetActive(false);
                        _visualDamaged.SetActive(true);
                        _visualDestroyed.SetActive(false);
                    }
                    
                    if (e.currentHealth >= e.maxHealth)
                    {
                        _visualComplete.SetActive(true);
                        _visualDamaged.SetActive(false);
                        _visualDestroyed.SetActive(false);
                        
                        GameplayEventBus.Instance.Invoke(new GameplayEvents.BlockRepaired<DestructibleBlock>
                        {
                            Sender = this,
                        });
                    }

                    break;
            }
        }
    }

    private void HealthComponent_OnDeath(object sender, HealthComponent e)
    {
        _explodeFeedback?.PlayFeedbacks();

        //_collider.enabled = false;
        //_visualObject.SetActive(false);

        _collider.center = new Vector3(0, -0.1f, 0);
        _collider.size = new Vector3(2, 0.2f, 2);

        _visualDamaged.SetActive(false);
        _visualDestroyed.SetActive(true);

        _explosionDebris.TriggerExplosion();

        GameplayEventBus.Instance.Invoke(new GameplayEvents.BlockDestroyed<DestructibleBlock>
        {
            Sender = this,
        });
    }
}

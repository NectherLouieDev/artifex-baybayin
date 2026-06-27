using UnityEngine;

public class BaseStateHandler : MonoBehaviour
{
    [SerializeField] private HealthComponent _healthComponent;
    
    private void Start()
    {
        _healthComponent.OnHealthChanged += HealthComponent_OnHealthChanged;
        _healthComponent.OnDeath += HealthComponent_OnDeath;
    }

    private void HealthComponent_OnHealthChanged(object sender, HealthComponent.HealthChangedArgs e)
    {
        //throw new System.NotImplementedException();

        GameplayEventBus.Instance.Invoke(new GameplayEvents.BaseHealthChanged
        {
            Base = this,
            CurrentHealth = _healthComponent.CurrentHealth,
            MaxHealth = _healthComponent.MaxHealth,
        });
    }

    private void HealthComponent_OnDeath(object sender, HealthComponent e)
    {
        // GAME OVER!

        // Feedbacks

        //Destroy(gameObject);

        Debug.Log("Base Dead! GAME OVER");

        GameplayEventBus.Instance.Invoke(new GameplayEvents.BaseDeath
        {
            Base = this,
        });
    }
}

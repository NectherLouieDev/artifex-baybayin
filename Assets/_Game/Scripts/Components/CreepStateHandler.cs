using Microlight.MicroBar;
using UnityEngine;

public class CreepStateHandler : MonoBehaviour
{
    [SerializeField] private ECreepType _creepType;

    [SerializeField] private CreepAgentComponent _agentComponent;
    [SerializeField] private HealthComponent _healthComponent;
    [SerializeField] private MicroBar _healthBar;
    [SerializeField] private LootTableComponent _lootTableComponent;

    private void Start()
    {
        _healthComponent.OnHealthChanged += HealthComponent_OnHealthChanged;
        _healthComponent.OnDeath += HealthComponent_OnDeath;

        _healthBar.Initialize(_healthComponent.MaxHealth);
    }

    private void HealthComponent_OnHealthChanged(object sender, HealthComponent.HealthChangedArgs e)
    {
        //throw new System.NotImplementedException();

        _healthBar.UpdateBar(_healthComponent.CurrentHealth);
    }

    private void HealthComponent_OnDeath(object sender, HealthComponent e)
    {
        // Feedbacks

        //Destroy(gameObject);
    }

    public void KillCreep()
    {
        _lootTableComponent.SpawnLoot(_creepType);

        Destroy(gameObject);
    }
}

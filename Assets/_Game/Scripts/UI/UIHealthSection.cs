using Microlight.MicroBar;
using TMPro;
using UnityEngine;

public class UIHealthSection : MonoBehaviour
{
    [SerializeField] private MicroBar _bar;
    [SerializeField] private TMP_Text _healthText;

    private void Start()
    {
        GameplayEventBus.Instance.Subscribe<GameplayEvents.BaseHealthChanged>(OnBaseHealthChanged);
    }

    private void OnBaseHealthChanged(GameplayEvents.BaseHealthChanged evt)
    {
        int currentHealth = evt.CurrentHealth;
        int maxHealth = evt.MaxHealth;

        _bar.Initialize(maxHealth);

        _healthText.text = currentHealth.ToString() + "/" + maxHealth.ToString();

        _bar.UpdateBar(currentHealth);
    }
}

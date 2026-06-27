using DG.Tweening;
using UnityEngine;

public class ProjectileHealer : MonoBehaviour
{
    [SerializeField] private bool _editorOnly = false;

    private float _damageRadius = 3f;
    public float DamageRadius 
    { 
        get { return _damageRadius; } 
        set { _damageRadius = value; }
    }

    private int _damageValue = 10;
    public int DamageValue
    {
        get { return _damageValue; }
        set { _damageValue = value; }
    }

    public void Splode()
    {
        if (_editorOnly)
            return;

        transform.localScale = Vector3.zero;
        transform.DOScale(_damageRadius, 0.2f)
            .OnComplete(() =>
            {
                Destroy(gameObject);
            });
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out HealthComponent healthComponent))
        {
            Debug.Log("Projectile Healer -> collision.transform.name " + other.transform.name);
            if (healthComponent.IsDead)
                healthComponent.RespawnHeal(_damageValue);
            else
                healthComponent.Heal(_damageValue);
        }
    }
}

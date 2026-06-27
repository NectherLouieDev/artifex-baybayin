using DG.Tweening;
using MoreMountains.Feedbacks;
using UnityEngine;

public class ProjectileDamager : MonoBehaviour
{
    [Header("Feedbacks")]
    [SerializeField] private MMFeedbacks _splodeFeedback;

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
        // We feedback
        _splodeFeedback?.PlayFeedbacks();

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
            Debug.Log("collision.transform.name " + other.transform.name);
            healthComponent.TakeDamage(_damageValue);
        }
    }
}

using MoreMountains.Feedbacks;
using UnityEngine;

public class Emberlight : MonoBehaviour
{
    [SerializeField] private GameObject _visual;
    [SerializeField] private Collider _collider;
    [SerializeField] private MMFeedbacks _collectFeedback;

    private GGTimer _delayDestroyTimer;

    private void Start()
    {
        _delayDestroyTimer = gameObject.AddComponent<GGTimer>();
        _delayDestroyTimer.OnTimerCompleted += DelayDestroyTimer_OnTimerCompleted;
    }

    private void DelayDestroyTimer_OnTimerCompleted(object sender, GGTimer e)
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.TryGetComponent(out PlayerIdentity identity))
        {
            LanternManager.Instance.Refuel(5f);

            _collectFeedback?.PlayFeedbacks();

            _collider.enabled = false;
            _visual.SetActive(false);

            _delayDestroyTimer.StartTimer(0.5f, 1);
        }
    }
}

using MoreMountains.Feedbacks;
using UnityEngine;

public class Emberlight : MonoBehaviour
{
    [Header("Emberlight Settings")]
    [SerializeField] private GameObject _visual;
    [SerializeField] private Collider _collider;
    [SerializeField] private MMFeedbacks _collectFeedback;
    [SerializeField] private int emberlightValue = 1;

    // References
    private EmberlightStation station;
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
            Pickup();
        }
    }

    private void Pickup()
    {
        // Add fuel to lantern
        //LanternManager.Instance?.Refuel(fuelValue);
        InventoryManager.Instance.AddEmberlight(emberlightValue);

        // Play feedback
        _collectFeedback?.PlayFeedbacks();

        // Notify station
        if (station != null)
        {
            station.OnEmberlightPickedUp(gameObject);
        }

        // Show feedback
        UIManager.Instance?.ShowMessage($"+{emberlightValue} Emberlight!");

        // Disable visuals and collider
        _collider.enabled = false;
        _visual.SetActive(false);

        // Destroy after delay
        _delayDestroyTimer.StartTimer(0.5f, 1);
    }

    public void SetStation(EmberlightStation stationRef)
    {
        station = stationRef;
    }
}
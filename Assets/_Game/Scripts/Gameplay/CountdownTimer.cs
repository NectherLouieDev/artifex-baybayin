using UnityEngine;

public class CountdownTimer : MonoBehaviour
{
    [SerializeField] private float _countdownTime = 60.0f;
    [SerializeField] private GGTimer _timer;

    private void Awake()
    {
        _timer = gameObject.AddComponent<GGTimer>();
        _timer.OnTimerUpdated += OnTimerUpdated;
        _timer.OnTimerCompleted += OnTimerCompleted;
    }

    private void OnTimerCompleted(object sender, GGTimer e)
    {
        GameplayEventBus.Instance.Invoke(new GameplayEvents.CountdownTimerCompleted
        {
            CurrentTime = _timer.GetCurrentTime(),
        });
    }

    private void OnTimerUpdated(object sender, GGTimer e)
    {
        GameplayEventBus.Instance.Invoke(new GameplayEvents.CountdownTimerUpdated
        { 
            CurrentTime = _timer.GetCurrentTime() 
        });
    }

    public void StartTimer()
    {
        _timer.StartTimer(_countdownTime, 1, true);
    }

    public void StopTimer()
    {
        _timer.OnTimerUpdated -= OnTimerUpdated;
        _timer.OnTimerCompleted -= OnTimerCompleted;
        
        _timer.StopTimer();
    }
}

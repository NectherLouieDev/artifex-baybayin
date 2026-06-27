using System;
using UnityEngine;

public class GGTimer : MonoBehaviour
{
    public event EventHandler<GGTimer> OnTimerStarted;
    public event EventHandler<GGTimer> OnTimerUpdated;
    public event EventHandler<GGTimer> OnTimerCompleted;
    public event EventHandler<GGTimer> OnTimerLoop;

    public string timerId = string.Empty;

    private bool _countdown = false;
    private float _currentTime = 0f;
    private float _targetTime = 0f;
    private int _currentLoops = 0;
    private int _targetLoops = 0;
    private bool _running = false;

    public void StartTimer(float targetTime, int targetLoops, bool countdown = false)
    {
        _running = true;
        _countdown = countdown;
        _currentLoops = 0;
        _targetLoops = targetLoops;

        if (_countdown)
        {
            _currentTime = targetTime;
            _targetTime = 0f;
        }
        else
        {
            _currentTime = 0f;
            _targetTime = targetTime;
        }

        OnTimerStarted?.Invoke(this, this);
    }

    public void ResetTimer()
    {
        _running = true;

        if (_countdown)
        {
            _currentTime = _targetTime;
        }
        else
        {
            _currentTime = 0f;
        }
    }

    public void StopTimer()
    {
        _countdown = false;
        _currentLoops = 0;
        _targetLoops = 0;
        _currentTime = 0f;
        _targetTime = 0f;
        _running = false;
    }

    private void Update()
    {
        if (_running)
        {
            if (!_countdown)
            {
                _currentTime += Time.deltaTime;

                OnTimerUpdated?.Invoke(this, this);

                if (_currentTime >= _targetTime)
                {
                    CompleteTimer();
                }
            }
            else
            {
                _currentTime -= Time.deltaTime;

                OnTimerUpdated?.Invoke(this, this);

                if (_currentTime <= _targetTime)
                {
                    CompleteTimer();
                }
            }
        }
    }

    private void CompleteTimer()
    {
        _running = false;

        if (_targetLoops > 0)
        {
            _currentLoops++;
        }

        // Infinite Loop
        if (_targetLoops == 0)
        {
            OnTimerLoop?.Invoke(this, this);
            ResetTimer();
        }
        // Finite Loops
        else if (_currentLoops < _targetLoops)
        {
            OnTimerLoop?.Invoke(this, this);
            ResetTimer();
        }
        else
        {
            StopTimer();
            OnTimerCompleted?.Invoke(this, this);
        }
    }

    public void SetCurrentTime(float value)
    {
        if (_running)
        {
            _currentTime = value;
        }
    }

    public float GetCurrentTime()
    {
        return _currentTime;
    }

    public float TargetTime { get { return _targetTime; } }

    public bool IsRunning()
    {
        return _running;
    }
}

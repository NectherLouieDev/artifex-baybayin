using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventSequencer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool _autoStart = false;
    [SerializeField] private bool _loopSequence = false;
    [SerializeField] private bool _destroyOnComplete = false;

    [Header("Debug")]
    [SerializeField] private bool _debugLogSteps = true;
    [SerializeField] private int _currentStepIndex = -1;

    private List<SequenceStep> _steps = new List<SequenceStep>();
    private bool _isRunning = false;
    private bool _isPaused = false;

    public event EventHandler OnSequenceStarted;
    public event EventHandler<int> OnStepStarted;
    public event EventHandler<int> OnStepCompleted;
    public event EventHandler OnSequenceCompleted;

    private void Start()
    {
        if (_autoStart)
        {
            StartSequence();
        }
    }

    public void StartSequence()
    {
        if (_isRunning) return;

        _currentStepIndex = -1;
        _isRunning = true;
        _isPaused = false;

        OnSequenceStarted?.Invoke(this, EventArgs.Empty);

        if (_debugLogSteps) Debug.Log($"[EventSequencer] Sequence started on {gameObject.name}");

        ExecuteNextStep();
    }

    public void StopSequence()
    {
        _isRunning = false;
        _isPaused = false;

        if (_debugLogSteps) Debug.Log($"[EventSequencer] Sequence stopped on {gameObject.name}");
    }

    public void PauseSequence()
    {
        _isPaused = true;
        if (_debugLogSteps) Debug.Log($"[EventSequencer] Sequence paused at step {_currentStepIndex}");
    }

    public void ResumeSequence()
    {
        if (!_isPaused) return;
        _isPaused = false;
        if (_debugLogSteps) Debug.Log($"[EventSequencer] Sequence resumed");
        ExecuteNextStep();
    }

    public void AddStep(SequenceStep step)
    {
        _steps.Add(step);
        step.Sequencer = this;
    }

    public void ClearSteps()
    {
        _steps.Clear();
        _currentStepIndex = -1;
    }

    public void InsertStep(int index, SequenceStep step)
    {
        _steps.Insert(index, step);
        step.Sequencer = this;
    }

    public void RemoveStep(int index)
    {
        if (index >= 0 && index < _steps.Count)
        {
            _steps.RemoveAt(index);
        }
    }

    public int GetStepCount()
    {
        return _steps.Count;
    }

    public bool IsRunning()
    {
        return _isRunning;
    }

    public bool IsPaused()
    {
        return _isPaused;
    }

    private void ExecuteNextStep()
    {
        if (!_isRunning || _isPaused) return;

        _currentStepIndex++;

        if (_currentStepIndex >= _steps.Count)
        {
            CompleteSequence();
            return;
        }

        OnStepStarted?.Invoke(this, _currentStepIndex);

        SequenceStep currentStep = _steps[_currentStepIndex];

        if (_debugLogSteps) Debug.Log($"[EventSequencer] Executing step {_currentStepIndex}/{_steps.Count - 1} on {gameObject.name}");

        currentStep.Execute();
    }

    public void OnStepComplete(SequenceStep step)
    {
        int stepIndex = _steps.IndexOf(step);
        OnStepCompleted?.Invoke(this, stepIndex);

        if (_debugLogSteps) Debug.Log($"[EventSequencer] Step {stepIndex} completed");

        ExecuteNextStep();
    }

    private void CompleteSequence()
    {
        _isRunning = false;
        OnSequenceCompleted?.Invoke(this, EventArgs.Empty);

        if (_debugLogSteps) Debug.Log($"[EventSequencer] Sequence completed on {gameObject.name}");

        if (_loopSequence)
        {
            StartSequence();
        }
        else if (_destroyOnComplete)
        {
            Destroy(gameObject);
        }
    }
}
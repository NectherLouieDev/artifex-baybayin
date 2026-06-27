using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Level10_CountdownSequenceStepper : SequenceStepper
{
    public override event EventHandler OnSequenceCompleted;

    [SerializeField] private EventSequencer _sequencer;
    [SerializeField] private List<SequenceStep> _steps = new List<SequenceStep>();

    [Header("Level Config")]
    [SerializeField] private CountdownTimer _countdownTimer;
    [SerializeField] private SpawnTutorialFlagStep _spawnTutorialFlagStep;
    [SerializeField] private DialogComponent _dialogComponent;
    [Multiline]
    [SerializeField] private string[] _successMessages;
    [Multiline]
    [SerializeField] private string[] _failMessages;

    private bool _hasStartedSequence = false;

    private void Start()
    {
        GameplayEventBus.Instance.Subscribe<GameplayEvents.CountdownTimerCompleted>(OnCountdownTimerCompleted);
        GameplayEventBus.Instance.Subscribe<GameplayEvents.QuestDataUpdated>(OnQuestDataUpdated);
        
        // Add steps to sequencer
        _steps.ForEach(step =>
        {
            _sequencer.AddStep(step);
        });
    }

    private void OnCountdownTimerCompleted(GameplayEvents.CountdownTimerCompleted evt)
    {
        // Player Failed
        GameplayEventBus.Instance.Unsubscribe<GameplayEvents.CountdownTimerCompleted>(OnCountdownTimerCompleted);
        GameplayEventBus.Instance.Unsubscribe<GameplayEvents.QuestDataUpdated>(OnQuestDataUpdated);

        if (!_hasStartedSequence)
        {
            _countdownTimer.StopTimer();

            _dialogComponent.SetMessages(_failMessages);
            _spawnTutorialFlagStep.SetSuccess(false);

            StartSequence();
        }
    }

    private void OnQuestDataUpdated(GameplayEvents.QuestDataUpdated evt)
    {
        if (!evt.Data.questLevel0_10.CheckSuccess())
            return;

        // Player Success
        if (!_hasStartedSequence)
        {
            GameplayEventBus.Instance.Unsubscribe<GameplayEvents.CountdownTimerCompleted>(OnCountdownTimerCompleted);
            GameplayEventBus.Instance.Unsubscribe<GameplayEvents.QuestDataUpdated>(OnQuestDataUpdated);

            _countdownTimer.StopTimer();

            _dialogComponent.SetMessages(_successMessages);
            _spawnTutorialFlagStep.SetSuccess(true);

            StartSequence();
        }
    }

    public override void StartSequence()
    {
        _hasStartedSequence = true;

        // Subscribe to events
        _sequencer.OnSequenceStarted += Sequencer_OnSequenceStarted;
        _sequencer.OnStepStarted += Sequencer_OnStepStarted;
        _sequencer.OnStepCompleted += Sequencer_OnStepCompleted;
        _sequencer.OnSequenceCompleted += Sequencer_OnSequenceCompleted;

        // Start the sequence
        _sequencer.StartSequence();
    }

    private void Sequencer_OnSequenceCompleted(object sender, System.EventArgs e)
    {
        Debug.Log("All steps completed!");

        // Unsubscribe
        _sequencer.OnSequenceStarted -= Sequencer_OnSequenceStarted;
        _sequencer.OnStepStarted -= Sequencer_OnStepStarted;
        _sequencer.OnStepCompleted -= Sequencer_OnStepCompleted;
        _sequencer.OnSequenceCompleted -= Sequencer_OnSequenceCompleted;

        OnSequenceCompleted?.Invoke(this, EventArgs.Empty);
    }

    private void Sequencer_OnStepCompleted(object sender, int index)
    {
        Debug.Log($"Step {index} completed");
    }

    private void Sequencer_OnStepStarted(object sender, int index)
    {
        Debug.Log($"Step {index} started");
    }

    private void Sequencer_OnSequenceStarted(object sender, System.EventArgs e)
    {
        Debug.Log("Sequence started!");
    }
}

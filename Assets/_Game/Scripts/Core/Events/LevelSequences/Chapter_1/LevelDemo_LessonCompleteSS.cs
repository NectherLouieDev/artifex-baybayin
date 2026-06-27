using System;
using System.Collections.Generic;
using UnityEngine;
using static GameplayEvents;

public class LevelDemo_LessonCompleteSS : SequenceStepper
{
    public override event EventHandler OnSequenceCompleted;

    [SerializeField] private EventSequencer _sequencer;
    [SerializeField] private List<SequenceStep> _steps = new List<SequenceStep>();

    [Header("Level Config")]
    [SerializeField] private WaveManager _waveManager;
    [SerializeField] private TutorialLevelCanvasCompleteStep _tutorialLevelCanvasCompleteStep;
    [SerializeField] private TutorialCompleteLevelStep _tutorialCompleteStep;

    private bool _hasStarted = false;

    private void Start()
    {
        GameplayEventBus.Instance.Subscribe<GameplayEvents.BaseDeath>(OnBaseDeath);
        GameplayEventBus.Instance.Subscribe<GameplayEvents.AllWavesCompleted>(OnLevelCleared);

        // Add steps to sequencer
        _steps.ForEach(step =>
        {
            _sequencer.AddStep(step);
        });
    }

    private void Update()
    {
        //if (_waveManager.LevelCleared)
        //{
        //    GameplayEventBus.Instance.Invoke(new GameplayEvents.LevelCleared
        //    {
        //        WaveCount = 0
        //    });
        //}
    }

    private void OnBaseDeath(GameplayEvents.BaseDeath evt)
    {
        GameplayEventBus.Instance.Unsubscribe<GameplayEvents.BaseDeath>(OnBaseDeath);
        GameplayEventBus.Instance.Unsubscribe<GameplayEvents.AllWavesCompleted>(OnLevelCleared);

        _tutorialLevelCanvasCompleteStep.SetSuccess(false);
        _tutorialCompleteStep.SetSuccess(false);

        StartSequence();
    }

    private void OnLevelCleared(GameplayEvents.AllWavesCompleted evt)
    {
        GameplayEventBus.Instance.Unsubscribe<GameplayEvents.AllWavesCompleted>(OnLevelCleared);
        GameplayEventBus.Instance.Unsubscribe<GameplayEvents.BaseDeath>(OnBaseDeath);

        _tutorialLevelCanvasCompleteStep.SetSuccess(true);
        _tutorialCompleteStep.SetSuccess(true);

        StartSequence();
    }

    public override void StartSequence()
    {
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

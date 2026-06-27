using UnityEngine;

public class CompleteLevelStep : SequenceStep
{
    [SerializeField] private SaveManager _saveManager;
    [SerializeField] private ELevelIDs _levelIDToComplete;

    public override void Execute()
    {
        _saveManager.CompleteLevel(_levelIDToComplete);
        Complete();
    }
}

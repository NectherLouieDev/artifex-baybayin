using UnityEngine;

public class TutorialCompleteLevelStep : SequenceStep
{
    [SerializeField] private SaveManager _saveManager;
    [SerializeField] private ELevelIDs _levelIDToComplete;

    private bool _success = false;

    public void SetSuccess(bool success)
    {
        _success = success;
    }


    public override void Execute()
    {
        if (_success)
            _saveManager.CompleteLevel(_levelIDToComplete);
        
        Complete();
    }
}

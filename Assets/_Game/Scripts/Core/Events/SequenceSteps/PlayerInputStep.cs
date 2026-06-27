using UnityEngine;

public class PlayerInputStep : SequenceStep
{
    [SerializeField] private LevelManager _levelManager;
    [SerializeField] private MapManager _mapManager;
    [SerializeField] private bool _inputEnabled = true;

    private PlayerController _playerController;

    public override void Execute()
    {
        if (_levelManager != null)
        {
            if (_levelManager.Identity.TryGetComponent(out PlayerController controller))
            {
                controller.InputEnabled = _inputEnabled;
            }
        }

        if (_mapManager != null)
        {
            if (_mapManager.Identity.TryGetComponent(out PlayerController controller))
            {
                controller.InputEnabled = _inputEnabled;
            }
        }

        Complete();
    }
}

using PixeLadder.EasyTransition;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private SceneLoader _sceneLoader;
    [SerializeField] private bool _debugTransition = false;

    [Header("Events")]
    [SerializeField] private SequenceStepper _introSequenceStepper;
    [SerializeField] private SequenceStepper _lessonCompleteSequenceStepper;

    [Header("Player Spawn")]
    [SerializeField] private Transform _playerStart;
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Collider _confinerVolume;

    [Header("Data")]
    [SerializeField] private GameData_Player _playerData;

    private PlayerIdentity _playerIdentity;
    public PlayerIdentity Identity { get { return _playerIdentity; } }

    private void Awake()
    {
        // Spawn Player
        GameObject playerObject = Instantiate(_playerPrefab);
        playerObject.transform.SetPositionAndRotation(_playerStart.position, _playerStart.rotation);

        if (playerObject.TryGetComponent(out PlayerManager manager))
        {
            manager.SetConfinerVolume(_confinerVolume);

            _playerIdentity = manager.Identity;
        }

        if (!_debugTransition)
            SceneTransitioner.OnSceneLoaded += SceneTransitioner_OnSceneLoaded;
        else
            _introSequenceStepper.StartSequence();
    }

    private void SceneTransitioner_OnSceneLoaded()
    {
        SceneTransitioner.OnSceneLoaded -= SceneTransitioner_OnSceneLoaded;

        // Intro Event Sequencer
        _introSequenceStepper.StartSequence();
    }
}

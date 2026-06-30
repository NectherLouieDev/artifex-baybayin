using PixeLadder.EasyTransition;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private SceneLoader _sceneLoader;
    [SerializeField] private bool _debugTransition = false;

    [Header("Events")]
    [SerializeField] private SequenceStepper _introSequenceStepper;
    [SerializeField] private SequenceStepper _lessonCompleteSequenceStepper;
    [SerializeField] private ESceneID _targetSceneIDOnComplete = ESceneID.MapSelection;

    [Header("Player Spawn")]
    [SerializeField] private Transform _playerStart;
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Collider _confinerVolume;

    [Header("Data")]
    [SerializeField] private GameData_Player _playerData;
    [SerializeField] private QuestData _questData;
    
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
            SceneTransitioner.Instance.OnSceneLoaded.AddListener(SceneTransitioner_OnSceneLoaded);
        else
            _introSequenceStepper.StartSequence();
    }

    private void Start()
    {
        GameplayEventBus.Instance.Subscribe<GameplayEvents.CoinCollected>(OnCoinCollected);
        GameplayEventBus.Instance.Subscribe<GameplayEvents.AllWavesCompleted>(OnAllWavesCompleted);
        
        // should be on a quest manager
        GameplayEventBus.Instance.Subscribe<GameplayEvents.AssemblySpawnedItem>(OnAssemblySpawnedItem);
        GameplayEventBus.Instance.Subscribe<GameplayEvents.BlockDestroyed<DestructibleBlock>>(OnBlockDestroyed);
        GameplayEventBus.Instance.Subscribe<GameplayEvents.BlockRepaired<DestructibleBlock>>(OnBlockRepaired);

        if (_lessonCompleteSequenceStepper != null)
            _lessonCompleteSequenceStepper.OnSequenceCompleted += LessonCompleteSequenceStepper_OnSequenceCompleted;
    }

    private void SceneTransitioner_OnSceneLoaded()
    {
        SceneTransitioner.Instance.OnSceneLoaded.RemoveListener(SceneTransitioner_OnSceneLoaded);

        // Intro Event Sequencer
        _introSequenceStepper.StartSequence();
    }

    private void LessonCompleteSequenceStepper_OnSequenceCompleted(object sender, System.EventArgs e)
    {
        _sceneLoader.ChangeScene(_targetSceneIDOnComplete);
    }

    private void OnCoinCollected(GameplayEvents.CoinCollected evt)
    {
        _playerData.coinCount++;

        GameplayEventBus.Instance.Invoke(new GameplayEvents.CoinCountUpdated
        {
            CoinCount = _playerData.coinCount
        });
    }

    private void OnAllWavesCompleted(GameplayEvents.AllWavesCompleted evt)
    {
        if(_playerIdentity.TryGetComponent(out PlayerController controller))
        {
            controller.InputEnabled = false;
        }
    }

    private void OnAssemblySpawnedItem(GameplayEvents.AssemblySpawnedItem evt)
    {
        if (!_questData)
            return;

        _questData.questLevel0_6.IncrementCount(evt.Amount);

        _questData.questLevel0_6.CheckSuccess();

        GameplayEventBus.Instance.Invoke(new GameplayEvents.QuestDataUpdated
        { 
            Data = _questData 
        });
    }

    private void OnBlockDestroyed(GameplayEvents.BlockDestroyed<DestructibleBlock> evt)
    {
        if (!_questData)
            return;

        _questData.questLevel0_8.IncrementCount(1);
        _questData.questLevel0_8.CheckSuccess();

        GameplayEventBus.Instance.Invoke(new GameplayEvents.QuestDataUpdated
        {
            Data = _questData
        });
    }

    private void OnBlockRepaired(GameplayEvents.BlockRepaired<DestructibleBlock> evt)
    {
        if (!_questData)
            return;

        _questData.questLevel0_10.IncrementCount(1);
        _questData.questLevel0_10.CheckSuccess();

        Debug.Log("_questData.questLevel0_10.CountString() -> " + _questData.questLevel0_10.CountString());

        GameplayEventBus.Instance.Invoke(new GameplayEvents.QuestDataUpdated
        {
            Data = _questData
        });
    }
}

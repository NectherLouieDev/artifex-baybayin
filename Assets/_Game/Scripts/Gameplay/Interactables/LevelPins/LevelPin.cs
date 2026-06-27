using UnityEngine;

public class LevelPin : InteractableObject
{
    [SerializeField] private SaveManager _saveManager;
    [SerializeField] private SceneLoader _sceneLoader;

    [SerializeField] private ESceneID _targetSceneID;
    [SerializeField] private ELevelIDs _levelID;
    [SerializeField] private GameObject _flagCompleteObject;

    private void Awake()
    {
        _saveManager = FindFirstObjectByType<SaveManager>();
        _sceneLoader = FindFirstObjectByType<SceneLoader>();
    }

    private void Start()
    {
        if (_saveManager.IsLevelComplete(_levelID))
        {
            _flagCompleteObject.SetActive(true);
        }
    }

    public override void Interact(Transform interactor)
    {
        base.Interact(interactor);

        _sceneLoader.ChangeScene(_targetSceneID);
    }
}

using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenuController : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private SceneLoader _sceneLoader;
    [SerializeField] private SaveManager _saveManager;

    [Header("Input")]
    [SerializeField] private InputAction _upDownInputAction;
    [SerializeField] private InputAction _selectInputAction;
    [SerializeField] private RectTransform _selectorTransform;

    [Header("Arrays")]
    [SerializeField] private List<SelectableItem> _selectableItems = new List<SelectableItem>();
    [SerializeField] private List<float> _selectableTargetY = new List<float>();
    [SerializeField] private List<ESceneID> _selectableTargetScenes = new List<ESceneID>();

    private bool _isMoving = false;
    private int _selectionIndex = 0;

    private void Awake()
    {
        _saveManager = FindFirstObjectByType<SaveManager>();
    }

    private void Start()
    {
        _upDownInputAction.started += UpDownInputAction_started;
        _upDownInputAction.Enable();

        _selectInputAction.started += SelectInputAction_started;
        _selectInputAction.Enable();
    }

    private void SelectInputAction_started(InputAction.CallbackContext obj)
    {
        if (_isMoving) 
            return;

        Debug.Log($"Selected item {_selectionIndex}: {_selectableItems[_selectionIndex].name}");

        ESceneID targetSceneID = _selectableTargetScenes[_selectionIndex];
       
        switch(targetSceneID)
        {
            case ESceneID.MapSelection:
            case ESceneID.Chapter_0_Level_1:

                if (_saveManager.CanSkipChapter0Level1)
                {
                    targetSceneID = ESceneID.MapSelection;
                }

                _sceneLoader.ChangeScene(targetSceneID);
                break;
            case ESceneID.DEMO_LEVEL:
                _sceneLoader.ChangeScene(ESceneID.DEMO_LEVEL);
                break;
            case ESceneID.SettingsMenu:
                Debug.Log("No Settings Menu yet");
                break;
            case ESceneID.QuitMenu:
                Application.Quit();
                break;
        }
    }

    private void UpDownInputAction_started(InputAction.CallbackContext obj)
    {
        if (_isMoving)
            return;

        Vector2 val = obj.ReadValue<Vector2>();
        if (val.y > 0) // Up
        {
            _selectionIndex = (_selectionIndex - 1 + _selectableItems.Count) % _selectableItems.Count;
            MoveSelector();
        }
        else if (val.y < 0) // Down
        {
            _selectionIndex = (_selectionIndex + 1) % _selectableItems.Count;
            MoveSelector();
        }

        Debug.Log("selectionIndex -> " + _selectionIndex);
        UpdateSelectionHighlight();
    }

    private void MoveSelector()
    {
        _isMoving = true;
        //float targetY = (_selectableItems[_selectionIndex].transform as RectTransform).anchoredPosition.y;
        float targetY = _selectableTargetY[_selectionIndex];
        _selectorTransform.DOAnchorPosY(targetY, 0.25f)
            .OnComplete(() => _isMoving = false);
    }

    private void UpdateSelectionHighlight()
    {
        // Update visual feedback for selected item
        for (int i = 0; i < _selectableItems.Count; i++)
        {
            if (_selectableItems[i] != null)
            {
                _selectableItems[i].SetHighlight(i == _selectionIndex);
            }
        }
    }
}

using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenuHandler : MonoBehaviour
{
    [SerializeField] private InputAction _mainMenuInputAction;
    [SerializeField] private InputAction _exitInputAction;
    [SerializeField] private SceneLoader _sceneLoader;

    [Header("Feedback")]
    [SerializeField] private MMFeedbacks _startFeedback;

    private GGTimer _delayTimer;
    private GGTimer _exitDelayTimer;

    private void Start()
    {
        _delayTimer = gameObject.AddComponent<GGTimer>();
        _delayTimer.OnTimerCompleted += DelayTimer_OnTimerCompleted;

        _mainMenuInputAction.performed += MainMenuInputAction_performed;
        _mainMenuInputAction.Enable();

        _exitDelayTimer = gameObject.AddComponent<GGTimer>();
        _exitDelayTimer.timerId = "Exit Delay Timer";
        _exitDelayTimer.OnTimerCompleted += ExitDelayTimer_OnTimerCompleted;
        
        _exitInputAction.performed += ExitInputAction_performed;
        _exitInputAction.Disable();

        _exitDelayTimer.StartTimer(0.5f, 1);
    }

    private void ExitDelayTimer_OnTimerCompleted(object sender, GGTimer e)
    {
        _exitInputAction.Enable();
    }

    private void ExitInputAction_performed(InputAction.CallbackContext obj)
    {
        Debug.Log("Application.Quit()");

        QuitGame();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void DelayTimer_OnTimerCompleted(object sender, GGTimer e)
    {
        _mainMenuInputAction.performed -= MainMenuInputAction_performed;
        _mainMenuInputAction.Enable();

        _exitInputAction.performed -= ExitInputAction_performed;
        _exitInputAction.Disable();

        _sceneLoader.ChangeScene(ESceneID.MapSelection);
    }

    private void MainMenuInputAction_performed(InputAction.CallbackContext obj)
    {
        _mainMenuInputAction.performed -= MainMenuInputAction_performed;

        _startFeedback?.PlayFeedbacks();

        _delayTimer.StartTimer(0.1f, 1);
    }
}

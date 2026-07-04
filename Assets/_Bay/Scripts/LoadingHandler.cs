using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LoadingHandler : MonoBehaviour
{
    [SerializeField] private InputAction _continueInputAction;
    [SerializeField] private SceneLoader _sceneLoader;
    [SerializeField] private GameObject _questContinue;
    [SerializeField] private GameObject _questPanel;
    [SerializeField] private GameObject _loadingPanel;
    [SerializeField] private Slider _progressSlider;

    private GGTimer _delayTimer;
    private GGTimer _loadingTimer;

    private void Start()
    {
        _questPanel.SetActive(true);
        _loadingPanel.SetActive(false);
        _questContinue.SetActive(false);

        _loadingTimer = gameObject.AddComponent<GGTimer>();
        _loadingTimer.timerId = "Loading Timer";
        _loadingTimer.OnTimerUpdated += LoadingTimer_OnTimerUpdated;
        _loadingTimer.OnTimerCompleted += LoadingTimer_OnTimerCompleted;

        _continueInputAction.performed += ContinueInputAction_performed;
        //_continueInputAction.Enable();

        _delayTimer = gameObject.AddComponent<GGTimer>();
        _delayTimer.timerId = "Delay Timer";
        _delayTimer.OnTimerCompleted += DelayTimer_OnTimerCompleted;
        _delayTimer.StartTimer(3, 1);
    }

    private void DelayTimer_OnTimerCompleted(object sender, GGTimer e)
    {
        _questContinue.SetActive(true);

        _continueInputAction.Enable();
    }

    private void ContinueInputAction_performed(InputAction.CallbackContext obj)
    {
        _continueInputAction.performed -= ContinueInputAction_performed;
        _continueInputAction.Disable();

        _questPanel.SetActive(false);
        _loadingPanel.SetActive(true);

        float loadTime = Random.Range(5, 10);

        _loadingTimer.StartTimer(loadTime, 1);
    }

    private void LoadingTimer_OnTimerCompleted(object sender, GGTimer e)
    {
        _sceneLoader.ChangeScene(ESceneID.DEMO_LEVEL);
    }

    private void LoadingTimer_OnTimerUpdated(object sender, GGTimer e)
    {
        _progressSlider.value = e.GetCurrentTime() / e.TargetTime;
    }
}

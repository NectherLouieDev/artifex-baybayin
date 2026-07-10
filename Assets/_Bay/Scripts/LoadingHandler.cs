using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LoadingHandler : MonoBehaviour
{
    [SerializeField] private InputAction _continueInputAction;
    [SerializeField] private SceneLoader _sceneLoader;
    [SerializeField] private GameObject _questContinue;
    [SerializeField] private GameObject _questPanel;
    [SerializeField] private CanvasGroup _questPanelCanvas;
    [SerializeField] private GameObject _loadingPanel;
    [SerializeField] private Slider _progressSlider;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip bookClip;

    private GGTimer _delayTimer;
    private GGTimer _loadingTimer;

    private void Start()
    {
        _questPanel.SetActive(false);
        _loadingPanel.SetActive(false);
        _questContinue.SetActive(false);

        _loadingTimer = gameObject.AddComponent<GGTimer>();
        _loadingTimer.timerId = "Loading Timer";
        _loadingTimer.OnTimerUpdated += LoadingTimer_OnTimerUpdated;
        _loadingTimer.OnTimerCompleted += LoadingTimer_OnTimerCompleted;

        _continueInputAction.performed += ContinueInputAction_performed;
        //_continueInputAction.Enable();

        _questPanelCanvas.alpha = 0;
        _questPanelCanvas.DOFade(1, 0.25f)
            .OnStart(() => {
                sfxSource.PlayOneShot(bookClip);
                _questPanel.SetActive(true);
            })
            .OnComplete(() => {
                _questPanelCanvas.alpha = 1;
                _questPanel.SetActive(true);
            });

        _delayTimer = gameObject.AddComponent<GGTimer>();
        _delayTimer.timerId = "Delay Timer";
        _delayTimer.OnTimerCompleted += DelayTimer_OnTimerCompleted;
        _delayTimer.StartTimer(3.5f, 1);
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

        _questPanelCanvas.alpha = 1;
        _questPanelCanvas.DOFade(0, 0.25f)
            .OnStart(() => {
                sfxSource.PlayOneShot(bookClip);
                _questPanel.SetActive(true);
            })
            .OnComplete(() => {
                _questPanelCanvas.alpha = 0;
                _questPanel.SetActive(false);
            });

        _loadingPanel.SetActive(true);

        float loadTime = Random.Range(6, 12);

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

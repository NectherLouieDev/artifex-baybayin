using Microlight.MicroBar;
using TMPro;
using UnityEngine;

public class UIWaveSection : MonoBehaviour
{
    [SerializeField] private MicroBar _waveTimerBar;
    [SerializeField] private TMP_Text _waveText;

    private bool _isBarInitialized = false;

    private void Start()
    {
        //GameplayEventBus.Instance.Subscribe<GameplayEvents.PreWaveTimerUpdated>(OnPreWaveTimerUpdated);
        //GameplayEventBus.Instance.Subscribe<GameplayEvents.WaveStatePreparing>(OnWavePreparing);
    }

    private void OnEnable()
    {
        GameplayEventBus.Instance.Subscribe<GameplayEvents.PreWaveTimerUpdated>(OnPreWaveTimerUpdated);
        GameplayEventBus.Instance.Subscribe<GameplayEvents.WaveStatePreparing>(OnWavePreparing);
    }

    private void OnPreWaveTimerUpdated(GameplayEvents.PreWaveTimerUpdated evt)
    {
        float currentTime = evt.CurrentTime;

        if (currentTime > 0)
        {
            float targetTime = evt.TargetTime;

            Debug.Log(currentTime + " / " + targetTime);

            if (!_isBarInitialized)
            {
                _waveTimerBar.Initialize(currentTime);
                _isBarInitialized = true;
            }
        
            _waveTimerBar.UpdateBar(currentTime);
        }
        else
        {
            _isBarInitialized= false;
        }

    }

    private void OnWavePreparing(GameplayEvents.WaveStatePreparing evt)
    {
        _isBarInitialized = false;

        int currentWaveNum = evt.CurrentWaveNumber;
        int maxWaveNum = evt.MaxWaveNumber;

        _waveText.text = currentWaveNum.ToString() + "/" + maxWaveNum.ToString();
    }
}

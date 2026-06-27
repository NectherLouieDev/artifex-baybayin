using TMPro;
using UnityEngine;

public class UICountdownTimer : MonoBehaviour
{
    [SerializeField] private TMP_Text _timerText;

    private void Start()
    {
        GameplayEventBus.Instance.Subscribe<GameplayEvents.CountdownTimerUpdated>(OnCountdownTimerUpdated);
    }

    private void OnCountdownTimerUpdated(GameplayEvents.CountdownTimerUpdated evt)
    {
        _timerText.text = evt.CurrentTime.ToString("000");
    }
}

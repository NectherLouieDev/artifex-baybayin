using DG.Tweening;
using Febucci.TextAnimatorForUnity;
using UnityEngine;
using System.Collections.Generic;

public class TriggerDialogBox : MonoBehaviour
{
    [Multiline]
    [SerializeField] private string[] _messages;
    [SerializeField] private TypewriterComponent _textComponent;
    [SerializeField] private CanvasGroup _bubbleCanvasGroup;
    [SerializeField] private GGTimer _dialogTimer;
    [SerializeField] private float _messageDisplayDuration = 3f;
    [SerializeField] private float _fadeDuration = 0.25f;

    private int _currentMessageIndex = 0;
    private bool _isShowingMessage = false;

    private void Start()
    {
        _textComponent.ShowText("");
        _bubbleCanvasGroup.alpha = 0;

        if (_dialogTimer == null)
        {
            _dialogTimer = gameObject.AddComponent<GGTimer>();
            _dialogTimer.timerId = "Dialog Timer";
            if (_dialogTimer == null)
            {
                Debug.LogError("GGTimer component not found on " + gameObject.name);
            }
        }

        // Subscribe to timer events
        if (_dialogTimer != null)
        {
            _dialogTimer.OnTimerCompleted += OnMessageDisplayComplete;
            _dialogTimer.OnTimerLoop += OnMessageLoop;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from timer events
        if (_dialogTimer != null)
        {
            _dialogTimer.OnTimerCompleted -= OnMessageDisplayComplete;
            _dialogTimer.OnTimerLoop -= OnMessageLoop;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isShowingMessage && _messages != null && _messages.Length > 0)
        {
            _currentMessageIndex = 0;
            _bubbleCanvasGroup.DOFade(1, _fadeDuration)
                .OnComplete(OnBubbleFadeInComplete);
        }
    }

    private void OnBubbleFadeInComplete()
    {
        ShowCurrentMessage();
    }

    private void ShowCurrentMessage()
    {
        if (_currentMessageIndex < _messages.Length)
        {
            _isShowingMessage = true;
            _textComponent.ShowText(_messages[_currentMessageIndex]);

            // Start timer for this message
            if (_dialogTimer != null)
            {
                // Use loops = number of remaining messages (including current)
                int remainingMessages = _messages.Length - _currentMessageIndex;
                _dialogTimer.StartTimer(_messageDisplayDuration, remainingMessages);
            }
            else
            {
                Debug.LogError("GGTimer is null, cannot display dialog");
            }
        }
        else
        {
            // All messages shown, fade out
            FadeOutAndComplete();
        }
    }

    private void OnMessageLoop(object sender, GGTimer timer)
    {
        // Move to next message when timer loops
        _currentMessageIndex++;
        ShowCurrentMessage();
    }

    private void OnMessageDisplayComplete(object sender, GGTimer timer)
    {
        // All messages have been displayed
        _isShowingMessage = false;
        FadeOutAndComplete();
    }

    private void FadeOutAndComplete()
    {
        _bubbleCanvasGroup.DOFade(0, _fadeDuration)
            .OnComplete(OnDialogComplete);
    }

    private void OnDialogComplete()
    {
        // Clear text and disable trigger collider
        _textComponent.ShowText("");
        GetComponent<Collider>().enabled = false;
        _currentMessageIndex = 0;
        _isShowingMessage = false;
    }

    // Public methods for external control
    public void SkipCurrentMessage()
    {
        if (_isShowingMessage && _dialogTimer != null && _dialogTimer.IsRunning())
        {
            _dialogTimer.StopTimer();
            OnMessageLoop(_dialogTimer, _dialogTimer);
        }
    }

    public void ResetDialog()
    {
        _currentMessageIndex = 0;
        _isShowingMessage = false;
        _bubbleCanvasGroup.alpha = 0;
        _textComponent.ShowText("");

        if (_dialogTimer != null)
        {
            _dialogTimer.StopTimer();
        }

        GetComponent<Collider>().enabled = true;
    }

    // Optional: Method to set messages dynamically
    public void SetMessages(string[] messages)
    {
        _messages = messages;
        ResetDialog();
    }
}
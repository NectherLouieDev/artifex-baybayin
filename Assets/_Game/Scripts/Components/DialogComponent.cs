using Febucci.TextAnimatorForUnity;
using System;
using UnityEngine;

public class DialogComponent : MonoBehaviour
{
    public event EventHandler OnDialogComplete;

    [Multiline]
    [SerializeField] private string[] _messages;
    [SerializeField] private TypewriterComponent _textComponent;
    [SerializeField] private GGTimer _dialogTimer;
    [SerializeField] private float _messageDisplayDuration = 3f;

    private int _currentMessageIndex = 0;
    private bool _isShowingMessage = false;

    public void SetupDialog()
    {
        _textComponent.ShowText("");

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
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from timer events
        if (_dialogTimer != null)
        {
            _dialogTimer.OnTimerCompleted -= OnMessageDisplayComplete;
        }
    }

    private void ShowCurrentMessage()
    {
        _isShowingMessage = true;
        _textComponent.ShowText(_messages[_currentMessageIndex]);

        // Start timer for this message
        if (_dialogTimer != null)
        {
            // Use loops = number of remaining messages (including current)
            int remainingMessages = _messages.Length - _currentMessageIndex;
            _dialogTimer.StartTimer(_messageDisplayDuration, 1);
        }
        else
        {
            Debug.LogError("GGTimer is null, cannot display dialog");
        }
    }

    private void OnMessageDisplayComplete(object sender, GGTimer timer)
    {
        _textComponent.onTextDisappeared.AddListener(OnTextDisappeared);
        _textComponent.StartDisappearingText();
    }

    private void OnTextDisappeared()
    {
        _textComponent.onTextDisappeared.RemoveListener(OnTextDisappeared);

        _currentMessageIndex++;
        if (_currentMessageIndex < _messages.Length)
        {
            ShowCurrentMessage();
        }
        else
        {
            CompleteDialog();
        }
    }

    private void CompleteDialog()
    {
        _isShowingMessage = false;

        _textComponent.ShowText("");

        OnDialogComplete.Invoke(this, EventArgs.Empty);
    }

    // Public methods for external control
    public void SkipCurrentMessage()
    {
        if (_isShowingMessage && _dialogTimer != null && _dialogTimer.IsRunning())
        {
            _dialogTimer.StopTimer();
            //OnMessageLoop(_dialogTimer, _dialogTimer);
        }
    }

    public void ResetDialog()
    {
        _currentMessageIndex = 0;
        _isShowingMessage = false;
        _textComponent.ShowText("");

        if (_dialogTimer != null)
        {
            _dialogTimer.StopTimer();
        }

        GetComponent<Collider>().enabled = true;
    }

    public void StartDialog()
    {
        if (!_isShowingMessage && _messages != null && _messages.Length > 0)
        {
            _currentMessageIndex = 0;
            ShowCurrentMessage();
        }
    }

    public void SetMessages(string[] messages)
    {
        _messages = messages;
    }
}

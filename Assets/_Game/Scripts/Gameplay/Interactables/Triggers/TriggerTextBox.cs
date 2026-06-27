using DG.Tweening;
using Febucci.TextAnimatorForUnity;
using UnityEngine;

public class TriggerTextBox : MonoBehaviour
{
    [Multiline]
    [SerializeField] private string _message;
    [SerializeField] private TypewriterComponent _textComponent;
    [SerializeField] private CanvasGroup _bubbleCanvasGroup;

    private void Start()
    {
        _textComponent.ShowText("");
        _bubbleCanvasGroup.alpha = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        _bubbleCanvasGroup.DOFade(1, 0.25f)
            .OnComplete(OnBubbleFadeInComplete);
    }

    private void OnBubbleFadeInComplete()
    { 
        _textComponent.ShowText($"{_message}");

        GetComponent<Collider>().enabled = false;
    }
}

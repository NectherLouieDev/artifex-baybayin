using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class SelectableItem : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private string _itemID;
    [SerializeField] private UnityEvent _onSelected;
    
    [Header("Visual Feedback")]
    [SerializeField] private TextMeshProUGUI _labelText;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _highlightImage;
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _highlightColor = Color.yellow;
    
    public void Trigger()
    {
        _onSelected?.Invoke();
    }
    
    public void SetHighlight(bool highlighted)
    {
        //if (_labelText != null)
        //{
        //    _labelText.color = highlighted ? _highlightColor : _normalColor;
        //}
        
        if (_highlightImage != null)
        {
            _highlightImage.color = highlighted ? _highlightColor : _normalColor;
        }
    }
}
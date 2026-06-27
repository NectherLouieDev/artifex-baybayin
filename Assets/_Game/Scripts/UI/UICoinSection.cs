using TMPro;
using UnityEngine;

public class UICoinSection : MonoBehaviour
{
    [SerializeField] private TMP_Text _coinText;

    private void Start()
    {
        GameplayEventBus.Instance.Subscribe<GameplayEvents.CoinCountUpdated>(OnCoinCountUpdated);
    }

    private void OnCoinCountUpdated(GameplayEvents.CoinCountUpdated evt)
    {
        _coinText.text = evt.CoinCount.ToString("D6");
    }
}

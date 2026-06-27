using TMPro;
using UnityEngine;

public class UIBombsCookedSection : MonoBehaviour
{
    [SerializeField] private TMP_Text _bombCountText;

    private void Start()
    {
        GameplayEventBus.Instance.Subscribe<GameplayEvents.QuestDataUpdated>(OnQuestDataUpdated);
    }

    private void OnQuestDataUpdated(GameplayEvents.QuestDataUpdated evt)
    {
        _bombCountText.text = evt.Data.questLevel0_6.CountString();
    }
}

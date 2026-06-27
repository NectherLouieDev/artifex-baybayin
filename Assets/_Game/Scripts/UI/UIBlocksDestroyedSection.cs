using TMPro;
using UnityEngine;

public class UIBlocksDestroyedSection : MonoBehaviour
{
    [SerializeField] private TMP_Text _blockCountText;

    private void Start()
    {
        GameplayEventBus.Instance.Subscribe<GameplayEvents.QuestDataUpdated>(OnQuestDataUpdated);
    }

    private void OnQuestDataUpdated(GameplayEvents.QuestDataUpdated evt)
    {
        _blockCountText.text = evt.Data.questLevel0_8.CountString();
    }
}

using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

public class InteractableHighlighter : MonoBehaviour
{
    [SerializeField] private GameObject interactableGameObject;
    [SerializeField] private GameObject[] highlights;

    private IInteractable interactableObject;
    private GameplayEventBus GEB;

    private void Start()
    {
        interactableObject = interactableGameObject.GetComponent<IInteractable>();

        GEB = GameplayEventBus.Instance;

        if (GEB != null)
        {
            GEB.Subscribe<GameplayEvents.InteractableChanged>(OnInteractableChanged);
        }
    }

    private void OnDisable()
    {
        if (GEB != null)
        {
            GEB.Unsubscribe<GameplayEvents.InteractableChanged>(OnInteractableChanged);
        }
    }

    public void OnInteractableChanged(GameplayEvents.InteractableChanged evt)
    {
        // Check if a player is attemptin to highlight
        //List<PlayerIdentity> players = PlayerManager.Instance.GetPlayers();

        //bool playerIsAttemptingToHighlight = false;
        //int playerIndex = -1;

        //foreach (PlayerIdentity p in players)
        //{
        //    IInteractable c = p.GetComponent<PlayerInteractionHandler>().CurrentInteractableObject;

        //    if (c == interactableObject)
        //    {
        //        playerIsAttemptingToHighlight = true;
        //        playerIndex = p.PlayerIndex;
        //        break;
        //    }
        //}

        //if (args.SelectedObject != null || playerIsAttemptingToHighlight)
        //{
        //    if (args.PlayerIndex == playerIndex)
        //        Show();
        //}
        //else
        //{
        //    Hide();
        //}

        //IInteractable c = p.GetComponent<InteractionHandler>().CurrentInteractableObject;
        if (evt.SelectedObject != null && evt.SelectedObject == interactableObject)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        if (highlights.Length > 0)
        {
            foreach (GameObject g in highlights)
            {
                g.SetActive(true);
            }
        }
    }

    private void Hide()
    {
        if (highlights.Length > 0)
        {
            foreach (GameObject g in highlights)
            {
                g.SetActive(false);
            }
        }
    }
}

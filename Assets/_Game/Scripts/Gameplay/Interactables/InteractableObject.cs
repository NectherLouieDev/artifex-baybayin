using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    public virtual void Interact(Transform interactor) { }
}

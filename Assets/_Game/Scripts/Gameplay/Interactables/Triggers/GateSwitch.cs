using DG.Tweening;
using UnityEngine;

public class GateSwitch : InteractableObject
{
    [SerializeField] private Transform _leverTransform;
    [SerializeField] private Transform _doorTransform;

    public override void Interact(Transform interactor)
    {
        base.Interact(interactor);

        _leverTransform.DOShakeRotation(0.2f); // Change this

        _doorTransform.DOLocalMoveY(-5f, 0.5f)
            .SetEase(Ease.InOutBack)
            .SetDelay(0.1f);
    }
}

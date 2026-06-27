using UnityEngine;

public class DialogStep : SequenceStep
{
    [SerializeField] private DialogComponent _dialogComponent;

    public override void Execute()
    {
        _dialogComponent.OnDialogComplete += DialogComponent_OnDialogComplete;
        _dialogComponent.SetupDialog();
        _dialogComponent.StartDialog();
    }

    private void DialogComponent_OnDialogComplete(object sender, System.EventArgs e)
    {
        Complete();
    }
}

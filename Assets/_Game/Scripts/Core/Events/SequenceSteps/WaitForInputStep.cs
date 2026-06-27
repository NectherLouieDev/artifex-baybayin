using UnityEngine;
using UnityEngine.InputSystem;

public class WaitForInputStep : SequenceStep
{
    [SerializeField] private InputAction _inputAction;

    public override void Execute()
    {
        _inputAction.performed += InputAction_performed;
        _inputAction.Enable();
    }

    private void InputAction_performed(InputAction.CallbackContext obj)
    {
        _inputAction.Disable();
        _inputAction.performed -= InputAction_performed;
        
        Complete();
    }
}

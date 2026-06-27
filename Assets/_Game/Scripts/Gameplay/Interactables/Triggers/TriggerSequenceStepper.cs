using UnityEngine;

public class TriggerSequenceStepper : MonoBehaviour
{
    [SerializeField] private SequenceStepper _stepper;

    private void OnTriggerEnter(Collider other)
    {
        _stepper.OnSequenceCompleted += Stepper_OnSequenceCompleted;
        _stepper.StartSequence();
    }

    private void Stepper_OnSequenceCompleted(object sender, System.EventArgs e)
    {
        GetComponent<Collider>().enabled = false;
    }
}

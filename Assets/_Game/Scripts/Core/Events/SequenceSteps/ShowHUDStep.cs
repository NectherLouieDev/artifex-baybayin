using System.Collections.Generic;
using UnityEngine;

public class ShowHUDStep : SequenceStep
{
    [SerializeField] private List<GameObject> _hudList = new List<GameObject>();
    [SerializeField] private bool _enable;

    public override void Execute()
    {
        foreach (GameObject go in _hudList)
        {
            go.SetActive(_enable);
        }

        Complete();
    }
}
